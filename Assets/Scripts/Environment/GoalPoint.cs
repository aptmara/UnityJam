using System;
using UnityEngine;

namespace UnityJam.Environment
{
    /// <summary>
    /// ゴール地点。Trigger侵入で到達通知を行う。
    /// プレイヤー判定は Tag = "Player" を使用する。
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class GoalPoint : MonoBehaviour
    {
        public event Action<GameObject> OnReached;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Tag運用（現状の方針に合わせる）
            if (!other.CompareTag("Player"))
            {
                return;
            }

            OnReached?.Invoke(other.gameObject);
        }
    }
}
