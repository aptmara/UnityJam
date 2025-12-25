using UnityEngine;
using System.Collections;

namespace UnityJam.Credits
{
    public class FlashEffect : MonoBehaviour
    {
        private Renderer _renderer;
        private Color _originalColor;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null)
            {
                if (_renderer.material.HasProperty("_Color"))
                {
                    _originalColor = _renderer.material.color;
                }
                else if (_renderer.material.HasProperty("_BaseColor")) // URP
                {
                    _originalColor = _renderer.material.GetColor("_BaseColor");
                }
            }
        }

        public void Flash()
        {
            if (_renderer == null) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            // Set White
            SetColor(Color.white);
            yield return new WaitForSeconds(0.05f);
            // Revert
            SetColor(_originalColor);
            _flashRoutine = null;
        }

        private void SetColor(Color c)
        {
            if (_renderer.material.HasProperty("_Color"))
                _renderer.material.color = c;
            else if (_renderer.material.HasProperty("_BaseColor"))
                _renderer.material.SetColor("_BaseColor", c);
        }
    }
}
