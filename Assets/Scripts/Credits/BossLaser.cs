using UnityEngine;

namespace UnityJam.Credits
{
    /// @brief ボスレーザーのダメージ設定。
    /// @author 山内陽
    public class BossLaser : MonoBehaviour
    {
        [SerializeField] private float damage = 1f;

        public float Damage => damage;
    }
}
