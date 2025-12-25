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
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private ParticleSystem fixedEffect;
        [SerializeField] private GameObject monolithVisual;

        [Header("Settings")]
        [SerializeField] private Color buggedColor = Color.red;
        [SerializeField] private Color lockedColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color fixedColor = Color.cyan;

        public bool CanBeLocked { get; private set; }
        public bool IsFixed { get; private set; }

        private string creditName;
        private string creditRole;


        private bool isMonolith;
        private float maxHP = 30f;
        private float currentHP;


        private List<CreditGameManager.CreditSection> allSections;
        private int currentPhaseIndex = 0;
        private float hpPerPhase = 200f;

        private bool isInvincible = false;
        private bool isHiding = false;


        private bool hasFinishedNamesInPhase = false;

        private void Awake()
        {
            if (textMesh == null) textMesh = GetComponentInChildren<TextMeshPro>(true);


            var col = GetComponent<Collider>();
            if (col == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(4, 4, 1);
            }


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
            yield return new WaitForSeconds(1.0f);

            int lastPhaseIndex = -1;
            int nameIndex = 0;

            while (!IsFixed)
            {
                if (allSections == null || currentPhaseIndex >= allSections.Count) yield break;


                if (currentPhaseIndex != lastPhaseIndex)
                {
                    lastPhaseIndex = currentPhaseIndex;
                    nameIndex = 0;
                    hasFinishedNamesInPhase = false;
                    yield return new WaitForSeconds(1.0f);
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

                    hasFinishedNamesInPhase = true;
                }

                yield return new WaitForSeconds(3.0f);
            }
        }

        private void Update()
        {
            if (isMonolith && !IsFixed && textMesh != null && textMesh.gameObject.activeSelf)
            {

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


            if (CreditGameManager.Instance != null) CreditGameManager.Instance.AddScore(10);


            float nextPhaseThreshold = maxHP - (currentPhaseIndex + 1) * hpPerPhase;
            bool isDefeated = false;

            if (currentHP <= nextPhaseThreshold)
            {

                if (!hasFinishedNamesInPhase)
                {





                    currentHP = nextPhaseThreshold + 1f;

                    if (monolithVisual != null)
                    {


                    }





                }


                currentHP = nextPhaseThreshold;

                if (currentPhaseIndex < allSections.Count - 1)
                {
                   StartCoroutine(PhaseTransitionRoutine());
                }
                else
                {

                   if (currentHP <= 0) isDefeated = true;
                }
            }


            if (CreditGameManager.Instance != null) CreditGameManager.Instance.UpdateBossHP(currentHP, maxHP);


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


            fireTimer = 0f;


            if (monolithVisual != null) monolithVisual.SetActive(false);
            var col = GetComponent<Collider>();
            if (col != null) col.enabled = false;


            yield return new WaitForSeconds(3.0f);


            if (CreditGameManager.Instance != null) CreditGameManager.Instance.AddScore(5000);
            currentPhaseIndex++;
            SetupPhaseData();


            Vector3 newPos = new Vector3(Random.Range(-5f, 5f), Random.Range(2f, 4f), 0);
            transform.position = newPos;


            if (monolithVisual != null) monolithVisual.SetActive(true);

            var flash = monolithVisual?.GetComponent<FlashEffect>();
            if (flash != null) flash.Flash();

            yield return new WaitForSeconds(1.0f);

            if (col != null) col.enabled = true;
            isHiding = false;
            isInvincible = false;
        }

        private float fireTimer;
        private float fireInterval = 0.8f;
        private int patternIndex = 0;


        private Vector3 targetPos;
        private float moveSpeed = 2.0f;
        private float moveTimer = 0f;
        private float moveInterval = 4.0f;

        private void HandleBossBehavior()
        {

            fireTimer += Time.deltaTime;
            if (fireTimer > fireInterval)
            {

                 fireInterval = 0.7f + Random.Range(0f, 0.2f);
                 fireTimer = 0f;
                 FirePattern();
            }


            moveTimer += Time.deltaTime;
            if (moveTimer > moveInterval)
            {
                moveTimer = 0f;


                float targetX = Random.Range(-6f, 6f);
                float targetY = Random.Range(2f, 4.5f);
                targetPos = new Vector3(targetX, targetY, 0f);
            }





            Vector3 currentBase = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);


            float bobY = Mathf.Sin(Time.time * 2f) * 0.2f;
            float swayX = Mathf.Cos(Time.time * 1.5f) * 0.3f;





























            if (monolithVisual != null)
            {
                 monolithVisual.transform.localPosition = new Vector3(
                     Mathf.Cos(Time.time * 1.5f) * 0.3f,
                     Mathf.Sin(Time.time * 2f) * 0.2f,
                     0
                 );
            }


            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);
        }

        private void FirePattern()
        {
            if (isHiding) return;




            int type = currentPhaseIndex % 4;

            switch(type)
            {
                case 0:
                    int r0 = Random.Range(0, 2);
                    if (r0==0) StartCoroutine(FireRandomBurst(10, 0.1f));
                    else FireNWayAimed(5, 60f);
                    break;
                case 1:
                    int r1 = Random.Range(0, 2);
                    if (r1==0) StartCoroutine(FireCircle(16));
                    else StartCoroutine(FireWave(20, 0.05f));
                    break;
                case 2:
                    if (Random.value < 0.4f) StartCoroutine(FireCenterLaser(3f, 4f));
                    else StartCoroutine(FireSpiral(30, 10f, 0.05f));
                    break;
                case 3:
                    int r3 = Random.Range(0, 4);
                    if (r3==0) StartCoroutine(FireFlower(8, 4));
                    else if (r3==1) StartCoroutine(FireCross(20, 0.05f));
                    else if (r3==2) StartCoroutine(FireCenterLaser(3f, 5f));
                    else StartCoroutine(FireRotatingLazer(40, 0.02f));
                    break;
            }
        }



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
                     float angle = p * baseAngle + (i * 5f);
                     SpawnBullet(Quaternion.Euler(0,0,angle) * Vector3.down);
                 }
                 yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator FireCross(int count, float delay)
        {

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
                textMesh.gameObject.SetActive(true);
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

            if (CreditGameManager.Instance != null) CreditGameManager.Instance.AddScore(50000);

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
                textMesh.text = $"<size=150%>{creditName}</size>";
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

            GameObject laserObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            laserObj.name = "BossIndestructibleLaser";
            laserObj.transform.position = new Vector3(0, 0, 0);
            laserObj.transform.rotation = Quaternion.Euler(90, 0, 0);









            laserObj.transform.localScale = new Vector3(0.1f, 10f, 0.1f);


            float randomX = Random.Range(-7f, 7f);
            float randomY = Random.Range(-4.5f, 4.5f);
            laserObj.transform.position = new Vector3(randomX, randomY, 0);

            var rend = laserObj.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Sprites/Default"));
            rend.material.color = new Color(1f, 0f, 1f, 0.5f);


            Destroy(laserObj.GetComponent<Collider>());
            var capsule = laserObj.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 0.5f;
            capsule.height = 2f;


            laserObj.AddComponent<BossLaser>();


            float t = 0f;
            float warnTime = 1.0f;
            while(t < warnTime && !IsFixed)
            {
                t += Time.deltaTime;
                float flicker = Mathf.PingPong(t * 10, 1f);
                rend.material.color = new Color(1f, 0f, 0f, 0.2f + 0.3f * flicker);




                yield return null;
            }

            if (IsFixed) { Destroy(laserObj); yield break; }


            rend.material.color = Color.red;
            float fireT = 0f;

            while(fireT < duration && !IsFixed)
            {
                fireT += Time.deltaTime;

                float w = Mathf.Lerp(0.1f, width, Mathf.Min(1f, fireT * 5f));
                laserObj.transform.localScale = new Vector3(w, 10f, w);




                yield return null;
            }

            Destroy(laserObj);
        }
    }
}
