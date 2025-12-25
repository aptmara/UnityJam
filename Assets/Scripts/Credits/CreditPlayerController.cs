using UnityEngine;

namespace UnityJam.Credits
{
    /// @brief クレジット用プレイヤー操作。
    /// @author 山内陽
    public class CreditPlayerController : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private float bulletSpeed = 20f;

        [SerializeField] private float fireRate = 0.1f;
        private float nextFireTime;

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 move = new Vector3(h, v, 0).normalized * speed * Time.deltaTime;
            transform.position += move;


            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -8f, 8f);
            pos.y = Mathf.Clamp(pos.y, -4.5f, 4.5f);
            transform.position = pos;

            if ((Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space)) && Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        private void Shoot()
        {
            if (bulletPrefab != null && muzzle != null)
            {
                int[] angles = { 0, 10, -10, 20, -20 };
                foreach (int a in angles)
                {
                   SpawnBullet(Quaternion.Euler(0, 0, a));
                }
            }
        }

        private void SpawnBullet(Quaternion rot)
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzle.position, rot);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb == null) rb = bullet.AddComponent<Rigidbody>();

            rb.useGravity = false;
            rb.velocity = bullet.transform.up * bulletSpeed;

            Destroy(bullet, 3f);
            bullet.tag = "Bullet";

            if (bullet.GetComponent<Collider>() == null)
            {
                var col = bullet.AddComponent<SphereCollider>();
                col.isTrigger = true;
                col.radius = 0.2f;
            }
        }
    }
}
