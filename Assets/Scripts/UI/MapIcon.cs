using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using UnityEngine;

public class MapIcon : MonoBehaviour
{
    [SerializeField]
    Sprite icon;

    private void Start()
    {
 
    }

    void OnEnable()
    {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.8f);
        MiniMapEvents.OnRegister?.Invoke(new IconData(icon, transform));
     }


    private void OnDestroy()
    {

    }

    void OnDisable()
    {
        MiniMapEvents.OnUnregister?.Invoke(new IconData(icon, transform));
    }

}
