using UnityEngine;

namespace UnityJam
{
    public class CameraController : MonoBehaviour
    {
        private Camera cam;

        [SerializeField] private float fieldOfViewDegree = 60f;

        private enum FOVMode
        {
            Vertical,
            Horizontal
        };

        [SerializeField] private FOVMode fovMode = FOVMode.Vertical;

        // ★Death演出などで外部がFOVを触る間、上書きを止める
        [SerializeField] private bool lockFov = false;

        public void SetFovLock(bool locked)
        {
            lockFov = locked;
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError($"{nameof(CameraController)} は Camera を持つ GameObject に付けてください。", this);
            }
        }

        private void FixedUpdate()
        {
            if (cam == null) return;
            if (lockFov) return; 

            float currentAspectRatio = cam.aspect;
            float verticalFov = 0f;

            switch (fovMode)
            {
                case FOVMode.Vertical:
                    verticalFov = fieldOfViewDegree;
                    break;

                case FOVMode.Horizontal:
                    verticalFov = HorizontalToVerticalFov(fieldOfViewDegree, currentAspectRatio);
                    break;
            }

            cam.fieldOfView = verticalFov;
        }

        private float HorizontalToVerticalFov(float horizontalFov, float aspectRatio)
        {
            return 2f * Mathf.Rad2Deg * Mathf.Atan(Mathf.Tan(horizontalFov * 0.5f * Mathf.Deg2Rad) / aspectRatio);
        }
    }
}
