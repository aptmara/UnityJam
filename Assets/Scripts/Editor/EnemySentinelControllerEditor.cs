using UnityEngine;
using UnityEditor;
using UnityJam.Enemies;

namespace UnityJam.Editors
{
    [CustomEditor(typeof(EnemySentinelController))]
    public class EnemySentinelControllerEditor : Editor
    {
        // プロパティ
        SerializedProperty movementType;
        SerializedProperty moveSpeed;
        SerializedProperty waitTime;

        SerializedProperty lightMode;
        SerializedProperty flickerSpeed;

        SerializedProperty lookAngleStep;
        SerializedProperty lookWaitTime;
        SerializedProperty lookRotateSpeed;

        SerializedProperty linearDistance;
        SerializedProperty linearDirection;

        SerializedProperty circleRadiusX;
        SerializedProperty circleRadiusZ;
        SerializedProperty clockwise;

        SerializedProperty figureEightSize;
        SerializedProperty wanderRadius;

        SerializedProperty simpleWaypoints;
        SerializedProperty currentGraphWaypoint;

        SerializedProperty targetTag;
        SerializedProperty viewLight;
        SerializedProperty obstacleMask;
        SerializedProperty viewRadius;
        SerializedProperty viewAngle;
        SerializedProperty eyeHeight;
        SerializedProperty weightSensitivity;

        SerializedProperty rushSpeed;
        SerializedProperty roarDuration;
        SerializedProperty attackScaleMultiplier;
        SerializedProperty playerEyeHeight;
        SerializedProperty scriptsToDisable;

        SerializedProperty walkModel;
        SerializedProperty attackModel;
        SerializedProperty animator;

        private void OnEnable()
        {
            movementType = serializedObject.FindProperty("movementType");
            moveSpeed = serializedObject.FindProperty("moveSpeed");
            waitTime = serializedObject.FindProperty("waitTime");

            lightMode = serializedObject.FindProperty("lightMode");
            flickerSpeed = serializedObject.FindProperty("flickerSpeed");

            lookAngleStep = serializedObject.FindProperty("lookAngleStep");
            lookWaitTime = serializedObject.FindProperty("lookWaitTime");
            lookRotateSpeed = serializedObject.FindProperty("lookRotateSpeed");

            linearDistance = serializedObject.FindProperty("linearDistance");
            linearDirection = serializedObject.FindProperty("linearDirection");

            circleRadiusX = serializedObject.FindProperty("circleRadiusX");
            circleRadiusZ = serializedObject.FindProperty("circleRadiusZ");
            clockwise = serializedObject.FindProperty("clockwise");

            figureEightSize = serializedObject.FindProperty("figureEightSize");
            wanderRadius = serializedObject.FindProperty("wanderRadius");

            simpleWaypoints = serializedObject.FindProperty("simpleWaypoints");
            currentGraphWaypoint = serializedObject.FindProperty("currentGraphWaypoint");

            targetTag = serializedObject.FindProperty("targetTag");
            viewLight = serializedObject.FindProperty("viewLight");
            obstacleMask = serializedObject.FindProperty("obstacleMask");
            viewRadius = serializedObject.FindProperty("viewRadius");
            viewAngle = serializedObject.FindProperty("viewAngle");
            eyeHeight = serializedObject.FindProperty("eyeHeight");
            weightSensitivity = serializedObject.FindProperty("weightSensitivity");

            rushSpeed = serializedObject.FindProperty("rushSpeed");
            roarDuration = serializedObject.FindProperty("roarDuration");
            attackScaleMultiplier = serializedObject.FindProperty("attackScaleMultiplier");
            playerEyeHeight = serializedObject.FindProperty("playerEyeHeight");
            scriptsToDisable = serializedObject.FindProperty("scriptsToDisable");

            walkModel = serializedObject.FindProperty("walkModel");
            attackModel = serializedObject.FindProperty("attackModel");
            animator = serializedObject.FindProperty("animator");
        }

