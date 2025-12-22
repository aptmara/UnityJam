using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.Experimental.Rendering;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityJam.Enemies
{
    /// <summary>
    /// 敵キャラクター単体の機能を制御するクラス
    /// 移動（直線・円・ランダム）と視界判定（扇形・即死）を管理します。
    /// </summary>
    public class EnemySentinelController : MonoBehaviour
    {
        // 設定項目（SerializeField private）
        // ============================================================

        [Header("--- Settings: References ---")]
        [Tooltip("検知対象のタグ（Findは使用せず、Tag判定を行う）")]
        [SerializeField] private string targetTag = "Player";

        [Tooltip("視界として使うスポットライト（あれば設定）")]
        [SerializeField] private Light viewLight;

        [Tooltip("障害物とみなすレイヤー（壁越しに見えないようにする場合設定）")]
        [SerializeField] private LayerMask obstacleMask;

        [Header("--- Settings: Movement ---")]
        [Tooltip("移動パターンを選択します")]
        [SerializeField] private MovementType movementType = MovementType.RandomWander;

        [Tooltip("移動速度")]
        [SerializeField, Range(0.1f, 10f)] private float moveSpeed = 3.0f;

        [Tooltip("目的地に到着した後の待機時間")]
        [SerializeField] private float waitTime = 1.0f;

        [Header("Pattern: Waypoints (巡回)")]
        [Tooltip("巡回するポイントのリスト")]
        [SerializeField] private Transform[] waypoints;

        [Header("Pattern: Linear (直線往復 - 旧)")]
        [Tooltip("往復する距離（初期位置からの片道距離）")]
        [SerializeField] private float linearDistance = 5.0f;
        [Tooltip("往復する方向（ワールド座標基準）")]
        [SerializeField] private Vector3 linearDirection = Vector3.forward;

        [Header("Pattern: Circular (円運動)")]
        [Tooltip("円の半径")]
        [SerializeField] private float circleRadius = 5.0f;
        [Tooltip("右回りかどうか")]
        [SerializeField] private bool clockwise = true;

        [Header("Pattern: Random (ランダムうろちょろ)")]
        [Tooltip("移動範囲の半径（初期位置中心）")]
        [SerializeField] private float wanderRadius = 7.0f;

        [Header("--- Settings: Field of View ---")]
        [Tooltip("視界の距離")]
        [SerializeField, Range(1f, 20f)] private float viewRadius = 5.0f;
        [Tooltip("視界の角度（扇型の広がり）")]
        [SerializeField, Range(0f, 360f)] private float viewAngle = 90.0f;

        [Tooltip("目の高さ（地面すれすれではなく、少し高い位置から見る）")]
        [SerializeField, Range(0.1f, 2.0f)] private float eyeHeight = 1.0f;

        // 内部変数
        // ============================================================

        private Vector3 initialPosition;    // 初期位置
        private float   currentWaitTimer;   // 待機タイマー
        private Vector3 targetPosition;     // ランダム移動用ターゲット
        private float   circleAngle;        // 円運動用角度

        // Waypoint用
        private int     currentWaypointIndex = 0;
        private bool    isMovingForward = true; // PingPong用

        // 移動タイプの定義
        public enum MovementType
        {
            Idle,           // 動かない
            PatrolLinear,   // 直線往復
            PatrolCircular, // 円運動
            RandomWander,   // ランダム徘徊
            PatrolLoop,     // ウェイポイント周回（1 -> 2 -> 3 -> 1）
            PatrolPingPong, // ウェイポイント往復（1 -> 2 -> 3 -> 2 -> 1）
        }

        // Unity イベント関数
        // ============================================================

        void Start()
        {
            initialPosition = transform.position;
            SetNewRandomTarget();

            // ライトの初期設定同期
            SyncLightSettings();

            // Waypointの初期ターゲット設定
            if (waypoints != null && waypoints.Length > 0 )
            {
                targetPosition = waypoints[0].position;
            }
        }

        void Update()
        {
            HandleMovement();
            DetectPlayer();
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
                // ライトの色や強さをここで強制してもよいですが、今回はユーザー設定に任せます
            }
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
                case MovementType.PatrolLinear:
                    MoveLinear();
                    break;
                case MovementType.PatrolCircular:
                    MoveCircular();
                    break;
                case MovementType.RandomWander:
                    MoveRandom();
                    break;
                case MovementType.PatrolLoop:
                    MoveWaypoints(true);
                    break;
                case MovementType.PatrolPingPong:
                    MoveWaypoints(false);
                    break;
                case MovementType.Idle:
                default:
                    break;
            }
        }

        // 直接往復
        void MoveLinear()
        {
            // PingPong関数を使って 0 ~ 1 の値を作り、それを -1 ~ 1 に変換して往復させる
            float dist = Mathf.PingPong(Time.time * moveSpeed, linearDistance * 2) - linearDistance;
            Vector3 nextPos = initialPosition + (linearDirection * dist);

            // 移動
            transform.position = nextPos;

            // 進行方向を向く（オプション）
            Vector3 dir = nextPos - transform.position;
            if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
        }

        // 円運動
        void MoveCircular()
        {
            // 角度を更新
            float speed = moveSpeed / circleRadius; // 半径が大きいほど角速度を遅くして見た目の速度を一定に
            circleAngle += (clockwise ? speed : -speed) * Time.deltaTime;

            // 新しい位置を計算（X-Z平面での円運動）
            float x = Mathf.Cos(circleAngle) * circleRadius;
            float z = Mathf.Sin(circleAngle) * circleRadius;
            Vector3 offset = new Vector3(x, 0, z);

            Vector3 nextPos = initialPosition + offset;

            // 移動
            transform.position = nextPos;

            // 常に進行方向（あるいは円の中心など）を向く
            // ここでは進行方向を向かせる
            Vector3 forwardDir = new Vector3(-Mathf.Sin(circleAngle), 0, Mathf.Cos(circleAngle));
            if (!clockwise) forwardDir *= -1;
            if (forwardDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(forwardDir);
        }

        // ランダム徘徊
        void MoveRandom()
        {
            MoveToTargetAndLook(targetPosition, () =>
            {
                SetNewRandomTarget();
            });
        }

        // 新しいランダムな目的地を設定
        void SetNewRandomTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            targetPosition = initialPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        // Waypoint移動ロジック
        void MoveWaypoints(bool isLoop)
        {
            if(waypoints == null || waypoints.Length == 0) return;

            // ターゲット位置を更新（動くオブジェクトに対応するため舞フレーム取得）
            targetPosition = waypoints[currentWaypointIndex].position;

            MoveToTargetAndLook(targetPosition, () =>
            {
                // 到着時の処理
                UpdateNextWaypoint(isLoop);
            });
        }

        void UpdateNextWaypoint(bool isLoop)
        {
            if(isLoop)
            {
                // ループ: 0 -> 1 -> 2 -> 0
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            else
            {
                // PingPong: 0 -> 1 -> 2 -> 1 -> 0
                if(isMovingForward)
                {
                    if(currentWaypointIndex >=  waypoints.Length - 1)
                    {
                        isMovingForward = false;
                        currentWaypointIndex--;
                    }
                    else
                    {
                        currentWaypointIndex++;
                    }
                }
                else
                {
                    if(currentWaypointIndex <= 0)
                    {
                        isMovingForward = true;
                        currentWaypointIndex++;
                    }
                    else
                    {
                        currentWaypointIndex--;
                    }
                }
            }
        }

        // 共通: 移動と回転のヘルパー
        void MoveToTargetAndLook(Vector3 target, System.Action onArrived)
        {
            // 移動
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            // 回転
            Vector3 direction = target - transform.position;
            direction.y = 0; // 水平回転のみ
            if (direction != Vector3.zero && direction.sqrMagnitude > 0.001f)
            {
                Quaternion toRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, step * 5f);
            }

            // 到着判定
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                currentWaitTimer -= Time.deltaTime;
                if (currentWaitTimer <= 0)
                {
                    currentWaitTimer = waitTime;
                    onArrived?.Invoke();
                }
            }
        }

        /// <summary>
        /// プレイヤー検出ロジック
        /// </summary>
        void DetectPlayer()
        {
            // 範囲内のコライダーを全て取得
            Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius);

            // 目の位置（足元 + EyeHeight）
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
                    if (distToTarget > viewRadius) continue;

                    // 2. 角度判定（扇型の範囲内か）
                    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                    {
                        // 3. 障害物チェック（Raycast）
                        // eyePos から targetPos に向かってレイを撃つ
                        // obstacleMaskに含まれるオブジェクトに当たったら「見えていない」と判断
                        if (!Physics.Raycast(eyePos, dirToTarget, distToTarget, obstacleMask))
                        {
                            KillPlayer(target.gameObject);
                        }
                    }
                }
            }
        }

        void KillPlayer(GameObject player)
        {
            Debug.Log($"<color=red>GAME OVER! Player found by {gameObject.name}</color>");

            // ここに実際のゲームオーバー処理を書く
            // 例: SceneManager.LoadScene("GameOver");
            // ここでは 即死 = オブジェクト削除 とする。
            Destroy(player);
        }

        // エディタ拡張・デバッグ表示（Gizmos）
        // ============================================================

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 移動パターンの可視化（黄色）
            Gizmos.color = Color.yellow;
            Vector3 center = Application.isPlaying ? initialPosition : transform.position;

            switch (movementType)
            {
                case MovementType.PatrolLinear:
                    Vector3 start = center - (linearDirection.normalized * linearDistance);
                    Vector3 end = center + (linearDirection.normalized * linearDistance);
                    Gizmos.DrawLine(start, end);
                    Gizmos.DrawWireSphere(start, 0.2f);
                    Gizmos.DrawWireSphere(end, 0.2f);
                    break;

                case MovementType.PatrolCircular:
                    Handles.color = new Color(1, 1, 0, 0.1f);
                    Handles.DrawWireDisc(center, Vector3.up, circleRadius);
                    break;

                case MovementType.RandomWander:
                    Handles.color = new Color(1, 1, 0, 0.1f);
                    Handles.DrawSolidDisc(center, Vector3.up, wanderRadius);
                    break;

                case MovementType.PatrolLoop:
                case MovementType.PatrolPingPong:
                    // Waypointsの可視化
                    if (waypoints != null && waypoints.Length > 0)
                    {
                        Gizmos.color = Color.cyan;
                        for (int i = 0; i < waypoints.Length; i++)
                        {
                            if (waypoints[i] != null)
                            {
                                Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);
                                if (i < waypoints.Length - 1)
                                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);

                                // ループ線
                                if (i == waypoints.Length - 1 && movementType == MovementType.PatrolLoop)
                                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                            }
                        }
                    }
                    break;

            }

            // 視界範囲（高さ考慮）
            Vector3 eyePos = Application.isPlaying ? transform.position + Vector3.up * eyeHeight : transform.position + Vector3.up * eyeHeight;
            if (viewLight == null)
            {
                Handles.color = new Color(1, 0, 0, 0.2f);
                Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
                Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);
                Handles.DrawSolidArc(eyePos, Vector3.up, viewAngleA, viewAngle, viewRadius);
            }
            // 目の位置を緑の球で表示
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(eyePos, 0.1f);
        }

        // 角度からベクトルを算出するヘルパー関数
        private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
        {
            if (!angleIsGlobal)
            {
                angleInDegrees += transform.eulerAngles.y;
            }
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
        }
#endif
    }
}
