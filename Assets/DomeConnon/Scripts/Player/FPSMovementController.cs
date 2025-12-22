/*********************************************************************/
/**
 * @file   FPSMovementController.cs
 * @brief  CharacterController を用いたFPS移動
 *
 * Responsibility:
 * - 入力方向をプレイヤーYawに合わせてワールド方向へ変換
 * - CharacterController.Move で移動する
 *
 * Notes:
 * - 重力・ジャンプは次ステップで追加する
 */
/*********************************************************************/
using UnityEngine;

namespace DomeCannon
{
    /// <summary>
    /// CharacterController を用いたFPS移動を担当する
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public sealed class FPSMovementController : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 6.0f;

        private CharacterController controller;
        private PlayerInputHandler input;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            input = GetComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            // Step 1: 入力取得
            Vector2 move = input.Move;

            // Step 2: ローカル移動ベクトル生成
            Vector3 local = new Vector3(move.x, 0.0f, move.y);

            // Step 3: プレイヤー向き（Yaw）に合わせてワールドへ変換
            Vector3 world = transform.TransformDirection(local);

            // Step 4: 移動反映（重力なしの暫定）
            controller.Move(world * (moveSpeed * Time.deltaTime));
        }
    }
}
