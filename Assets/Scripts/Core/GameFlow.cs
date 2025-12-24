using UnityEngine;
using UnityJam.Environment;
using UnityJam.Player;
using UnityJam.UI;

namespace UnityJam.Core
{
    /// <summary>
    /// 最小のゲーム進行：
    /// - 開始時にプレイヤーを StartPoint へスポーン
    /// - Goal 到達でメッセージ表示
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private StartPoint startPoint;
        [SerializeField] private GoalPoint goalPoint;
        [SerializeField] private PlayerSpawnTarget playerSpawnTarget;
        [SerializeField] private GoalMessageView goalMessageView;

        [Header("Options")]
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private string goalMessage = "GOAL!";

        private bool goalReached;

        private void OnEnable()
        {
            if (goalPoint != null)
            {
                goalPoint.OnReached += HandleGoalReached;
            }
        }

        private void OnDisable()
        {
            if (goalPoint != null)
            {
                goalPoint.OnReached -= HandleGoalReached;
            }
        }

        private void Start()
        {
            goalReached = false;
            
            if (goalMessageView != null)
            {
                goalMessageView.Hide();
            }

            // Startでの自動スポーンは廃止し、Initialize経由で行う
        }

        public void Initialize(GameObject stageRoot)
        {
            // ステージ内の参照を動的に取得
            if (stageRoot != null)
            {
                startPoint = stageRoot.GetComponentInChildren<StartPoint>();
                goalPoint = stageRoot.GetComponentInChildren<GoalPoint>();
                
                // イベント再登録
                if (goalPoint != null)
                {
                    goalPoint.OnReached -= HandleGoalReached; // 重複防止
                    goalPoint.OnReached += HandleGoalReached;
                }
            }

            // プレイヤー参照は PlayerSpawnTarget がシーン内にあればそれを使うが、
            // 動的生成されたプレイヤーを探す必要がある場合は別途ロジックが必要。
            // ここでは PlayerSpawnTarget (CameraRigなど) はシーンに常駐していると仮定するか、
            // もしくは StageManager から渡してもらう設計が良い。
            // いったん FindFirstObjectByType で安全策をとる。
            if (playerSpawnTarget == null)
            {
                playerSpawnTarget = FindFirstObjectByType<PlayerSpawnTarget>();
            }

            if (spawnOnStart)
            {
                SpawnPlayer();
            }
        }

        [ContextMenu("Spawn Player Now")]
        public void SpawnPlayer()
        {
            if (startPoint == null)
            {
                Debug.LogWarning("StartPoint が未設定です。", this);
                return;
            }

            if (playerSpawnTarget == null)
            {
                // 再検索
                playerSpawnTarget = FindFirstObjectByType<PlayerSpawnTarget>();
                if (playerSpawnTarget == null)
                {
                    Debug.LogWarning("PlayerSpawnTarget が見つかりません。", this);
                    return;
                }
            }

            Transform t = startPoint.PointTransform;
            playerSpawnTarget.TeleportTo(t.position, t.rotation, resetVelocity: true);
        }

        private void HandleGoalReached(GameObject playerObject)
        {
            if (goalReached)
            {
                return;
            }

            goalReached = true;

            if (goalMessageView != null)
            {
                goalMessageView.Show(goalMessage);
            }

            StartCoroutine(WaitAndFinishStage());
        }

        private System.Collections.IEnumerator WaitAndFinishStage()
        {
            yield return new WaitForSeconds(2.0f);
            GameManager.Instance.ChangeState(GameState.ScoreCalc);
        }
    }
}
