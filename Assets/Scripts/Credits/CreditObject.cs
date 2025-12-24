using UnityEngine;
using TMPro;

namespace UnityJam.Credits
{
    public class CreditObject : MonoBehaviour
    {
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private int hp = 3;

        private bool isEnemy;
        private Transform playerTransform;
        private GameObject bulletPrefab;
        private float fireTimer;
        private float fireInterval = 2.0f;
        

        private enum State { Waiting, Entrance, Idle, Attack }
        private State currentState = State.Waiting;
        private Vector3 attackDir;
        private Vector3 targetLocalPos;

        public void Setup(string text, bool isTitle, bool isEnemy = false, Transform player = null, GameObject bullet = null)
        {
            this.isEnemy = isEnemy;
            this.playerTransform = player;
            this.bulletPrefab = bullet;
            
            if (textMesh != null)
            {
                textMesh.text = text;
                if (isTitle)
                {
                    textMesh.color = Color.yellow;
                    textMesh.fontSize = 6;
                    textMesh.fontStyle = FontStyles.Italic;
                }
                else
                {
                    textMesh.color = Color.white;
                    // If regular text (Description or Name), differentiate?
                    // Passed isTitle distinguishes Header/Role vs Entry content.
                    // Entry content (Description vs Name) is not distinguished by isTitle.
                    // But usually Description is small.
                    // Let's rely on Manager passing correct bool or just style.
                    // For now, simple white is fine for Desc/Name.
                    textMesh.fontSize = 8; 
                    textMesh.fontStyle = FontStyles.Bold;
                }

                textMesh.ForceMeshUpdate();
                var col = GetComponent<BoxCollider>();
                if (col == null) col = gameObject.AddComponent<BoxCollider>();
                col.center = textMesh.textBounds.center;
                col.size = textMesh.textBounds.size;
                col.isTrigger = true;
            }

            // Setup Entrance for EVERYONE
            targetLocalPos = transform.localPosition;
            
            // Random start position (Left/Right/Top)
            float startX = Random.Range(0, 2) == 0 ? -15f : 15f;
            float startYOffset = Random.Range(-5f, 5f);
            transform.localPosition = new Vector3(startX, targetLocalPos.y + startYOffset, 0);
            
            currentState = State.Waiting;
        }

        private void Update()
        {
            float wobble = Mathf.Sin(Time.time * 2f + transform.position.y) * 0.002f;

            switch (currentState)
            {
                case State.Waiting:
                    // Check if we are near the top of the screen (World Y < 10)
                    // Parent moves down. We are at local Y (positive).
                    // World Y = Parent Y + Local Y.
                    if (transform.parent != null)
                    {
                         float worldY = transform.parent.position.y + targetLocalPos.y;
                         if (worldY < 6f) // Trigger when entering screen
                         {
                             currentState = State.Entrance;
                         }
                    }
                    break;

                case State.Entrance:
                    transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * 5f);
                    if (Vector3.Distance(transform.localPosition, targetLocalPos) < 0.1f)
                    {
                        transform.localPosition = targetLocalPos; // Snap
                        currentState = State.Idle;
                    }
                    break;

                case State.Idle:
                    // Wobble
                    transform.localPosition = targetLocalPos + new Vector3(Mathf.Sin(Time.time * 3f + targetLocalPos.y) * 0.5f, 0, 0);
                    // Note: Overwriting localPosition with wobble calculation based on targetLocalPos prevents drift.
                    
                    if (!isEnemy) return;

                    // Shooting Logic
                    if (playerTransform != null && bulletPrefab != null)
                    {
                        if (transform.position.y < 6f && transform.position.y > -6f)
                        {
                            fireTimer += Time.deltaTime;
                            if (fireTimer >= fireInterval)
                            {
                                fireTimer = 0f;
                                ShootAtPlayer();
                            }
                            
                            // Attack Chance
                            if (Random.Range(0, 800) == 0) 
                            {
                                StartAttack();
                            }
                        }
                    }
                    break;

                case State.Attack:
                    transform.position += attackDir * 8f * Time.deltaTime; // Faster attack
                    transform.Rotate(0, 0, 720f * Time.deltaTime);
                    
                    if (Mathf.Abs(transform.position.y) > 10f || Mathf.Abs(transform.position.x) > 14f)
                    {
                        Destroy(gameObject);
                    }
                    break;
            }
        }

        private void StartAttack()
        {
            if (playerTransform == null) return;
            currentState = State.Attack;
            transform.SetParent(null); // Detach from scroller
            
            // Aim at player
            attackDir = (playerTransform.position - transform.position).normalized;
            
            // Effect?
            if (textMesh != null) textMesh.color = Color.red;
        }

        private void ShootAtPlayer()
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            
            // 3-way aimed shot
            SpawnBullet(Quaternion.Euler(0, 0, angle));
            SpawnBullet(Quaternion.Euler(0, 0, angle + 20));
            SpawnBullet(Quaternion.Euler(0, 0, angle - 20));
        }

        private void SpawnBullet(Quaternion rot)
        {
            GameObject b = Instantiate(bulletPrefab, transform.position, rot);
            Rigidbody rb = b.GetComponent<Rigidbody>();
            if (rb == null) rb = b.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.velocity = b.transform.up * 5f; // Slower enemy bullets
            
            Destroy(b, 5f);
            b.tag = "EnemyBullet"; // Tag for player to take damage (needs implementation)
            
            if (b.GetComponent<Collider>() == null)
            {
                var col = b.AddComponent<SphereCollider>();
                col.radius = 0.2f;
                col.isTrigger = true;
            }
            // Ensure safe against self? Layer collision matrix or ignore collision
        }

        private void OnTriggerEnter(Collider other)
        {
            // Simple check for "PlayerBullet" tag or similar
            if (other.CompareTag("Bullet"))
            {
                TakeDamage(1);
                Destroy(other.gameObject); // Destroy bullet
            }
        }

        private void TakeDamage(int damage)
        {
            hp -= damage;
            if (hp <= 0)
            {
                Explode();
            }
            else
            {
                // Hit reaction
                transform.position += Random.insideUnitSphere * 0.2f;
                if (textMesh != null)
                {
                    // Flash red
                    StartCoroutine(FlashColor());
                }
            }
        }

        private System.Collections.IEnumerator FlashColor()
        {
            Color original = textMesh.color;
            textMesh.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            textMesh.color = original;
        }

        private void Explode()
        {
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            Destroy(gameObject);
        }
    }
}
