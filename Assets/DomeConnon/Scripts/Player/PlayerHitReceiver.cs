/*********************************************************************/
/**
 * @file   PlayerHitReceiver.cs
 * @brief  弾接触の通知を受け取り、ゲーム敗北へ遷移させる
 *
 * Responsibility:
 * - Projectile からの接触通知を受ける
 * - GameFlow に Failed 遷移を依頼する
 *
 * Notes:
 * - FindObjectOfType 等の探索に依存しない
 * - 参照はInspectorで設定するのが基本
 * - 参照未設定時は、親から拾える範囲のみ補完する（安全策）
 */
/*********************************************************************/
using UnityEngine;

namespace DomeCannon
{
    /// <summary>
    /// 弾接触の通知を受け取り、ゲーム敗北へ遷移させる
    /// </summary>
    public sealed class PlayerHitReceiver : MonoBehaviour
    {
        [SerializeField]
        private GameFlow gameFlow;

        /// <summary>
        /// 弾に当たったことを通知する
        /// </summary>
        public void NotifyHit()
        {
            if (gameFlow == null)
            {
                return;
            }

            // Step 1: プレイ中以外は無視（Ready/Result中に当たっても意味がない）
            if (gameFlow.CurrentState != GameFlow.State.Playing)
            {
                return;
            }

            // Step 2: 失敗へ遷移（GameFlow側でResultへつなぐ）
            gameFlow.ChangeState(GameFlow.State.Failed);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Step 1: 参照未設定なら親から拾える範囲のみ補完
            if (gameFlow == null)
            {
                gameFlow = GetComponentInParent<GameFlow>();
            }
        }
#endif

        private void Reset()
        {
            // Step 1: 追加直後の補完
            gameFlow = GetComponentInParent<GameFlow>();
        }
    }
}
