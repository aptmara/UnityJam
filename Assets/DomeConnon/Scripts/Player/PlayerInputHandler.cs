/*********************************************************************/
/**
 * @file   PlayerInputHandler.cs
 * @brief  プレイヤー入力（Move / Look）を取得して公開する
 *
 * Responsibility:
 * - New Input System から Move/Look を読み取る
 * - 他コンポーネントへ「値」だけ提供する
 *
 * Notes:
 * - 入力の解釈や移動・回転の処理は担当しない
 */
/*********************************************************************/
using UnityEngine;
using UnityEngine.InputSystem;

namespace DomeCannon
{
    /// <summary>
    /// プレイヤー入力（Move / Look）を取得して公開する
    /// </summary>
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        private PlayerInputActions actions;

        /// <summary>移動入力（X=左右, Y=前後）</summary>
        public Vector2 Move { get; private set; }

        /// <summary>視点入力（X=Yaw, Y=Pitch）</summary>
        public Vector2 Look { get; private set; }

        private void Awake()
        {
            actions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            actions.Enable();
            actions.Player.Move.performed += OnMove;
            actions.Player.Move.canceled += OnMove;

            actions.Player.Look.performed += OnLook;
            actions.Player.Look.canceled += OnLook;
        }

        private void OnDisable()
        {
            actions.Player.Move.performed -= OnMove;
            actions.Player.Move.canceled -= OnMove;

            actions.Player.Look.performed -= OnLook;
            actions.Player.Look.canceled -= OnLook;

            actions.Disable();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            Move = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }
    }
}
