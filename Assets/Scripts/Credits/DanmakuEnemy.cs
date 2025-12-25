using UnityEngine;
using System.Collections;

namespace UnityJam.Credits
{
    public class DanmakuEnemy : MonoBehaviour
    {
        private Transform player;
        private float speed = 5f;
        private float hp = 3;
        private float fireTimer;
        private bool isDead = false;

        public void Setup(Transform playerTransform)
        {
            player = playerTransform;
            Destroy(gameObject, 10f);
            gameObject.AddComponent<FlashEffect>();
        }



        public void TakeDamage(float dmg)
        {
            var flash = GetComponent<FlashEffect>();
            if (flash != null) flash.Flash();

            hp -= dmg;
            if (hp <= 0 && !isDead)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;


            GameObject exp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            exp.transform.position = transform.position;
            exp.transform.localScale = Vector3.zero;




            Destroy(exp.GetComponent<Collider>());
            var rend = exp.GetComponent<Renderer>();
            rend.material.color = Color.yellow;



            GameObject particles = new GameObject("ExplosionParticles");
            particles.transform.position = transform.position;
            var ps = particles.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 10f;
            main.startSize = 0.5f;
            main.maxParticles = 20;
            main.loop = false;
            main.playOnAwake = true;
            ps.Emit(20);
            Destroy(particles, 1.0f);

            Destroy(exp, 0.1f);


            if (CreditGameManager.Instance != null) CreditGameManager.Instance.AddScore(500);

            Destroy(gameObject);
        }


        private void OnTriggerEnter(Collider other)
        {
            var bullet = other.GetComponent<DanmakuBullet>();
            if (bullet != null && bullet.IsPlayerBullet)
            {
                TakeDamage(1);
                Destroy(bullet.gameObject);
            }
        }
    }
}
