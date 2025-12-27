using UnityEngine;
using System.Collections.Generic;

namespace UnityJam.Core
{
    /// <summary>
    /// GameStateの変更を検知して、GameObjectの表示/非表示を切り替えるクラス
    /// </summary>
    public class GameStateListener : MonoBehaviour
    {
        [System.Serializable]
        public struct StateObjPair
        {
            public GameState state;
            public GameObject targetObject;
        }

        [SerializeField] private List<StateObjPair> stateObjects;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnStateChanged;
                // 初期状態反映
                OnStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(GameState newState)
        {
            foreach (var pair in stateObjects)
            {
                if (pair.targetObject != null)
                {
                    bool isActive = (pair.state == newState);
                    
                    // 既にアクティブ状態が同じなら変更しない（アニメーション等がリセットされるのを防ぐ）
                    if (pair.targetObject.activeSelf != isActive)
                    {
                        pair.targetObject.SetActive(isActive);
                    }
                }
            }
        }
    }
}
