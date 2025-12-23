using UnityEngine;

namespace UnityJam.Player
{
    /// <summary>
    /// Rigidbody前提のスポーン（テレポート）処理を提供する。
    /// PlayerController が velocity を更新するため、スポーン時は速度もリセットする。
    /// </summary>
    public sealed class PlayerSpawnTarget : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;

        public void TeleportTo(Vector3 worldPos, Quaternion worldRot, bool resetVelocity = true)
        {
            if (rb != null)
            {
                // 物理挙動を安定させるため、MovePosition/MoveRotation を使う選択もあるが
                // 初回スポーンは明示代入 + 速度リセットがシンプルで事故が少ない。
                rb.position = worldPos;
                rb.rotation = worldRot;

                if (resetVelocity)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
            else
            {
                transform.SetPositionAndRotation(worldPos, worldRot);
            }
        }
    }
}
