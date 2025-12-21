/*********************************************************************/
/**
 * @file   Projectile.cs
 * @brief  直線移動する弾（寿命管理＋接触通知）
 *
 * Responsibility:
 * - 指定方向へ一定速度で直線移動する
 * - 寿命（秒）経過でプールへ返却する
 * - Trigger接触時に PlayerHitReceiver へ通知する
 *
 * Notes:
 * - Instantiate/Destroy は行わず、ProjectilePoolへ返却する
 */
/*********************************************************************/
using UnityEngine;

namespace DomeCannon
{
    /// <summary>
    /// 直線移動する弾（寿命管理＋接触通知）
    /// </summary>
    public sealed class Projectile : MonoBehaviour
    {
        [SerializeField]
        private float defaultLifetimeSec = 6.0f;

        private ProjectilePool ownerPool;
        private Vector3 velocity;
        private float lifeTimerSec;
        private bool active;

        /// <summary>
        /// 弾を初期化して発射状態にする
        /// </summary>
        /// <param name="pool">返却先プール</param>
        /// <param name="position">初期位置（world）</param>
        /// <param name="direction">移動方向（world、正規化推奨）</param>
        /// <param name="speed">移動速度[m/s]。0以上</param>
        /// <param name="lifetimeSec">寿命[sec]。0以上</param>
        public void Activate(ProjectilePool pool, Vector3 position, Vector3 direction, float speed, float lifetimeSec)
        {
            ownerPool = pool;
            transform.position = position;

            Vector3 dir = (direction.sqrMagnitude > 0.0001f) ? direction.normalized : Vector3.forward;
            velocity = dir * Mathf.Max(0.0f, speed);

            lifeTimerSec = Mathf.Max(0.0f, lifetimeSec);
            active = true;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 弾を非アクティブ化してプールへ返却する
        /// </summary>
        public void Deactivate()
        {
            active = false;
            gameObject.SetActive(false);

            if (ownerPool != null)
            {
                ownerPool.Return(this);
            }
        }

        private void Update()
        {
            if (!active)
            {
                return;
            }

            // Step 1: 移動
            transform.position += velocity * Time.deltaTime;

            // Step 2: 寿命
            lifeTimerSec -= Time.deltaTime;
            if (lifeTimerSec <= 0.0f)
            {
                Deactivate();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!active)
            {
                return;
            }

            // Step 1: プレイヤー側の受け口があれば通知
            PlayerHitReceiver receiver = other.GetComponent<PlayerHitReceiver>();
            if (receiver != null)
            {
                receiver.NotifyHit();
                Deactivate();
            }
        }

        private void OnDisable()
        {
            active = false;
        }

        /// <summary>
        /// 既定寿命[sec]を取得する
        /// </summary>
        public float DefaultLifetimeSec => defaultLifetimeSec;
    }
}
