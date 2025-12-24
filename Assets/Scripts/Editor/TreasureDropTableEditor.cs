using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityJam.Core; // TreasureDropTableのnamespace

namespace UnityJam.Editors
{
    [CustomEditor(typeof(TreasureDropTable))]
    public class TreasureDropTableEditor : Editor
    {
        // 固定のカラー配列を削除し、関数で生成するようにしました
        public override void OnInspectorGUI()
        {
            TreasureDropTable table = (TreasureDropTable)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("ドロップ率の調整", EditorStyles.boldLabel);

            // 1. 円グラフの描画
            DrawPieChart(table);

            EditorGUILayout.Space(20);

            // 2. スライダー付きリストの描画
            DrawListWithSliders(table);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(table);
            }
        }

        // インデックスに基づいてユニークな色を生成する関数
        Color GetUniqueColor(int index)
        {
            float hue = (index * 0.61803398875f) % 1.0f;
            // Saturation(彩度)=0.7, Value(明度)=1.0 で鮮やかに
            return Color.HSVToRGB(hue, 0.7f, 1.0f);
        }

        void DrawPieChart(TreasureDropTable table)
        {
            // リストが空なら描画しない
            if (table.dropList == null || table.dropList.Count == 0) return;

            float totalWeight = 0f;
            foreach (var entry in table.dropList) totalWeight += entry.weight;

            if (totalWeight <= 0) return;

            Rect rect = GUILayoutUtility.GetRect(200, 200);
            float size = Mathf.Min(rect.width, rect.height) * 0.8f;
            Vector2 center = rect.center;

            // 開始角度のオフセット
            float currentAngle = 0f;

            for (int i = 0; i < table.dropList.Count; i++)
            {
                var entry = table.dropList[i];
                if (entry.weight <= 0) continue;

                float percentage = entry.weight / totalWeight;
                float angle = percentage * 360f;

                Color color = GetUniqueColor(i);
                Handles.color = color;

                // 描画ロジックの変更
                // Vector3.down (上方向) からスタートし、時計回りに描画
                Vector3 startVector = Quaternion.Euler(0, 0, currentAngle) * Vector3.down;

                Handles.DrawSolidArc(
                    new Vector3(center.x, center.y, 0),
                    Vector3.forward,
                    startVector,
                    angle, // 正の値で時計回り
                    size / 2
                );

                // 次の開始位置へ進める（時計回りなので足す）
                currentAngle += angle;
            }
            Handles.color = Color.white;
        }

        void DrawListWithSliders(TreasureDropTable table)
        {
            float totalWeight = 0f;
            if (table.dropList != null)
            {
                foreach (var entry in table.dropList) totalWeight += entry.weight;
            }

            serializedObject.Update();
            SerializedProperty listProp = serializedObject.FindProperty("dropList");

            // ▼リストの折りたたみヘッダー
            listProp.isExpanded = EditorGUILayout.Foldout(listProp.isExpanded, "Drop List");

            if (listProp.isExpanded)
            {
                EditorGUI.indentLevel++;

                // リストの中身を描画
                for (int i = 0; i < listProp.arraySize; i++)
                {
                    SerializedProperty entryProp = listProp.GetArrayElementAtIndex(i);
                    SerializedProperty itemProp = entryProp.FindPropertyRelative("item");
                    SerializedProperty weightProp = entryProp.FindPropertyRelative("weight");

                    // データの安全取得（削除直後などのエラー防止）
                    int weightValue = 10;
                    if (i < table.dropList.Count) weightValue = table.dropList[i].weight;

                    Color color = GetUniqueColor(i);

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        // 上段：色、アイテム、削除ボタン
                        EditorGUILayout.BeginHorizontal();
                        {
                            // 色見本
                            Rect colorRect = EditorGUILayout.GetControlRect(GUILayout.Width(15), GUILayout.Height(15));
                            EditorGUI.DrawRect(colorRect, color);

                            // アイテム欄
                            EditorGUILayout.PropertyField(itemProp, GUIContent.none);

                            // 削除ボタン（×）
                            if (GUILayout.Button("×", GUILayout.Width(25)))
                            {
                                listProp.DeleteArrayElementAtIndex(i);
                                break; // リストが変動したのでループを抜ける
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        // 下段：スライダーと確率
                        if (itemProp.objectReferenceValue != null)
                        {
                            float percent = (totalWeight > 0) ? (weightValue / totalWeight) * 100f : 0;
                            EditorGUILayout.LabelField($"確率: {percent:F1}%  (Weight: {weightValue})", EditorStyles.miniLabel);
                            weightProp.intValue = EditorGUILayout.IntSlider("重み", weightProp.intValue, 1, 100);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("アイテムをセットしてください", EditorStyles.miniLabel);
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }

                // 追加ボタン
                EditorGUILayout.Space(5);
                if (GUILayout.Button("+ アイテムを追加", GUILayout.Height(30)))
                {
                    listProp.InsertArrayElementAtIndex(listProp.arraySize);
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
