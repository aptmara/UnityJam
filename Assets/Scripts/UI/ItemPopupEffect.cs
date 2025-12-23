using UnityEngine;

namespace UnityJam.UI
{
    /// <summary>
    /// アイテム取得時に宝箱から飛び出す2Dスプライト演出（シンプル版）
    /// </summary>
    public class ItemPopupEffect : MonoBehaviour
    {
        [Header("必須設定")]
        [Tooltip("自身のSpriteRendererをアタッチしてください")]
        [SerializeField] private SpriteRenderer iconRenderer;

        [Header("演出設定")]
        [Tooltip("上昇する速度")]
        [SerializeField] private float moveSpeed = 2.0f;
        [Tooltip("消えるまでの時間")]
        [SerializeField] private float lifeTime = 2.0f;

        private float timer = 0f;

        // 外部から初期化する関数
        public void Initialize(Sprite sprite)
        {
            if (iconRenderer == null)
            {
                // もし設定し忘れていたら、自動で取得を試みる
                iconRenderer = GetComponent<SpriteRenderer>();
            }

            if (iconRenderer != null)
            {
                iconRenderer.sprite = sprite;
                // 確実に見えるように色とサイズをリセット
                iconRenderer.color = Color.white;
                transform.localScale = Vector3.one * 0.5f; // 少し小さめで出現
            }
            else
            {
                Debug.LogError("[ItemPopupEffect] SpriteRendererがありません！Prefabを確認してください。");
            }
        }

        void Update()
        {
            // 1. 上昇
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // 2. カメラの方を向く（ビルボード処理）
            if (Camera.main != null)
            {
                // カメラの逆方向を向くことで、正面を向いているように見せる
                transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            }

            // 3. 時間経過で消滅
            timer += Time.deltaTime;
            if (timer >= lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