        // =========================================================
        // Scene View 操作
        // =========================================================
        private void OnSceneGUI()
        {
            EnemySentinelController sentinel = (EnemySentinelController)target;
            Vector3 pos = sentinel.transform.position;

            // 1. 視界ハンドル
            Handles.color = Color.green;
            EditorGUI.BeginChangeCheck();
            float newRadius = Handles.RadiusHandle(Quaternion.identity, pos + Vector3.up * eyeHeight.floatValue, viewRadius.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(sentinel, "Change View Radius");
                viewRadius.floatValue = newRadius;
                serializedObject.ApplyModifiedProperties();
            }

            Handles.color = new Color(0, 1, 0, 0.3f);
            Vector3 angleLeft = Quaternion.Euler(0, -viewAngle.floatValue / 2, 0) * sentinel.transform.forward;
            Handles.DrawSolidArc(pos + Vector3.up * eyeHeight.floatValue, Vector3.up, angleLeft, viewAngle.floatValue, viewRadius.floatValue);

            // 2. 移動範囲ハンドル
            EnemySentinelController.MovementType mode = (EnemySentinelController.MovementType)movementType.enumValueIndex;

            if (mode == EnemySentinelController.MovementType.RandomWander)
            {
                Handles.color = Color.cyan;
                EditorGUI.BeginChangeCheck();
                float newWanderR = Handles.RadiusHandle(Quaternion.identity, pos, wanderRadius.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(sentinel, "Change Wander Radius");
                    wanderRadius.floatValue = newWanderR;
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else if (mode == EnemySentinelController.MovementType.PatrolCircular)
            {
                Handles.color = Color.yellow;
                EditorGUI.BeginChangeCheck();
                float newCircR = Handles.RadiusHandle(Quaternion.identity, pos, circleRadiusX.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(sentinel, "Change Circle Radius");
                    circleRadiusX.floatValue = newCircR;
                    circleRadiusZ.floatValue = newCircR;
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // =========================================================
        // Inspector GUI
        // =========================================================
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
            boxStyle.margin = new RectOffset(0, 0, 10, 10);

            // --- 1. 基本設定 ---
            EditorGUILayout.BeginVertical(boxStyle);
            {
                DrawTitle("Base Settings", "索敵対象やライト設定");
                EditorGUILayout.PropertyField(targetTag, new GUIContent("Target Tag", "見つける対象のタグ"));
                EditorGUILayout.PropertyField(obstacleMask, new GUIContent("Obstacle Layers", "視線を遮るレイヤー"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Light Effect", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(viewLight, new GUIContent("Spot Light", "視界と連動するライト"));
                EditorGUILayout.PropertyField(lightMode, new GUIContent("Mode", "ライトの点灯パターン"));
                if (lightMode.enumValueIndex == (int)EnemySentinelController.LightMode.Flicker ||
                    lightMode.enumValueIndex == (int)EnemySentinelController.LightMode.Malfunction)
                {
                    EditorGUILayout.Slider(flickerSpeed, 0.1f, 50f, new GUIContent("Flicker Speed", "点滅の速さ"));
                }
            }
            EditorGUILayout.EndVertical();

            // --- 2. 視界設定 ---
            EditorGUILayout.BeginVertical(boxStyle);
            {
                DrawTitle("Field of View", "見える範囲（シーン上で調整可）");
                EditorGUILayout.Slider(viewRadius, 0.1f, 50f, new GUIContent("View Radius", "見える距離(m)"));
                EditorGUILayout.Slider(viewAngle, 0f, 360f, new GUIContent("View Angle", "視野角(度)"));
                EditorGUILayout.Slider(eyeHeight, 0.1f, 3f, new GUIContent("Eye Height", "目の高さ(m)"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Weight Penalty", EditorStyles.boldLabel);
                EditorGUILayout.Slider(weightSensitivity, 0f, 2.0f, new GUIContent("Sensitivity (m/kg)", "1kgあたり広がる距離"));
                EditorGUILayout.HelpBox("総重量が増えると視界が広がります。\n計算式: Base Radius + (Total Weight * Sensitivity)", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            // --- 3. 移動ロジック ---
            EditorGUILayout.BeginVertical(boxStyle);
            {
                DrawTitle("Movement Logic", "移動パターン設定");

                EditorGUILayout.PropertyField(movementType, new GUIContent("Movement Type", "敵の動きの種類"));
                EnemySentinelController.MovementType mode = (EnemySentinelController.MovementType)movementType.enumValueIndex;

                if (mode != EnemySentinelController.MovementType.Idle)
                {
                    EditorGUILayout.Slider(moveSpeed, 0.1f, 20f, new GUIContent("Move Speed", "移動速度"));
                    if (mode != EnemySentinelController.MovementType.SentinelLook)
                        EditorGUILayout.Slider(waitTime, 0f, 10f, new GUIContent("Wait Time", "到着後の待機時間"));
                }

                EditorGUILayout.Space(5);
                DrawModeSpecificUI(mode);
            }
            EditorGUILayout.EndVertical();

            // --- 4. 攻撃設定 ---
            EditorGUILayout.BeginVertical(boxStyle);
            {
                DrawTitle("Attack Settings", "発見時のホラー演出");
                EditorGUILayout.Slider(rushSpeed, 1f, 30f, new GUIContent("Rush Speed", "追いかける速度"));
                EditorGUILayout.Slider(roarDuration, 0f, 5f, new GUIContent("Roar Duration", "咆哮（ため）時間"));
                EditorGUILayout.Slider(attackScaleMultiplier, 1f, 5f, new GUIContent("Scale Multiplier", "巨大化倍率"));
                EditorGUILayout.PropertyField(playerEyeHeight, new GUIContent("Player Eye Height", "カメラジャック時の高さ"));
                EditorGUILayout.PropertyField(scriptsToDisable, new GUIContent("Scripts to Disable", "停止させるスクリプト名"), true);
            }
            EditorGUILayout.EndVertical();

            // --- 5. ビジュアル ---
            EditorGUILayout.BeginVertical(boxStyle);
            {
                DrawTitle("Visuals", "モデル割り当て");
                EditorGUILayout.PropertyField(walkModel, new GUIContent("Walk Model", "通常時のモデル"));
                EditorGUILayout.PropertyField(attackModel, new GUIContent("Attack Model", "攻撃時のモデル"));
                EditorGUILayout.PropertyField(animator, new GUIContent("Animator", "使用するアニメーター"));
            }
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawTitle(string title, string subtitle)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(subtitle, EditorStyles.miniLabel);
            EditorGUILayout.Space(2);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(5);
        }

        void DrawModeSpecificUI(EnemySentinelController.MovementType mode)
        {
            switch (mode)
            {
                case EnemySentinelController.MovementType.SentinelLook:
                    EditorGUILayout.HelpBox("その場で左右を監視します", MessageType.Info);
                    EditorGUILayout.Slider(lookAngleStep, 10f, 180f, new GUIContent("Angle Step", "一度に回る角度"));
                    EditorGUILayout.Slider(lookWaitTime, 0f, 10f, new GUIContent("Wait Time", "停止時間"));
                    EditorGUILayout.Slider(lookRotateSpeed, 0.1f, 10f, new GUIContent("Rotate Speed", "回転速度"));
                    break;

                case EnemySentinelController.MovementType.PatrolLinear:
                    EditorGUILayout.HelpBox("直線距離を往復します", MessageType.Info);
                    EditorGUILayout.Slider(linearDistance, 1f, 50f, new GUIContent("Distance", "片道の距離"));
                    EditorGUILayout.PropertyField(linearDirection, new GUIContent("Direction", "進む方向"));
                    break;

                case EnemySentinelController.MovementType.PatrolCircular:
                    EditorGUILayout.HelpBox("黄色い円周上を移動します", MessageType.Info);
                    EditorGUILayout.Slider(circleRadiusX, 1f, 50f, new GUIContent("Radius", "半径"));
                    EditorGUILayout.PropertyField(clockwise, new GUIContent("Clockwise", "時計回り？"));
                    break;

                case EnemySentinelController.MovementType.PatrolElliptical:
                    EditorGUILayout.Slider(circleRadiusX, 1f, 50f, new GUIContent("Radius X", "横幅"));
                    EditorGUILayout.Slider(circleRadiusZ, 1f, 50f, new GUIContent("Radius Z", "縦幅"));
                    EditorGUILayout.PropertyField(clockwise);
                    break;

                case EnemySentinelController.MovementType.PatrolFigureEight:
                    EditorGUILayout.Slider(figureEightSize, 1f, 50f, new GUIContent("Size", "8の字の大きさ"));
                    break;

                case EnemySentinelController.MovementType.RandomWander:
                    EditorGUILayout.HelpBox("水色の円内をランダム移動します", MessageType.Info);
                    EditorGUILayout.Slider(wanderRadius, 1f, 50f, new GUIContent("Radius", "移動半径"));
                    break;

                case EnemySentinelController.MovementType.PatrolLoop:
                case EnemySentinelController.MovementType.PatrolPingPong:
                    string help = mode == EnemySentinelController.MovementType.PatrolPingPong
                        ? "登録地点を往復します (1→2→3→2→1)"
                        : "登録地点を周回します (1→2→3→1)";
                    EditorGUILayout.HelpBox(help, MessageType.Info);
                    EditorGUILayout.PropertyField(simpleWaypoints, new GUIContent("Waypoints", "経由地点のリスト"), true);
                    break;

                case EnemySentinelController.MovementType.WaypointGraphRandom:
                    EditorGUILayout.HelpBox("SentinelWaypointスクリプトを持つ地点をランダムに移動します", MessageType.Info);
                    EditorGUILayout.PropertyField(currentGraphWaypoint, new GUIContent("Start Point", "開始地点"));
                    break;
            }
        }
    }
}
