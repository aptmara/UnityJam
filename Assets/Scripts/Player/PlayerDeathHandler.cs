using System.Collections.Generic;
using UnityEngine;

namespace UnityJam.Player
{
    /// <summary>
    /// プレイヤー死亡時の「無効化/非表示」を担当する。
    /// Destroy せず停止することで、演出中の参照切れを防ぐ。
    /// </summary>
    public sealed class PlayerDeathHandler : MonoBehaviour
    {
        [Header("--- Disable ---")]
        [Tooltip("死亡時に無効化したいコンポーネント群（PlayerController / PlayerInteractor など）")]
        [SerializeField] private List<Behaviour> componentsToDisable = new List<Behaviour>();

        [Header("--- Collision ---")]
        [Tooltip("死亡時に無効化したい Collider（CharacterController など）。未設定なら子から自動収集します。")]
        [SerializeField] private List<Collider> collidersToDisable = new List<Collider>();

        [Header("--- Visual ---")]
        [Tooltip("死亡時に非表示にしたい Renderer。未設定なら子から自動収集します。")]
        [SerializeField] private List<Renderer> renderersToHide = new List<Renderer>();

        private bool isDead;

        private void Awake()
        {
            if (renderersToHide == null || renderersToHide.Count == 0)
            {
                renderersToHide = new List<Renderer>(GetComponentsInChildren<Renderer>(true));
            }

            if (collidersToDisable == null || collidersToDisable.Count == 0)
            {
                collidersToDisable = new List<Collider>(GetComponentsInChildren<Collider>(true));
            }
        }

        private void OnEnable()
        {
            PlayerRegistry.Register(this);
        }

        private void OnDisable()
        {
            PlayerRegistry.Unregister(this);
        }

        /// <summary>
        /// 操作停止＋当たり判定停止＋見た目非表示。
        /// </summary>
        public void KillAndHide()
        {
            if (isDead) return;
            isDead = true;

            if (componentsToDisable != null)
            {
                for (int i = 0; i < componentsToDisable.Count; i++)
                {
                    if (componentsToDisable[i] != null)
                    {
                        componentsToDisable[i].enabled = false;
                    }
                }
            }

            if (collidersToDisable != null)
            {
                for (int i = 0; i < collidersToDisable.Count; i++)
                {
                    if (collidersToDisable[i] != null)
                    {
                        collidersToDisable[i].enabled = false;
                    }
                }
            }

            if (renderersToHide != null)
            {
                for (int i = 0; i < renderersToHide.Count; i++)
                {
                    if (renderersToHide[i] != null)
                    {
                        renderersToHide[i].enabled = false;
                    }
                }
            }
        }
    }
}
