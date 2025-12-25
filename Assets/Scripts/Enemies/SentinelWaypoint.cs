using System.Collections.Generic;
using UnityEngine;

namespace UnityJam.Enemies
{
    /// <summary>
    /// 敵が移動する経由地点。隣接するポイントをつなぐことでルートを作る。
    /// </summary>
    public class SentinelWaypoint : MonoBehaviour
    {
        [Header("Connected Points")]
        [Tooltip("ここから移動可能な隣のポイント")]
        public List<SentinelWaypoint> neighbors = new List<SentinelWaypoint>();

        [Header("Debug")]
        [Tooltip("ギズモの色")]
        public Color gizmoColor = Color.cyan;

        // シーンビューで可視化するためのギズモ描画
        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            // 点を描画
            Gizmos.DrawSphere(transform.position, 0.3f);

            // つながっている線を描画
            if (neighbors != null)
            {
                Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
                foreach (var neighbor in neighbors)
                {
                    if (neighbor != null)
                    {
                        Gizmos.DrawLine(transform.position, neighbor.transform.position);
                    }
                }
            }
        }
    }
}
