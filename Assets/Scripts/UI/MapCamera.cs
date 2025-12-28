using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityJam.UI
{
    public static class MapTargetRegister
    {
        public static event Action<Transform> OnTargetRegistered;

        public static void Register(Transform targetTransform)
        {
            OnTargetRegistered?.Invoke(targetTransform);
        }
    }


    public class MapCamera : MonoBehaviour
    {

        // 初期ターゲット
        [SerializeField]
        private Transform TargetTransform;

        public Transform targetTransform
        {
            get => TargetTransform;
            private set => TargetTransform = value;
        }

        private void OnEnable()
        {
            MapUIEvents.OnUIRequested += Register;
            MapTargetRegister.OnTargetRegistered += SetTransform;

        }

        private void OnDisable()
        {
            MapUIEvents.OnUIRequested -= Register;
        }

        void Register(MapMask mask)
        {
            mask.CameraSetting(this);
        }

        void SetTransform(Transform a_targetTransform)
        {
            targetTransform = a_targetTransform;

        }
    }


}
