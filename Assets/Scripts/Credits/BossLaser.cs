using UnityEngine;

namespace UnityJam.Credits
{
    public class BossLaser : MonoBehaviour
    {
        [SerializeField] private float damage = 1f;

        public float Damage => damage;
    }
}
