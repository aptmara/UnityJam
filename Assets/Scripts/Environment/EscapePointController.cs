using UnityEngine;
using UnityJam.Core;
using UnityJam.Interaction;

namespace UnityJam.Environment
{
    /// <summary>
    /// 脱出地点のインタラクト。
    /// </summary>
    public sealed class EscapePointController : InteractableBase
    {
        [Header("--- 演出 ---")]
        [SerializeField] private GameObject interactVfxPrefab;

        [SerializeField] private string debugMessage = "脱出地点を起動した！";

        protected override void OnInteractCompleted()
        {
            if (interactVfxPrefab != null)
            {
                Instantiate(interactVfxPrefab, transform.position, Quaternion.identity);
            }

            if (EscapeState.Instance != null)
            {
                EscapeState.Instance.SetEscaped();
            }
            else
            {
                Debug.LogWarning("EscapePointController: EscapeState が Scene に存在しません。@EscapeState を配置してください。", this);
            }

            Debug.Log(debugMessage);
        }
    }
}
