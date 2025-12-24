using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityJam.Core;

namespace UnityJam.UI
{
    /// <summary>
    /// 宝箱を開けた時に、残数のヒントを画面に一時表示するクラス
    /// </summary>
    public class TreasureHintUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("ヒントを表示するテキストコンポーネント")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Settings")]
        [Tooltip("表示し続ける時間（秒）")]
        [SerializeField] private float displayDuration = 3.0f;

        [Tooltip("フェードアウトにかかる時間（秒）")]
        [SerializeField] private float fadeDuration = 1.0f;

        private Coroutine currentCoroutine;
        private CanvasGroup canvasGroup;

        void Start()
        {
            // CanvasGroupのアタッチ確認（なければつける）
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // 最初は非表示
            canvasGroup.alpha = 0f;

            // マネージャーのイベントを監視
            if (TreasureManager.Instance != null)
            {
                // 宝箱の数が減ったら ShowMessage を呼ぶ
                TreasureManager.Instance.OnTreasureCountChanged += ShowMessage;
            }
        }

        void OnDestroy()
        {
            // オブジェクト破棄時にイベント購読を解除（エラー防止）
            if (TreasureManager.Instance != null)
            {
                TreasureManager.Instance.OnTreasureCountChanged -= ShowMessage;
            }
        }

        // ヒントを表示する処理
        void ShowMessage()
        {
            if (messageText == null) return;

            // マネージャーからヒントの文言をもらう
            string hint = TreasureManager.Instance.GetTreasureHint();
            messageText.text = hint;

            // 前の表示処理が残っていたら止める
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);

            // 新しく表示アニメーションを開始
            currentCoroutine = StartCoroutine(DisplayRoutine());
        }

        IEnumerator DisplayRoutine()
        {
            // 1. パッと表示
            canvasGroup.alpha = 1f;

            // 2. 待機
            yield return new WaitForSeconds(displayDuration);

            // 3. フェードアウト
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }
    }
}
