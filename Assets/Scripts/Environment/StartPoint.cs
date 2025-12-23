using UnityEngine;

namespace UnityJam.Environment
{
    /// <summary>
    /// スタート地点を示すマーカー。
    /// </summary>
    public sealed class StartPoint : MonoBehaviour
    {
        public Transform PointTransform => transform;
    }
}
