using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityJam.Enemies
{
    /// <summary>
    /// 敵キャラクター単体の機能を制御するクラス
    /// 移動と視界判定（扇形・即死）を管理します。
    /// </summary>
    public class EnemySentinelController : MonoBehaviour
    {
        // 設定項目（Editor拡張で表示を切り替えるため、Attributeは最低限）
        // ============================================================

        // --- 基本設定 ---
        [Header("--- Settings: References ---")]
        [Tooltip("検知対象のタグ（Findは使用せず、Tag判定を行う）")]
        [SerializeField] private string targetTag = "Player";

        [Tooltip("視界として使うスポットライト（あれば設定）")]
        [SerializeField] private Light viewLight;

        [Tooltip("障害物とみなすレイヤー（壁越しに見えないようにする場合設定）")]
        [SerializeField] private LayerMask obstacleMask;

        // --- ライト演出 ---
        public enum LightMode { Steady, Flicker, Malfunction, Off }
        [Header("--- Settings: Light ---")]
        [SerializeField] private LightMode lightMode = LightMode.Steady;
        [SerializeField] private float flickerSpeed = 10f; // 点滅速度

        // --- 移動設定 ---
        public enum MovementType
        {
            [Tooltip("完全停止")]
            Idle,
            [Tooltip("定点監視（首振り）")]
            SentinelLook,
            [Tooltip("直線運動")]
            PatrolLinear,
            [Tooltip("円運動")]
            PatrolCircular,
            [Tooltip("楕円運動")]
            PatrolElliptical,
            [Tooltip("8の字")]
            PatrolFigureEight,
            [Tooltip("ウェイポイント周回（1 -> 2 -> 3 -> 1）")]
            PatrolLoop,
            [Tooltip("ウェイポイント往復（1 -> 2 -> 3 -> 2 -> 1）")]
            PatrolPingPong,
            [Tooltip("ウェイポイントランダム")]
            WaypointGraphRandom,
            [Tooltip("自由徘徊")]
            RandomWander,
        }

        [Header("--- Settings: Movement ---")]
        [Tooltip("移動パターンを選択します")]
        [SerializeField] private MovementType movementType = MovementType.RandomWander;

        [Tooltip("移動速度")]
        [SerializeField, Range(0.1f, 10f)] private float moveSpeed = 3.0f;

        [Tooltip("目的地に到着した後の待機時間")]
        [SerializeField] private float waitTime = 1.0f;

        // --- 各モード用パラメータ
        // [SentinelLook]
        [SerializeField] private float lookAngleStep = 45f;
        [SerializeField] private float lookWaitTime = 2.0f;
        [SerializeField] private float lookRotateSpeed = 2.0f;

        // [Linear]
        [SerializeField] private float linearDistance = 5.0f;
        [SerializeField] private Vector3 linearDirection = Vector3.forward;

        // [Circular / Elliptical]
        [SerializeField] private float circleRadiusX = 5.0f;
        [SerializeField] private float circleRadiusZ = 3.0f;
        [SerializeField] private bool clockwise = true;

        // [Figure Eight]
        [SerializeField] private float figureEightSize = 5.0f;

        // [Random Wander]
        [SerializeField] private float wanderRadius = 7.0f;

        // [Simple Waypoints (Loop / PingPong)]
        [SerializeField] private Transform[] simpleWaypoints;

        // [Graph Waypoints (Random)]
        [SerializeField] private SentinelWaypoint currentGraphWaypoint;

        // --- 視界設定 ---
        [Header("--- Settings: Field of View ---")]
        [Tooltip("視界の距離")]
        [SerializeField, Range(1f, 20f)] private float viewRadius = 5.0f;
        [Tooltip("視界の角度（扇型の広がり）")]
        [SerializeField, Range(0f, 360f)] private float viewAngle = 90.0f;
        [Tooltip("目の高さ（地面すれすれではなく、少し高い位置から見る）")]
        [SerializeField, Range(0.1f, 2.0f)] private float eyeHeight = 1.0f;
        [Tooltip("重量感度：総重量1kgにつき視界が何メートル広がるか (例: 0.5なら10kgで+5m)")]
        [SerializeField] private float weightSensitivity = 0.5f;


        // --- 攻撃設定 ---
        [Header("--- Settings: Attack ---")]
        [Tooltip("襲撃時の急接近スピード")]
        [SerializeField] private float rushSpeed = 10.0f;

        [Tooltip("プレイヤーを食べるまでのため時間（口を開く時間）")]
        [SerializeField] private float roarDuration = 1.5f;

        [Tooltip("襲撃時の巨大化倍率（1.5倍など）")]
        [SerializeField] private float attackScaleMultiplier = 1.5f;

        [Tooltip("襲撃時に停止させたいスクリプト名（例: FirstPersonController, LookController 等）")]
        [SerializeField] private List<string> scriptsToDisable = new List<string>();

        [Tooltip("FPS視点にする時の目の高さ")]
        [SerializeField] private float playerEyeHeight = 1.0f;

        // --- モデル参照 ---
        [Header("--- Models ---")]
        [Tooltip("普段の歩行用モデル")]
        [SerializeField] private GameObject walkModel;

        [Tooltip("襲撃時の口開きモデル")]
        [SerializeField] private GameObject attackModel;

        // --- アニメーション ---
        [Header("--- Animation ---")]
        [SerializeField] private Animator animator;

        // 内部変数
        // ============================================================

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 lastPosition;
        private float currentWaitTimer;
        private Vector3 targetPosition;

        // 計算用
        private float angleTimer;
        private int currentWaypointIndex;
        private bool isMovingForward = true; // PingPong用
        private bool isAttacking = false;

        // 定点監視用
        private float sentinelStateTimer;
        private Quaternion sentinelTargetRot;
        private bool isSentinelRotating;

        // ライト用
        private float initialLightIntensity;
        private float lightNoiseOffset;

        public float CurrentViewRadius
        {
            get
            {
                float r = viewRadius;
                // ゲーム中活インベントリがある場合、重量分を加算
                if (Application.isPlaying && Inventory.Instance != null)
                {
                    r += Inventory.Instance.TotalWeight * weightSensitivity;
                }
                return r;
            }
        }

        // Unity イベント関数
        // ============================================================

        void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            lastPosition = transform.position;
            sentinelTargetRot = initialRotation;

            if (viewLight != null) initialLightIntensity = viewLight.intensity;
            lightNoiseOffset = Random.Range(0f, 100f);

            // 初期ターゲット設定
            if (movementType == MovementType.RandomWander) SetNewRandomTarget();

            // ウェイポイント初期化
            if ((movementType == MovementType.PatrolLoop || movementType == MovementType.PatrolPingPong)
                && simpleWaypoints != null && simpleWaypoints.Length > 0)
            {
                targetPosition = simpleWaypoints[0].position;
            }

            if (movementType == MovementType.WaypointGraphRandom && currentGraphWaypoint != null)
                targetPosition = currentGraphWaypoint.transform.position;

            SyncLightSettings();
            InitModels();
        }

        void Update()
        {
            if (isAttacking) return;

            UpdateLightEffect();
            HandleMovement();
            DetectPlayer();
            UpdateAnimation();
        }

        // インスペクターで値を変更したときに即座に反映（エディタ機能）
        private void OnValidate()
        {
            SyncLightSettings();
        }

        void SyncLightSettings()
        {
            if(viewLight != null)
            {
                viewLight.type = LightType.Spot;
                viewLight.range = viewRadius;
                viewLight.spotAngle = viewAngle;
                // ライトの色や強さをここで強制してもよいが、今回はユーザー設定に任せる
            }
        }

        void UpdateLightEffect()
        {
            if (viewLight == null) return;

            // 現在の視界半径（重量加算済み）を取得
            float currentR = CurrentViewRadius;

            // 1. ライトの届く距離を更新
            viewLight.range = currentR;
            viewLight.spotAngle = viewAngle;

            // 2. 距離が伸びた分だけ、光の強さ(Intensity)も補正する
            // (現在の距離 / 基本距離) の倍率を計算
            float ratio = currentR / viewRadius;

            float targetIntensity = initialLightIntensity * ratio;

            switch (lightMode)
            {
                case LightMode.Steady:
                    viewLight.enabled = true;
                    viewLight.intensity = targetIntensity;
                    break;
                case LightMode.Off:
                    viewLight.enabled = false;
                    break;
                case LightMode.Flicker:
                    viewLight.enabled = true;
                    float flicker = Mathf.PingPong(Time.time * flickerSpeed, 1.0f);
                    viewLight.intensity = targetIntensity * (0.5f + flicker * 0.5f);
                    break;
                case LightMode.Malfunction:
                    viewLight.enabled = true;
                    float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, lightNoiseOffset);
                    viewLight.intensity = targetIntensity * (noise > 0.6f ? 1f : 0.1f);
                    break;
            }
        }

        void InitModels()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (walkModel != null)
            {
                walkModel.SetActive(true);
                // 歩行モデル側にAnimatorがある場合が多いので取得し直す
                var anim = walkModel.GetComponent<Animator>();
                if (anim != null) animator = anim;
            }
            if (attackModel != null) attackModel.SetActive(false);
        }

        // 機能ロジック
        // ============================================================

        /// <summary>
        /// 設定されたタイプに基づいて移動を制御
        /// </summary>
        void HandleMovement()
        {
            switch (movementType)
            {
                case MovementType.SentinelLook:
                    MoveSentinelLook();
                    break;
                case MovementType.PatrolLinear:
                    MoveLinear();
                    break;
                case MovementType.PatrolCircular:
                    MoveElliptical(true);
                    break;
                case MovementType.PatrolElliptical:
                    MoveElliptical(false);
                    break;
                case MovementType.PatrolFigureEight:
                    MoveFigureEight();
                    break;
                case MovementType.RandomWander:
                    MoveRandom();
                    break;
                case MovementType.PatrolLoop:
                    MoveWaypointsSimple(false); // Loop
                    break;
                case MovementType.PatrolPingPong:
                    MoveWaypointsSimple(true);  // PingPong
                    break;
                case MovementType.WaypointGraphRandom:
                    MoveWaypointGraph();
                    break;
                default:
                    animator.SetFloat("Speed", 0);
                    break;
            }
        }

        // 1. 定点監視（首振り）
        void MoveSentinelLook()
        {
            if (isSentinelRotating)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, sentinelTargetRot, lookRotateSpeed * 100f * Time.deltaTime);
                if (Quaternion.Angle(transform.rotation, sentinelTargetRot) < 0.1f)
                {
                    isSentinelRotating = false;
                    sentinelStateTimer = lookWaitTime;
                }
            }
            else
            {
                sentinelStateTimer -= Time.deltaTime;
                if (sentinelStateTimer <= 0)
                {
                    // ランダムな方向へ
                    float randomAngle = Random.Range(-lookAngleStep, lookAngleStep);
                    sentinelTargetRot = initialRotation * Quaternion.Euler(0, randomAngle, 0);
                    isSentinelRotating = true;
                }
            }
        }

        // 直接往復
        void MoveLinear()
        {
            // PingPongで 0~1 を作り、-1~1 に変換
            float t = Mathf.PingPong(Time.time * moveSpeed, linearDistance * 2) - linearDistance;

            // 現在位置から少し先の位置を予測して、そこに向かってMoveTowardsする形にする
            // これで「向き」を自然に制御できる
            Vector3 targetPos = initialPosition + (linearDirection.normalized * t);

            MoveToAndLook(targetPos);
        }

        // 3. 楕円・円軌道
        void MoveElliptical(bool isCircle)
        {
            float rX = isCircle ? circleRadiusX : circleRadiusX;
            float rZ = isCircle ? circleRadiusX : circleRadiusZ; // 円ならZもXと同じ半径

            // 角速度計算 (半径が大きいほどゆっくり回るように調整)
            float speed = moveSpeed / Mathf.Max(rX, rZ);
            angleTimer += (clockwise ? speed : -speed) * Time.deltaTime;

            float x = Mathf.Cos(angleTimer) * rX;
            float z = Mathf.Sin(angleTimer) * rZ;

            Vector3 targetPos = initialPosition + new Vector3(x, 0, z);

            // 移動と向き
            MoveToAndLook(targetPos);
        }

        // 4. 8の字旋回（インフィニティ）
        void MoveFigureEight()
        {
            // リサージュ図形的な計算
            float speed = moveSpeed / figureEightSize;
            angleTimer += speed * Time.deltaTime;

            float x = Mathf.Cos(angleTimer) * figureEightSize;
            float z = Mathf.Sin(2f * angleTimer) * (figureEightSize / 2f); // Zは2倍の周波数

            Vector3 targetPos = initialPosition + new Vector3(x, 0, z);
            MoveToAndLook(targetPos);
        }

        // 5. ランダム徘徊
        void MoveRandom()
        {
            MoveToTargetWithWait(targetPosition, () => SetNewRandomTarget());
        }

        // 新しいランダムな目的地を設定
        void SetNewRandomTarget()
        {
            Vector2 circle = Random.insideUnitCircle * wanderRadius;
            targetPosition = initialPosition + new Vector3(circle.x, 0, circle.y);
        }

        // 6. シンプルWP (Loop & PingPong)
        void MoveWaypointsSimple(bool isPingPong)
        {
            if (simpleWaypoints == null || simpleWaypoints.Length == 0) return;

            // インデックス安全策
            if (currentWaypointIndex >= simpleWaypoints.Length) currentWaypointIndex = 0;
            if (simpleWaypoints[currentWaypointIndex] == null) return;

            targetPosition = simpleWaypoints[currentWaypointIndex].position;

            MoveToTargetWithWait(targetPosition, () =>
            {
                if (isPingPong)
                {
                    // 往復ロジック
                    if (isMovingForward)
                    {
                        currentWaypointIndex++;
                        if (currentWaypointIndex >= simpleWaypoints.Length)
                        {
                            currentWaypointIndex = Mathf.Max(0, simpleWaypoints.Length - 2);
                            isMovingForward = false;
                        }
                    }
                    else
                    {
                        currentWaypointIndex--;
                        if (currentWaypointIndex < 0)
                        {
                            currentWaypointIndex = Mathf.Min(simpleWaypoints.Length - 1, 1);
                            isMovingForward = true;
                        }
                    }
                }
                else
                {
                    // ループ
                    currentWaypointIndex = (currentWaypointIndex + 1) % simpleWaypoints.Length;
                }
            });
        }

        // 7. グラフWP
        void MoveWaypointGraph()
        {
            if (currentGraphWaypoint == null) return;
            targetPosition = currentGraphWaypoint.transform.position;

            MoveToTargetWithWait(targetPosition, () =>
            {
                if (currentGraphWaypoint.neighbors != null && currentGraphWaypoint.neighbors.Count > 0)
                {
                    int rnd = Random.Range(0, currentGraphWaypoint.neighbors.Count);
                    var next = currentGraphWaypoint.neighbors[rnd];
                    if (next != null) currentGraphWaypoint = next;
                }
            });
        }

        // --- 共通移動関数 ---

        // 目的地へ移動し、待機時間を消化したら callback を呼ぶ
        void MoveToTargetWithWait(Vector3 target, System.Action onArrived)
        {
            MoveToAndLook(target);

            // 到着判定
            if (Vector3.Distance(transform.position, target) < 0.2f)
            {
                currentWaitTimer -= Time.deltaTime;
                if (currentWaitTimer <= 0)
                {
                    currentWaitTimer = waitTime;
                    onArrived?.Invoke();
                }
            }
            else
            {
                currentWaitTimer = waitTime; // 移動中はタイマーリセット
            }
        }

        // 指定位置へ移動し、その方向を向く
        void MoveToAndLook(Vector3 target)
        {
            // 高さ（Y）は維持
            Vector3 currentPos = transform.position;
            Vector3 targetPos = new Vector3(target.x, currentPos.y, target.z);

            // 移動
            transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

            // 向き変更
            Vector3 dir = (targetPos - currentPos).normalized;
            if (dir != Vector3.zero && dir.sqrMagnitude > 0.001f)
            {
                Quaternion toRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * 10f); // 滑らかに
            }
        }

        void UpdateAnimation()
        {
            if (animator == null) return;
            float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
            animator.SetFloat("Speed", speed);
            lastPosition = transform.position;
        }

        // 索敵 & 捕食
        // ============================================================

        /// <summary>
        /// プレイヤー検出ロジック
        /// </summary>
        void DetectPlayer()
        {
            // 範囲内のコライダーを全て取得
            float effectiveRadius = CurrentViewRadius;

            // 目の位置（足元 + EyeHeight）
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, effectiveRadius);
            Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

            foreach(Collider target in targetsInViewRadius)
            {
                // Playerタグを持っているか確認
                if (target.CompareTag(targetTag))
                {
                    Transform targetTransform = target.transform;
                    Vector3 targetPos = targetTransform.position;
                    // プレイヤーの原点が足元なら、胸当たり(1.0f)を見るように補正するとより正確です
                    targetPos += Vector3.up * (eyeHeight * 0.8f);

                    Vector3 dirToTarget = (targetPos - eyePos).normalized;
                    float distToTarget = Vector3.Distance(eyePos, targetPos);

                    // 1. 距離チェック
                    if (distToTarget > effectiveRadius) continue;

                    // 2. 角度判定（扇型の範囲内か）
                    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                    {
                        // 3. 障害物チェック（Raycast）
                        // eyePos から targetPos に向かってレイを撃つ
                        // obstacleMaskに含まれるオブジェクトに当たったら「見えていない」と判断
                        if (!Physics.Raycast(eyePos, dirToTarget, distToTarget, obstacleMask))
                        {
                            StartCoroutine(PredationSequence(target.gameObject));
                        }
                    }
                }
            }
        }

        // 捕食シーケンス
        IEnumerator PredationSequence(GameObject player)
        {
            isAttacking = true; // Updateを停止
            animator.SetFloat("Speed", 0);

            Debug.Log("発見");

            // 1. プレイヤーを動けなくする
            // プレイヤー本体とカメラの両方をチェック対象にする
            List<GameObject> checkTargets = new List<GameObject>();
            if (player != null) checkTargets.Add(player);
            if (Camera.main != null) checkTargets.Add(Camera.main.gameObject);

            // 親オブジェクトにスクリプトがある場合もあるので、カメラの親も追加しておく
            if (Camera.main != null && Camera.main.transform.parent != null)
                checkTargets.Add(Camera.main.transform.parent.gameObject);

            // リストに登録された名前のスクリプトを片っ端からオフにする
            foreach (var targetObj in checkTargets)
            {
                foreach (string scriptName in scriptsToDisable)
                {
                    // 文字列でコンポーネントを取得してオフにする
                    // (GetComponentは型だけでなく文字列でも検索できます)
                    var component = targetObj.GetComponent(scriptName) as MonoBehaviour;
                    if (component != null)
                    {
                        component.enabled = false;
                    }
                }
            }

            // 物理挙動も念のため止める
            var rb = player.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;
            player.SetActive(false);

            // カメラジャック（FPS視点化 & 敵の方向を向く）
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                // カメラをプレイヤーの目の位置へ移動
                mainCam.transform.position = player.transform.position + Vector3.up * playerEyeHeight;
                mainCam.transform.LookAt(transform.position + Vector3.up * eyeHeight);
            }

            // 2. 敵が一瞬止まって、プレイヤーのほうを向く
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            directionToPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

            // 3. モデルの切り替え
            if (walkModel != null) walkModel.SetActive(false); // 歩きモデルを消す
            if (attackModel != null)
            {
                attackModel.SetActive(true); // 怖いモデルを出す
            }

            // 4. その場で咆哮（溜め）
            yield return new WaitForSeconds(roarDuration);

            // 5. 急接近
            float attackTimer = 0f;
            Vector3 startScale = attackModel.transform.localScale;
            Vector3 targetScale = startScale * attackScaleMultiplier; // 指定倍率まで大きくする

            while (Vector3.Distance(transform.position, player.transform.position) > 0.8f)
            {
                // A. 敵の移動
                transform.LookAt(player.transform);
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, rushSpeed * Time.deltaTime);

                // B. カメラを常に敵の顔に向ける（逃げられない恐怖）
                if (mainCam != null)
                {
                    // プレイヤーの目の位置に固定
                    mainCam.transform.position = player.transform.position + Vector3.up * playerEyeHeight;

                    // 敵の顔（少し上）を見る
                    Vector3 lookTarget = transform.position + Vector3.up * (eyeHeight * 1.2f);
                    mainCam.transform.LookAt(lookTarget);
                }

                // C. 迫りくるにつれて巨大化させる
                // （距離が近づくほど大きくなる、または時間経過で大きくなる）
                if (attackModel != null)
                {
                    attackModel.transform.localScale = Vector3.Lerp(attackModel.transform.localScale, targetScale, Time.deltaTime * 5f);
                }

                attackTimer += Time.deltaTime;
                if (attackTimer > 2.0f) break;
                yield return null;
            }

            // 6. 捕食
            // 演出のために一瞬待つ
            Animator attackAnim = attackModel.GetComponent<Animator>();
            if (attackAnim != null)
            {
                // ここで「噛め！」と命令する
                attackAnim.SetTrigger("DoBite");
            }

            // 噛むアニメーションが終わるのを少し待つ（0.5秒くらい）
            yield return new WaitForSeconds(0.5f);

            // 7. プレイヤー破壊
            Debug.Log("<color=red>うぎゃああああぁぁぁ！！ぶち56すｯｯ！！！！</color>");
            player.TryGetComponent<SpriteRenderer>(out var sr);
            if (sr != null) sr.enabled = false; // プレイヤーを見えなくする

            // ゲームオーバー画面への遷移などをここに書く
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HandleDayFailed();
            }
        }

        // エディタ拡張・デバッグ表示（Gizmos）
        // ============================================================

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 center = transform.position;

            float r = Application.isPlaying ? CurrentViewRadius : viewRadius;

            // 1. 視界の描画
            Vector3 eye = center + Vector3.up * eyeHeight;
            Color fovColor = isAttacking ? Color.red : new Color(0, 1, 0, 0.2f);
            Handles.color = fovColor;
            Vector3 angleA = DirFromAngle(-viewAngle / 2);
            Handles.DrawSolidArc(eye, Vector3.up, angleA, viewAngle, r);

            // 2. 移動パスの描画
            Gizmos.color = new Color(1, 1, 0, 0.5f); // 薄い黄色
            Handles.color = Color.yellow;

            // プレイ中は「初期位置」を基準に描画し、停止中は「現在位置」を基準にする
            Vector3 drawCenter = Application.isPlaying ? initialPosition : center;

            switch (movementType)
            {
                case MovementType.PatrolLinear:
                    // 基準点（プレイ中は初期位置、エディタ中は現在位置）
                    Vector3 basePos = Application.isPlaying ? initialPosition : center;
                    Vector3 dir = linearDirection.normalized;

                    // プラス方向とマイナス方向の両端を計算
                    Vector3 posEnd = basePos + (dir * linearDistance);
                    Vector3 negEnd = basePos - (dir * linearDistance);

                    // 端から端まで線を引く
                    Gizmos.DrawLine(negEnd, posEnd);

                    // 両端に球を表示して分かりやすくする
                    Gizmos.DrawWireSphere(posEnd, 0.3f);
                    Gizmos.DrawWireSphere(negEnd, 0.3f);
                    break;

                case MovementType.PatrolCircular:
                    Handles.DrawWireDisc(drawCenter, Vector3.up, circleRadiusX);
                    break;

                case MovementType.PatrolElliptical:
                    DrawEllipse(drawCenter, circleRadiusX, circleRadiusZ);
                    break;

                case MovementType.PatrolFigureEight:
                    DrawFigureEight(drawCenter, figureEightSize);
                    break;

                case MovementType.RandomWander:
                    Handles.color = Color.cyan;
                    Handles.DrawWireDisc(drawCenter, Vector3.up, wanderRadius);
                    break;

                case MovementType.PatrolLoop:
                case MovementType.PatrolPingPong:
                    if (simpleWaypoints != null && simpleWaypoints.Length > 0)
                    {
                        for (int i = 0; i < simpleWaypoints.Length - 1; i++)
                            if (simpleWaypoints[i] && simpleWaypoints[i + 1])
                                Gizmos.DrawLine(simpleWaypoints[i].position, simpleWaypoints[i + 1].position);

                        // Loopの場合のみ最後の線を引く
                        if (movementType == MovementType.PatrolLoop && simpleWaypoints.Length > 1 && simpleWaypoints[0] && simpleWaypoints[simpleWaypoints.Length - 1])
                            Gizmos.DrawLine(simpleWaypoints[simpleWaypoints.Length - 1].position, simpleWaypoints[0].position);
                    }
                    break;
            }
        }

        // 楕円を描画するヘルパー関数
        void DrawEllipse(Vector3 center, float rx, float rz)
        {
            Vector3 prev = center + new Vector3(rx, 0, 0);
            int segments = 64;
            for (int i = 1; i <= segments; i++)
            {
                float t = (i / (float)segments) * Mathf.PI * 2;
                Vector3 next = center + new Vector3(Mathf.Cos(t) * rx, 0, Mathf.Sin(t) * rz);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        // 8の字を描画するヘルパー関数
        void DrawFigureEight(Vector3 center, float size)
        {
            Vector3 prev = center;
            int segments = 64;
            for (int i = 1; i <= segments; i++)
            {
                float t = (i / (float)segments) * Mathf.PI * 2;
                float x = Mathf.Cos(t) * size;
                float z = Mathf.Sin(2f * t) * (size / 2f);
                Vector3 next = center + new Vector3(x, 0, z);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        Vector3 DirFromAngle(float angleInDegrees)
        {
            angleInDegrees += transform.eulerAngles.y;
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }
#endif
    }
}
