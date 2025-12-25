using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace UnityJam.Credits
{
    public class CreditObject : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private Renderer meshRenderer; // For the "Bugged" cube
        [SerializeField] private ParticleSystem fixedEffect;
        [SerializeField] private GameObject monolithVisual; // The black box/glitch block

        [Header("Settings")]
        [SerializeField] private Color buggedColor = Color.red;
        [SerializeField] private Color lockedColor = new Color(1f, 0.5f, 0f); // Orange
        [SerializeField] private Color fixedColor = Color.cyan;

        public bool CanBeLocked { get; private set; }
        public bool IsFixed { get; private set; }

        private string creditName;
        private string creditRole;
        
        // Monolith state
        private bool isMonolith;
        private float maxHP = 30f;
        private float currentHP;
        
        // Phases
        private List<CreditGameManager.CreditSection> allSections;
        private int currentPhaseIndex = 0;
        private float hpPerPhase = 200f; // Scale as needed

        private bool isInvincible = false;
        private bool isHiding = false;
        
        // Gate
        private bool hasFinishedNamesInPhase = false;
        
        private void Awake()
        {
            if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>(true);
            
            // Add Collider if missing
            var col = GetComponent<Collider>();
            if (col == null) 
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(4, 4, 1);
            }

            // Initial state
            monolithVisual?.SetActive(true);
            if (textMesh != null && textMesh.gameObject != gameObject) 
            {
                textMesh.gameObject.SetActive(false);
            }
        }

        public void SetupMonolith(List<CreditGameManager.CreditSection> sections)
        {
            allSections = sections;
            currentPhaseIndex = 0;

            maxHP = sections.Count * hpPerPhase;
            currentHP = maxHP;
            
            if (CreditGameManager.Instance != null) CreditGameManager.Instance.UpdateBossHP(currentHP, maxHP);

            isMonolith = true;
            CanBeLocked = true;
            IsFixed = false;
            

            
            // Add Flash Effect to VISUAL ONLY
            if (monolithVisual != null)
            {
                if (monolithVisual.GetComponent<FlashEffect>() == null) 
                    monolithVisual.AddComponent<FlashEffect>();
            }

 

            SetupPhaseData();
            
            monolithVisual?.SetActive(true);

            StartCoroutine(EntryRoutine());
            StartCoroutine(SpawnNamesRoutine());
        }

        private void SetupPhaseData()
        {
            if (allSections == null || currentPhaseIndex >= allSections.Count) return;

            var section = allSections[currentPhaseIndex];
            
            // Show Role
            if (textMesh != null)
            {
                textMesh.text = section.Role;
                textMesh.fontSize = 6; 
                textMesh.alignment = TextAlignmentOptions.Center;
                textMesh.gameObject.SetActive(true);
            }
        }

        private IEnumerator SpawnNamesRoutine()
        {
            yield return new WaitForSeconds(1.0f); // Wait for entry
            
            int lastPhaseIndex = -1;
            int nameIndex = 0;

            while (!IsFixed)
            {
                if (allSections == null || currentPhaseIndex >= allSections.Count) yield break;

                // Detect phase change to reset name list
                if (currentPhaseIndex != lastPhaseIndex)
                {
                    lastPhaseIndex = currentPhaseIndex;
                    nameIndex = 0;
                    hasFinishedNamesInPhase = false; // Reset for new phase
                    yield return new WaitForSeconds(1.0f); // Grace period after phase start
                }

                while (isHiding) yield return null;

                var section = allSections[currentPhaseIndex];
                if (nameIndex < section.Names.Count)
                {
                    string n = section.Names[nameIndex];
                    if (CreditGameManager.Instance != null) CreditGameManager.Instance.ShowCreditName(n);
                    nameIndex++;
                }
                else
                {
                    // All names in this section shown
                    hasFinishedNamesInPhase = true;
                }

                yield return new WaitForSeconds(3.0f);
            }
        }

        private void Update()
        {
            if (isMonolith && !IsFixed && textMesh != null && textMesh.gameObject.activeSelf)
            {
                // Wait until entry finished (collider enabled) roughly implies fighting started
                var col = GetComponent<Collider>();
                if (col != null && col.enabled)
                {
                    HandleBossBehavior();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsFixed) return;

            var bullet = other.GetComponent<DanmakuBullet>();
            if (bullet != null && bullet.IsPlayerBullet)
            {
                TakeDamage(1f);
                Destroy(bullet.gameObject);
            }
        }

        public void TakeDamage(float dmg)
        {
            if (IsFixed || isInvincible) return;

            currentHP -= dmg;
            
            // Score on hit
            if (CreditGameManager.Instance != null) CreditGameManager.Instance.AddScore(10);

            // Check limit of current phase
            float nextPhaseThreshold = maxHP - (currentPhaseIndex + 1) * hpPerPhase;
            bool isDefeated = false;
            
            if (currentHP <= nextPhaseThreshold)
            {
                // Gate: Check if names are done
                if (!hasFinishedNamesInPhase)
                {
                    // Block transition but allow score? 
                    // Actually score is added on hit above.
                    // Just clamp HP and return.
                    
                    // Keep HP slightly above threshold to indicate "enduring"
                    currentHP = nextPhaseThreshold + 1f;
                    
                    if (monolithVisual != null)
                    {
                         // Optional: Flash differently or something?
                         // For now just standard flash from below code is fine.
                    }
                    
                    // Don't process transition
                    // But we still want to update UI? Code below updates UI. 
                    // Let's fall through but skipping the transition block?
                    // Better: just clamp and let flow.
                }

                // Prevent over-damage clip
                currentHP = nextPhaseThreshold; 

                if (currentPhaseIndex < allSections.Count - 1)
                {
                   StartCoroutine(PhaseTransitionRoutine());
                }
                else
                {
                   // Last phase done
                   if (currentHP <= 0) isDefeated = true;
                }
            }
            
            // Update UI BEFORE calling OnFixed (which might hide it)
            if (CreditGameManager.Instance != null) CreditGameManager.Instance.UpdateBossHP(currentHP, maxHP);

            // Flash Visual
            if (monolithVisual != null)
            {
                var flash = monolithVisual.GetComponent<FlashEffect>();
                if (flash != null) flash.Flash();
            }

            if (isDefeated)
            {
                OnFixed();
            }
        }


        private IEnumerator PhaseTransitionRoutine()
        {
            isInvincible = true;
            isHiding = true;
            
            // Stop Firing
            fireTimer = 0f;

            // Hide Visuals
            if (monolithVisual != null) monolithVisual.SetActive(false);
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Wait (Invincibility time)
            yield return new WaitForSeconds(3.0f);

            // Advance Phase
            if (CreditGameManager.Instance != null) CreditGameManager.Instance.AddScore(5000); // Phase Bonus
            currentPhaseIndex++;
            SetupPhaseData();

            // Teleport
            Vector3 newPos = new Vector3(Random.Range(-5f, 5f), Random.Range(2f, 4f), 0);
            transform.position = newPos;

            // Show Visuals
            if (monolithVisual != null) monolithVisual.SetActive(true);
            // Flash effect for reappearance
            var flash = monolithVisual?.GetComponent<FlashEffect>();
            if (flash != null) flash.Flash();

            yield return new WaitForSeconds(1.0f);

            if (col != null) col.enabled = true;
            isHiding = false;
            isInvincible = false;
        }

        private float fireTimer;
        private float fireInterval = 0.8f; // Faster base fire
        private int patternIndex = 0;

        // Movement
        private Vector3 targetPos;
        private float moveSpeed = 2.0f;
        private float moveTimer = 0f;
        private float moveInterval = 4.0f;

        private void HandleBossBehavior()
        {
            // Firing
            fireTimer += Time.deltaTime;
            if (fireTimer > fireInterval)
            {
                 // Small random delay variation
                 fireInterval = 0.7f + Random.Range(0f, 0.2f);
                 fireTimer = 0f;
                 FirePattern();
            }

            // Active Movement
            moveTimer += Time.deltaTime;
            if (moveTimer > moveInterval)
            {
                moveTimer = 0f;
                // Pick new target (X: -6 to 6, Y: 1.5 to 4.5)
                // Default spawn Y is 8. Final pos in Entry is 3.5.
                float targetX = Random.Range(-6f, 6f);
                float targetY = Random.Range(2f, 4.5f); 
                targetPos = new Vector3(targetX, targetY, 0f);
            }

            // Smooth move to target
            // Also add Bobbing (Idle)
            
            // Base position move
            Vector3 currentBase = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
            
            // Add bobbing offset
            float bobY = Mathf.Sin(Time.time * 2f) * 0.2f;
            float swayX = Mathf.Cos(Time.time * 1.5f) * 0.3f;
            
            // We want to apply bobbing on top of base movement, but Lerp acts on current position which ALREADY has bobbing.
            // Better to track separate "basePosition".
            
            // Actually simpler: Just move towards target, and the bobbing is inherent if we oscillate the target? 
            // Or just add force. 
            // Let's use a simpler approach: 
            // transform.position = Vector3.MoveTowards(...) + Bob? No that jumps.
            
            // Proper way:
            // 1. Move "physical" position towards target.
            // 2. Visuals can bob? Or just bob variables.
            
            // Let's just Lerp the transform and add a small offset calculation each frame?
            // No, if we Lerp transform.position, we overwrite the bobbing.
            
            // OK, let's treat targetPos as the anchor.
            // We move the anchor slowly.
            // We set transform to anchor + bob.
            
            // But we don't have a separate anchor variable persistent easily without refactoring entry.
            // Let's assume current transform is close to anchor.
            
            // Alternate: Just add Velocity?
            // Let's stick to: Move towards target. 
            // To add bobbing, we vary the TARGET slightly? No.
            
            // Let's add a "hoverOffset" to the visual object instead?
            // monolithVisual is the child. We can bob IT locally!
            if (monolithVisual != null)
            {
                 monolithVisual.transform.localPosition = new Vector3(
                     Mathf.Cos(Time.time * 1.5f) * 0.3f, 
                     Mathf.Sin(Time.time * 2f) * 0.2f, 
                     0
                 );
            }

            // Main object moves to target
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
        }

        private void FirePattern()
        {
            if (isHiding) return;
            
            // Dynamic Logic based on Index
            // Cycle 4 main types: 0=Simple, 1=Circle/Wave, 2=Spiral, 3=Chaos
            
            int type = currentPhaseIndex % 4;
            
            switch(type)
            {
                case 0: // Simple / NWay
                    int r0 = Random.Range(0, 2);
                    if (r0==0) StartCoroutine(FireRandomBurst(10, 0.1f));
                    else FireNWayAimed(5, 60f);
                    break;
                case 1: // Shapes (Wave/Circle)
                    int r1 = Random.Range(0, 2);
                    if (r1==0) StartCoroutine(FireCircle(16));
                    else StartCoroutine(FireWave(20, 0.05f));
                    break;
                case 2: // Spiral
                    if (Random.value < 0.4f) StartCoroutine(FireCenterLaser(3f, 4f)); 
                    else StartCoroutine(FireSpiral(30, 10f, 0.05f));
                    break;
                case 3: // Intense (Flower/Cross/Lazer)
                    int r3 = Random.Range(0, 4);
                    if (r3==0) StartCoroutine(FireFlower(8, 4));
                    else if (r3==1) StartCoroutine(FireCross(20, 0.05f));
                    else if (r3==2) StartCoroutine(FireCenterLaser(3f, 5f)); // High chance of laser
                    else StartCoroutine(FireRotatingLazer(40, 0.02f));
                    break;
            }
        }

        // --- New Patterns ---

        private IEnumerator FireSpiral(int count, float angleStep, float delay)
        {
            float angle = 0f;
            for(int i=0; i<count; i++)
            {
                if(isHiding) yield break;
                SpawnBullet(Quaternion.Euler(0,0,angle) * Vector3.down);
                angle += angleStep;
                yield return new WaitForSeconds(delay);
            }
        }

        private IEnumerator FireFlower(int petals, int bulletsPerPetal)
        {
            for(int i=0; i<bulletsPerPetal; i++)
            {
                 if(isHiding) yield break;
                 float baseAngle = 360f / petals;
                 for(int p=0; p<petals; p++)
                 {
                     float angle = p * baseAngle + (i * 5f); // Twist
                     SpawnBullet(Quaternion.Euler(0,0,angle) * Vector3.down);
                 }
                 yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator FireCross(int count, float delay)
        {
            // Plus shape rotating? Or just 4 directions
            Vector3[] dirs = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
            
            for(int i=0; i<count; i++)
            {
                if(isHiding) yield break;
                foreach(var d in dirs)
                {
                    SpawnBullet(d);
                }
                yield return new WaitForSeconds(delay);
            }
        }

        private IEnumerator FireRotatingLazer(int count, float delay)
        {
             // 2-way rotating stream
             float angle = 0f;
             for(int i=0; i<count; i++)
             {
                 if(isHiding) yield break;
                 SpawnBullet(Quaternion.Euler(0,0, angle) * Vector3.down);
                 SpawnBullet(Quaternion.Euler(0,0, angle + 180f) * Vector3.down);
                 angle += 15f; 
                 yield return new WaitForSeconds(delay);
             }
        }

        private IEnumerator FireStraightStream(int count, float delay)
        {
            for(int i=0; i<count; i++)
            {
                float xOffset = Random.Range(-4f, 4f); 
                SpawnBullet(Vector3.down, new Vector3(xOffset, -1f, 0));
                yield return new WaitForSeconds(delay);
            }
        }

        private IEnumerator FireWave(int count, float delay)
        {
            for(int i=0; i<count; i++)
            {
                float angle = Mathf.Sin(i * 1.5f) * 45f; 
                SpawnBullet(Quaternion.Euler(0,0,angle) * Vector3.down, Vector3.zero);
                yield return new WaitForSeconds(delay);
            }
        }

        private void SpawnBullet(Vector3 dir, Vector3 offset)
        {
            GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.transform.localScale = Vector3.one * 0.4f; 
            bullet.transform.position = transform.position + offset;
            
            Destroy(bullet.GetComponent<Collider>());
            var sc = bullet.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.5f; 

            var script = bullet.AddComponent<DanmakuBullet>();
            script.Setup(dir, 12f, false);
        }

        private void SpawnBullet(Vector3 dir)
        {
            SpawnBullet(dir, Vector3.zero);
        }

        private void FireNWayAimed(int count, float arc)
        {
            Vector3 baseDir = Vector3.down;
            float startAngle = -arc / 2f;
            float step = arc / Mathf.Max(1, count - 1);
            
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (i * step);
                Quaternion rot = Quaternion.Euler(0, 0, angle); 
                SpawnBullet(rot * baseDir);
            }
        }

        private IEnumerator FireCircle(int count)
        {
            float step = 360f / count;
            for(int i=0; i<count; i++)
            {
                float angle = i * step;
                Quaternion rot = Quaternion.Euler(0, 0, angle);
                SpawnBullet(rot * Vector3.down);
            }
            yield return null;
        }

        private IEnumerator FireRandomBurst(int count, float delay)
        {
            for(int i=0; i<count; i++)
            {
                Vector3 baseDir = Vector3.down;
                Vector3 spread = Random.insideUnitCircle * 0.8f; 
                Vector3 finalDir = (baseDir + spread).normalized;
                
                SpawnBullet(finalDir);
                yield return new WaitForSeconds(delay);
            }
        }

        private IEnumerator EntryRoutine()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false; 

            Vector3 startPos = transform.position; 
            Vector3 finalPos = new Vector3(0f, 3.5f, 0f);
            
            float duration = 1.5f;
            float t = 0f;
            
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float curve = Mathf.SmoothStep(0, 1, t); 
                transform.position = Vector3.Lerp(startPos, finalPos, curve);
                yield return null;
            }

            transform.position = finalPos;
            
            if (col != null) col.enabled = true; 
            
            // Initialize targetPos
            targetPos = transform.position;
        }

        public void SetupSmallBug(string text)
        {
            creditName = text;
            isMonolith = false;
            CanBeLocked = true;
            IsFixed = false;

            monolithVisual?.SetActive(true);
             if (textMesh != null && textMesh.gameObject != gameObject)
            {
                textMesh.text = text;
                textMesh.gameObject.SetActive(true); // Always show
            }
            
            transform.localScale = Vector3.one * 0.5f;
        }

        public void OnLocked()
        {
            if (IsFixed) return;
            if (monolithVisual != null)
            {
                var mat = monolithVisual.GetComponent<Renderer>().material;
                mat.color = lockedColor;
            }
        }

        public void OnFixed()
        {
            if (IsFixed) return;
            IsFixed = true;
            CanBeLocked = false;

            if (CreditGameManager.Instance != null) CreditGameManager.Instance.AddScore(50000); // Boss Kill Bonus
            
            if (fixedEffect != null) Instantiate(fixedEffect, transform.position, Quaternion.identity);

            if (isMonolith)
            {
                StartCoroutine(RevealRoutine());
                CreditGameManager.Instance.OnMonolithFixed();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator RevealRoutine()
        {
            if (monolithVisual != null) monolithVisual.SetActive(false);
            
            if (textMesh != null)
            {
                textMesh.text = $"<size=150%>{creditName}</size>"; // Maybe use ROLE/TITLE as name now
                textMesh.color = fixedColor;
                
                float t = 0;
                while(t < 1f)
                {
                    t += Time.deltaTime * 5f;
                    float scale = Mathf.Lerp(0.5f, 1.2f, Mathf.Sin(t * Mathf.PI)); 
                    textMesh.transform.localScale = Vector3.one * scale;
                    yield return null;
                }
                textMesh.transform.localScale = Vector3.one;
            }

            yield return new WaitForSeconds(3f);
            
             if (textMesh != null)
             {
                 float alpha = 1f;
                 while(alpha > 0)
                 {
                     alpha -= Time.deltaTime;
                     textMesh.alpha = alpha;
                     yield return null;
                 }
             }
             Destroy(gameObject);
        }
        private IEnumerator FireCenterLaser(float duration, float width)
        {
            // Indicator
            GameObject laserObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laserObj.name = "BossIndestructibleLaser";
            laserObj.transform.position = new Vector3(0, 0, 0); // Center of screen X=0
            laserObj.transform.rotation = Quaternion.Euler(90, 0, 0); // Horizontal cylinder? No, vertical on screen means laying down Z?
            // Cylinder default height is 2 (Y axis). We want it to stretch vertically on screen (Y).
            // Default Cylinder is Up-Down.
            // Screen is X (Left-Right), Y (Up-Down). 
            // We want laser to shoot DOWN from boss.
            
            // Actually, we want a beam from top to bottom.
            // Pos Y=0 is center. Boss is at Y ~8?
            // If we make it long enough it covers everything.
            
            laserObj.transform.localScale = new Vector3(0.1f, 10f, 0.1f); // Thin initial
            
            // Random X pos
            float randomX = Random.Range(-7f, 7f);
            float randomY = Random.Range(-4.5f, 4.5f);
            laserObj.transform.position = new Vector3(randomX, randomY, 0); 
            
            var rend = laserObj.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Sprites/Default")); // Simple white
            rend.material.color = new Color(1f, 0f, 1f, 0.5f); // Pink warning
            
            // Collider
            Destroy(laserObj.GetComponent<Collider>());
            var capsule = laserObj.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 0.5f;
            capsule.height = 2f; // Scaled by Transform.Y (10) -> 20 units height
            
            // Component
            laserObj.AddComponent<BossLaser>();
            
            // Warning Phase
            float t = 0f;
            float warnTime = 1.0f;
            while(t < warnTime && !IsFixed)
            {
                t += Time.deltaTime;
                float flicker = Mathf.PingPong(t * 10, 1f);
                rend.material.color = new Color(1f, 0f, 0f, 0.2f + 0.3f * flicker);
                
                // Do NOT track boss
                // laserObj.transform.position = new Vector3(transform.position.x, 0, 0);
                
                yield return null;
            }
            
            if (IsFixed) { Destroy(laserObj); yield break; }
            
            // FIRE
            rend.material.color = Color.red;
            float fireT = 0f;
            
            while(fireT < duration && !IsFixed)
            {
                fireT += Time.deltaTime;
                // Expand width
                float w = Mathf.Lerp(0.1f, width, Mathf.Min(1f, fireT * 5f)); // Expand fast
                laserObj.transform.localScale = new Vector3(w, 10f, w);
                
                 // Do NOT track boss
                // laserObj.transform.position = new Vector3(transform.position.x, 0, 0);
                
                yield return null;
            }
            
            Destroy(laserObj);
        }
    }
}
