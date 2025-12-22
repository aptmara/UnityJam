/*********************************************************************/
/**
 * @file   FPSCursorLockController.cs
 * @brief  FPS向けのカーソルロック制御を行う
 *
 * Responsibility:
 * - カーソルを中央固定（Locked）し、不可視化する
 * - デバッグ用途で一時解除（ESC）を提供する
 *
 * Notes:
 * - 現段階では Player 単体で完結させる
 * - 次ステップで GameFlow（Playing/Result）と連動して自動制御へ拡張する
 */
/*********************************************************************/
using UnityEngine;
using UnityEngine.InputSystem;

namespace DomeCannon
{
    /// <summary>
    /// FPS向けのカーソルロック制御を行う
    /// </summary>
    public sealed class FPSCursorLockController : MonoBehaviour
    {
        [SerializeField]
        private bool lockOnStart = true;

        [SerializeField]
        private bool allowToggleWithEsc = true;

        /// <summary>
        /// カーソルをロックして不可視化する
        /// </summary>
        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// カーソルロックを解除して可視化する
        /// </summary>
        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Start()
        {
            if (lockOnStart)
            {
                LockCursor();
            }
        }

        private void Update()
        {
            if (!allowToggleWithEsc)
            {
                return;
            }

            // Step 1: ESC入力でトグル（デバッグ用途）
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    UnlockCursor();
                }
                else
                {
                    LockCursor();
                }
            }
        }
    }
}
