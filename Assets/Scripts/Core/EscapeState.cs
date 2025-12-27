using System;
using UnityEngine;

namespace UnityJam.Core
{
    /// <summary>
    /// 脱出フラグをシーンを跨いで保持する状態クラス。
    /// </summary>
    public sealed class EscapeState : MonoBehaviour
    {
        public static EscapeState Instance { get; private set; }

        /// <summary>脱出完了フラグ</summary>
        public bool HasEscaped { get; private set; }

        /// <summary>脱出が成立した瞬間に通知</summary>
        public event Action OnEscaped;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 脱出成功を確定する（多重呼び出しは無視）。
        /// </summary>
        public void SetEscaped()
        {
            if (HasEscaped)
            {
                return;
            }

            HasEscaped = true;
            OnEscaped?.Invoke();
        }

        /// <summary>
        /// 次のランなどでリセットする（必要になったらGameFlow側から呼ぶ）。
        /// </summary>
        public void ResetState()
        {
            HasEscaped = false;
        }
    }
}
