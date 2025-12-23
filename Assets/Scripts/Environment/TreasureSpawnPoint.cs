using UnityEngine;

namespace UnityJam.Environment
{
    /// <summary>
    /// 宝箱（本物/偽物）のスポーン地点を表すマーカー。
    /// </summary>
    public sealed class TreasureSpawnPoint : MonoBehaviour
    {
        public Transform PointTransform => transform;
    }
}
