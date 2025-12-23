using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam;

namespace UnityJam
{
    public class LightRigController : MonoBehaviour
    {
        [SerializeField] CameraRigController cameraRig;
        [SerializeField] Transform targetTransform;


        [SerializeField] Vector3 lightOffset;
        [SerializeField] Transform lightTransform;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {


            Vector3 rotation = new Vector3(cameraRig.pitch, targetTransform.rotation.eulerAngles.y, 0.0f);

            lightTransform.rotation = Quaternion.Euler(rotation);

            Vector3 worldOffset = targetTransform.rotation * lightOffset;
            lightTransform.position = targetTransform.position + worldOffset;
        }
    }
}
