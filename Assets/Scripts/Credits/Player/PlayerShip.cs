using UnityEngine;
using System.Collections.Generic;

namespace UnityJam.Credits
{
    public class PlayerShip : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float bankAmount = 15f;

        [Header("Shooting")]
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private GameObject laserPrefab;
        [SerializeField] private Transform firePoint;

        private float nextFireTime;
        private Camera gameCamera;
        private Quaternion initialRotation;

        public void Initialize(Camera cam = null)
        {
            if (cam != null) gameCamera = cam;
            else gameCamera = Camera.main;


            var col = GetComponent<Collider>();
            if (col == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = Vector3.one * 0.5f;
            }

            var rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            SetLayerRecursively(gameObject, 2);



            initialRotation = transform.rotation;

            transform.position = Vector3.zero;
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        private void Update()
        {
            HandleMovement();
            HandleShooting();
        }

        private void HandleMovement()
        {
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            Vector3 move = new Vector3(x, y, 0) * moveSpeed * Time.deltaTime;
            transform.position += move;


            if (gameCamera != null)
            {
                Vector3 viewPos = gameCamera.WorldToViewportPoint(transform.position);
                viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
                viewPos.y = Mathf.Clamp(viewPos.y, 0.05f, 0.95f);


                viewPos.z = 10f;
                transform.position = gameCamera.ViewportToWorldPoint(viewPos);


                Vector3 p = transform.position;
                p.z = 0f;
                transform.position = p;
            }



            Quaternion bank = Quaternion.Euler(x * -bankAmount, 0, y * -bankAmount);
            transform.rotation = initialRotation * bank;
        }

        private void HandleShooting()
        {
            if (Input.GetMouseButton(0) && Time.time > nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                FireBullet();
            }
        }

        private void FireBullet()
        {

             GameObject bulletObj = laserPrefab;
            if (bulletObj == null)
            {
                bulletObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bulletObj.transform.localScale = Vector3.one * 0.3f;

                Destroy(bulletObj.GetComponent<Collider>());
                var sc = bulletObj.AddComponent<SphereCollider>();
                sc.isTrigger = true;
            }
            else
            {
                bulletObj = Instantiate(laserPrefab, firePoint != null ? firePoint.position : transform.position, Quaternion.identity);
            }


            if (laserPrefab == null) bulletObj.transform.position = firePoint != null ? firePoint.position : transform.position;


            DanmakuBullet bulletScript = bulletObj.AddComponent<DanmakuBullet>();
            bulletScript.Setup(Vector3.up, bulletSpeed, true);
        }

        private void Start()
        {
            if (GetComponent<FlashEffect>() == null) gameObject.AddComponent<FlashEffect>();
        }

        private void OnTriggerEnter(Collider other)
        {

            var bullet = other.GetComponent<DanmakuBullet>();
            bool hit = false;

            if (bullet != null && !bullet.IsPlayerBullet)
            {

                hit = true;
                Destroy(bullet.gameObject);
            }

            else if (other.GetComponent<DanmakuEnemy>() != null)
            {
                 hit = true;
                 Destroy(other.gameObject);
            }
            else if (other.GetComponent<CreditObject>() != null)
            {

                hit = true;
            }
            else if (other.GetComponent<BossLaser>() != null)
            {

                hit = true;
            }

            if (hit)
            {
                var flash = GetComponent<FlashEffect>();
                if (flash != null) flash.Flash();

                if (CreditGameManager.Instance != null) CreditGameManager.Instance.TakePlayerDamage(1);
            }
        }
    }



}
