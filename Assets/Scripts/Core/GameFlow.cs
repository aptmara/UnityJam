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

            if (spawnOnStart)
            {
                SpawnPlayer();
            }
        }

        [ContextMenu("Spawn Player Now")]
        public void SpawnPlayer()
        {
            if (startPoint == null || playerSpawnTarget == null)
            {
                Debug.LogWarning("StartPoint または PlayerSpawnTarget が未設定です。", this);
                return;
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
        }
    }
}
