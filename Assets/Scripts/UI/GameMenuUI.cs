using UnityEngine;
using UnityEngine.UI;
using UnityJam.Core;

namespace UnityJam.UI
{
    public class GameMenuUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Button closeButton;

        // Min/Max for UI slider mapping to sensitivity
        [SerializeField] private float minSensitivity = 10f;
        [SerializeField] private float maxSensitivity = 500f;

        private void Start()
        {
            InitializeVolume();
            InitializeSensitivity();

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        private void InitializeVolume()
        {
            if (volumeSlider == null) return;

            if (SoundManager.Instance != null)
            {
                // Init value
                volumeSlider.value = SoundManager.Instance.GetMasterVolume();
                // Add listener
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }

        private void InitializeSensitivity()
        {
            if (sensitivitySlider == null) return;

            // Check GameManager
            if (GameManager.Instance != null)
            {
                // Current val
                float currentSens = GameManager.Instance.CurrentSensitivity;

                sensitivitySlider.minValue = minSensitivity;
                sensitivitySlider.maxValue = maxSensitivity;
                sensitivitySlider.value = currentSens;

                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            }
        }

        // OnVolumeChanged removed as it is handled by the prefab's VolumeController

        public void OnSensitivityChanged(float value)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetSensitivity(value);
            }
        }

        public void OnVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetMasterVolume(value);
            }
        }

        private void OnCloseClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ToggleMenu();
            }
        }
    }
}
