using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityJam
{
    public class InteractHitBox : MonoBehaviour
    {
        // 書きかけ！！

        [Header("Layer")]
        [SerializeField] LayerMask wallLayers;
        [SerializeField] LayerMask interactLayer;

        GameObject interactObject;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer != interactLayer) return;


        }
    }

}
