using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using UnityEngine;

public class MapIcon : MonoBehaviour
{
    [SerializeField]
    Sprite icon;


    void OnEnable()
    {
        MiniMapEvents.OnRegister?.Invoke(new IconData(icon,transform));
    }

    void OnDisable()
    {
        MiniMapEvents.OnUnregister?.Invoke(new IconData(icon, transform));
    }

}
