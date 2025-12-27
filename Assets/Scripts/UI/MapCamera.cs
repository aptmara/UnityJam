using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityJam.UI
{
    public class MapCamera : MonoBehaviour
    {
        [SerializeField]
        Transform targetTransform;

        private void OnEnable()
        {
            MapUIEvents.OnUIRequested += Register;
        }

        private void OnDisable()
        {
            MapUIEvents.OnUIRequested -= Register;
        }

        void Register(MapMask mask)
        {
            mask.CameraSetting(this.GetComponent<Camera>(), targetTransform);
        }

    }


}
