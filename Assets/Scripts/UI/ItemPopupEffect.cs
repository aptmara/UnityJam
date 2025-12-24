using UnityEngine;

namespace UnityJam.UI
{
    /// <summary>
    /// アイテム取得時に宝箱から飛び出す2Dスプライト演出
    /// </summary>
    public class ItemPopupEffect : MonoBehaviour
    {
        [Header("必須設定")]
        [Tooltip("自身のSpriteRendererをアタッチしてください")]
        [SerializeField] private SpriteRenderer iconRenderer;

        [Header("演出設定")]
        [Tooltip("アイコンの表示サイズ（ワールド単位：0.5なら0.5m四方に収まるように自動調整）")]
        [SerializeField] private float iconSize = 0.5f;

        [Tooltip("上昇する速度")]
        [SerializeField] private float moveSpeed = 2.0f;
        [Tooltip("消えるまでの時間")]
        [SerializeField] private float lifeTime = 2.0f;

        private float timer = 0f;
        private Camera targetCamera;

        // 外部から初期化する関数
        public void Initialize(Sprite sprite)
        {
            if (iconRenderer == null) iconRenderer = GetComponent<SpriteRenderer>();

            if (iconRenderer != null && sprite != null)
            {
                iconRenderer.sprite = sprite;
                iconRenderer.color = Color.white;

                // 画像サイズに合わせてスケールを自動計算する
                float maxDimension = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);

                // 0除算防止
                if (maxDimension > 0)
                {
                    // 「目標サイズ ÷ 元のサイズ」で倍率を出す
                    float newScale = iconSize / maxDimension;
                    transform.localScale = Vector3.one * newScale;
                }
                else
                {
                    // 万が一サイズが取れなかったらそのまま
                    transform.localScale = Vector3.one * iconSize;
                }
            }
        }

        void Start()
        {
            // カメラを探す
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }

        void Update()
        {
            // 1. 上昇
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // 2. カメラの方を向く
            if (targetCamera != null)
            {
                transform.rotation = targetCamera.transform.rotation;
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
