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


            var rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;


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




        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayerBullet)
            {







            }
            else
            {


                if (other.name.Contains("Player"))
                {


                    Destroy(gameObject);
                }
            }
        }
    }
}
