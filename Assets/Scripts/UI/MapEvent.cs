using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Unity.UI
{
    public struct IconData
    {
        public Sprite texture;
        public Transform uiTransform;

        public IconData(Sprite a_texture, Transform a_uiTransform)
        {
            texture = a_texture;
            uiTransform = a_uiTransform;
        }
    }

    public static class MiniMapEvents
    {
        public static Action<IconData> OnRegister;
        public static Action<IconData> OnUnregister;
    }
}


