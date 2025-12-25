using UnityEngine;

namespace UnityJam.Credits
{
    [RequireComponent(typeof(Camera))]
    public class CyberRailCamera : MonoBehaviour
    {
        [Header("Rail Settings")]
        [SerializeField] private float baseSpeed = 20f;
        [SerializeField] private float boostSpeed = 80f;
        [SerializeField] private float rotationSmooth = 2f;
        
        [Header("Shake")]
        [SerializeField] private float shakeAmount = 0.5f;

        private float currentSpeed;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        
        // External control
        public float SpeedMultiplier { get; set; } = 1.0f;
        public bool IsBoost { get; set; } = false;

        private void Start()
        {
            currentSpeed = baseSpeed;
            targetRotation = transform.rotation;
        }

        private void Update()
        {
            // Forward Movement
            float targetS = IsBoost ? boostSpeed : baseSpeed;
            targetS *= SpeedMultiplier;
            currentSpeed = Mathf.Lerp(currentSpeed, targetS, Time.deltaTime * 3f);
            
            // 2D Pivot: Camera is stationary.
            // transform.position += transform.forward * currentSpeed * Time.deltaTime;
        }
    
        public System.Collections.IEnumerator Shake(float duration, float magnitude)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0.0f;
            
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                
                // Add to local position
                transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localPosition = originalPos;
        }
    }
}
