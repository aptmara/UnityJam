using UnityEngine;

namespace UnityJam.Credits
{
    public class DanmakuBullet : MonoBehaviour
    {
        public bool IsPlayerBullet { get; set; }
        private Vector3 direction;
        private float speed;
        private float lifeTime = 5f;

        public void Setup(Vector3 dir, float spd, bool isPlayer)
        {
            direction = dir;
            speed = spd;
            IsPlayerBullet = isPlayer;
            
            // Add Rigidbody for Trigger events
            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            
            // Visual coloring
            var rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = isPlayer ? Color.cyan : Color.magenta;
            }
            
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;

            // Simple Hit Check
            // Ideally use Trigger Collision, but Linecast is fine for high speed
            // Using Trigger for now to support various shapes
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayerBullet)
            {
                // Rely on CreditObject's OnTriggerEnter to handle damage
                // var credit = other.GetComponentInParent<CreditObject>();
                // if (credit != null)
                // {
                //    // credit.OnFixed(); // BAD!
                //    // Destroy(gameObject); // Let CreditObject destroy it
                // }
            }
            else // Enemy Bullet
            {
                // Hit Player?
                // Just destroy for now, or add PlayerHealth later
                if (other.name.Contains("Player")) // Simple tag check
                {
                    // Player Hit!
                    // Screen shake or effect?
                    Destroy(gameObject);
                }
            }
        }
    }
}
