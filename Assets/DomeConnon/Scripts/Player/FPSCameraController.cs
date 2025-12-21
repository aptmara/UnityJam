/*********************************************************************/
/**
 * @file   FPSCameraController.cs
 * @brief  FPS視点（Yaw/Pitch）を制御する
 *
 * Responsibility:
 * - マウス入力からYaw/Pitchを更新
 * - Pitch制限（上+60 / 下-30）を適用
 *
 * Notes:
 * - YawはPlayer本体に適用し、PitchはPivotに適用する
 */
/*********************************************************************/
using UnityEngine;

namespace DomeCannon
{
    /// <summary>
    /// FPS視点（Yaw/Pitch）を制御する
    /// </summary>
    [RequireComponent(typeof(PlayerInputHandler))]
    public sealed class FPSCameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform pitchPivot;

        [SerializeField]
        private float lookSensitivity = 0.08f;

        [SerializeField]
        private float pitchMin = -30.0f;

        [SerializeField]
        private float pitchMax = 60.0f;

        private PlayerInputHandler input;
        private float pitchDeg;

        private void Awake()
        {
            input = GetComponent<PlayerInputHandler>();
        }

        private void LateUpdate()
        {
            // Step 1: 入力取得
            Vector2 look = input.Look;

            // Step 2: Yaw適用（プレイヤー本体）
            float yawDelta = look.x * lookSensitivity;
            transform.Rotate(0.0f, yawDelta, 0.0f, Space.Self);

            // Step 3: Pitch更新（Pivot）
            float pitchDelta = -look.y * lookSensitivity;
            pitchDeg = Mathf.Clamp(pitchDeg + pitchDelta, pitchMin, pitchMax);

            if (pitchPivot != null)
            {
                pitchPivot.localRotation = Quaternion.Euler(pitchDeg, 0.0f, 0.0f);
            }
        }
    }
}
