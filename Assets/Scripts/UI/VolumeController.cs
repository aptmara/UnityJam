using UnityEngine;
using UnityEngine.UI;
using UnityJam.Core;

namespace UnityJam.UI
{
    [RequireComponent(typeof(Slider))]
    public class VolumeController : MonoBehaviour
    {
        private Slider _slider;

        private void Start()
        {
            _slider = GetComponent<Slider>();
            
            if (SoundManager.Instance != null)
            {
                // Initialize slider value
                _slider.value = SoundManager.Instance.GetMasterVolume();
                
                // Add listener
                _slider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }

        private void OnVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetMasterVolume(value);
            }
        }
    }
}
