using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Core;         // Inventoryを使うため
using UnityJam.Items;        // ItemMasterを使うため
using UnityJam.Interaction;  // 親クラス
using UnityJam.UI;
using UnityJam.Effects;      // BloomBurst注入用

namespace UnityJam.Gimmicks
{
    public class TreasureChestController : InteractableBase, IBloomBurstReceiver
    {
        [Header("--- ドロップアイテム ---")]
        [Tooltip("デフォルトのドロップテーブル（階層指定がない場合に使用）")]
        [SerializeField] private TreasureDropTable dropTable;

        [Tooltip("階層ごとのドロップテーブル（Index 0 = 1階, Index 1 = 2階...）\n要素数が階層数より少ない場合は、リストの最後が使われます。")]
        [SerializeField] private List<TreasureDropTable> floorDropTables;

        [Header("--- 演出 ---")]
        [Tooltip("宝箱のAnimator")]
        [SerializeField] private Animator chestAnimator;

        [Header("--- エフェクト設定 ---")]
        [Tooltip("宝箱の中から吹き出す光。宝箱の子オブジェクトにして配置想定")]
        [SerializeField] private ParticleSystem innerGlowParticles;

        [Tooltip("レアリティに関係なく必ず再生されるベースエフェクト")]
        [SerializeField] private GameObject baseVfxPrefab;

        [Tooltip("レアリティごとの開閉エフェクト (Element 0 がレア度1, Element 1 がレア度2...)")]
        [SerializeField] private GameObject[] rarityVfxPrefabs;

        [Header("--- 演出（サウンド） ---")]
        [Tooltip("開けた時の効果音")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioSource audioSource;

        [Header("--- 演出（カメラ振動） ---")]
        [Tooltip("開けた瞬間にカメラを揺らす強さ（0なら揺らさない）")]
        [SerializeField] private float cameraShakeStrength = 0.2f;
        [SerializeField] private float cameraShakeDuration = 0.3f;

        [Tooltip("アイテム取得時の飛び出すアイコン (Prefab)")]
        [SerializeField] private GameObject popupEffectPrefab;

        [Tooltip("宝箱が開いてからアイコンが出るまでの待ち時間（秒）")]
        [SerializeField] private float iconPopupDelay = 0.5f;

        [Header("--- ポストエフェクト ---")]
        [Tooltip("開封時にBloomを一瞬だけ強める（Spawnerから自動注入される想定）")]
        [SerializeField] private BloomBurstController bloomBurst;

        [Header("--- Light Burst (Optional) ---")]
        [Tooltip("宝箱開封時に一瞬だけ出す Point Light のPrefab（PFx_TreasureBurstLight など）")]
        [SerializeField] private GameObject burstLightPrefab;

        [SerializeField, Min(0.01f)]
        private float burstLightLifeTime = 0.5f;

        [SerializeField]
        private Vector3 burstLightOffset = new Vector3(0f, 1.0f, 0f);

        private bool isOpen = false;

        // Startでマネージャーに登録
        private void Start()
        {
            if (TreasureManager.Instance != null)
            {
                TreasureManager.Instance.RegisterChest(this.gameObject);
            }

            // AudioSourceがアタッチされていなくて、音設定がある場合は自動追加
            if (audioSource == null && openSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.volume = 0.5f; // 音量調整
                audioSource.spatialBlend = 1.0f; // 1.0にすると3Dサウンド
            }
        }

        /// <summary>
        /// TreasureSpawner 等から BloomBurstController を注入する。
        /// </summary>
        public void SetBloomBurst(BloomBurstController bloomBurstController)
        {
            bloomBurst = bloomBurstController;
        }

        // 親クラスにある abstract void OnInteractCompleted
        protected override void OnInteractCompleted()
        {
            OpenChest();
        }

        // 宝箱を開ける処理
        private void OpenChest()
        {
            if (isOpen) return;

            // ドロップテーブルの選定
            TreasureDropTable selectedTable = dropTable;

            if (floorDropTables != null && floorDropTables.Count > 0)
            {
                int floor = 1;
                if (GameSessionManager.Instance != null)
                {
                    floor = GameSessionManager.Instance.CurrentFloor;
                }

                // floorは1始まり、リストは0始まり
                // 階層がリスト数より多い場合は最後の要素を使う
                int index = Mathf.Clamp(floor - 1, 0, floorDropTables.Count - 1);
                selectedTable = floorDropTables[index];

                // もし万が一nullならデフォルトに戻す
                if (selectedTable == null) selectedTable = dropTable;
            }

            // ドロップテーブルが設定されているかチェック
            if (selectedTable == null)
            {
                Debug.LogWarning("宝箱にドロップテーブルが設定されていません！");
                return;
            }

            // A. ドロップテーブルを使って抽選
            ItemMaster item = selectedTable.PickOneItem();

            // 抽選結果が空なら（設定ミスなど）何もしない
            if (item == null)
            {
                Debug.LogWarning("ドロップ抽選に失敗しました（有効なアイテムがありません）");
                return;
            }

            // バッテリー減少処理
            if (selectedTable.batteryPenaltyPercent > 0)
            {
                // バッテリー管理スクリプトを探す
                // ※ "PlayerBattery" の部分は実際のクラス名に合わせてください。


                var playerBattery = FindObjectOfType<PlayerLight>();

                if (playerBattery != null)
                {
                    playerBattery.ReduceBatteryByPercent(dropTable.batteryPenaltyPercent);
                }
            }

            // B. インベントリに追加
            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddItem(item);
            }

            // C. 見た目の変更
            isOpen = true;

            if (chestAnimator != null)
            {
                chestAnimator.SetTrigger("Open");
            }

            // 内部からの光
            if (innerGlowParticles != null)
            {
                innerGlowParticles.gameObject.SetActive(true);
                innerGlowParticles.Play();
            }

            // サウンド再生
            if (audioSource != null && openSound != null)
            {
                audioSource.PlayOneShot(openSound);
            }

            // カメラシェイク
            if (cameraShakeStrength > 0)
            {
                StartCoroutine(ShakeCamera(cameraShakeDuration, cameraShakeStrength));
            }

            if (TreasureManager.Instance != null)
            {
                TreasureManager.Instance.UnregisterChest(this.gameObject);
            }

            // ベースエフェクト再生
            if (baseVfxPrefab != null)
            {
                // Parent to this chest to clean up when stage is destroyed
                Instantiate(baseVfxPrefab, transform.position, Quaternion.identity, transform);
            }

            // D. レアリティに応じたVFXを再生
            PlayRarityVfx(item.rarity);

            // Light Burst
            if (burstLightPrefab != null)
            {
                // Parent to this chest
                GameObject lightObj = Instantiate(burstLightPrefab, transform.position + burstLightOffset, Quaternion.identity, transform);
                StartCoroutine(FadeOutLight(lightObj, burstLightLifeTime));
            }

            // Bloom
            if (bloomBurst != null) bloomBurst.PlayBurst();

            // E. アイコン演出を遅延実行する
            StartCoroutine(ShowIconDelayed(item));

            Debug.Log($"宝箱を開けた！ {item.itemName} (Rarity:{item.rarity}) を獲得！");
        }

        // カメラ用シェイク
        private IEnumerator ShakeCamera(float duration, float magnitude)
        {
            if (Camera.main == null) yield break;

            Transform camTransform = Camera.main.transform;
            Vector3 originalPos = camTransform.localPosition;
            float elapsed = 0.0f;

            // シェイク処理（元の位置に戻る補正付き）
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                camTransform.localPosition = originalPos + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            camTransform.localPosition = originalPos;
        }

        // ライトのフェードアウト
        private IEnumerator FadeOutLight(GameObject lightObj, float duration)
        {
            // ライトコンポーネントを取得
            Light l = lightObj.GetComponent<Light>();

            // ライトがついていないPrefabだった場合の安全策
            if (l == null)
            {
                Destroy(lightObj, duration);
                yield break;
            }

            float startIntensity = l.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                // 時間経過
                elapsed += Time.deltaTime;

                // 割合を計算 (0.0 -> 1.0)
                float t = elapsed / duration;

                // 強さを初期値から0へ徐々に減らす
                l.intensity = Mathf.Lerp(startIntensity, 0f, t);

                // 1フレーム待つ
                yield return null;
            }

            // 完全に0にしてから削除
            l.intensity = 0f;
            Destroy(lightObj);
        }

        // 遅延実行用
        private IEnumerator ShowIconDelayed(ItemMaster item)
        {
            // 設定した秒数だけ待つ
            yield return new WaitForSeconds(iconPopupDelay);

            if (popupEffectPrefab != null)
            {
                // 少し上に出現させる
                // Parent to this chest
                GameObject effectObj = Instantiate(popupEffectPrefab, transform.position + Vector3.up, Quaternion.identity, transform);
                ItemPopupEffect popupScript = effectObj.GetComponent<ItemPopupEffect>();
                if (popupScript != null) popupScript.Initialize(item.icon);
            }
        }

        // レアリティVFX再生用メソッド
        private void PlayRarityVfx(int rarity)
        {
            // 設定配列が空なら何もしない
            if (rarityVfxPrefabs == null || rarityVfxPrefabs.Length == 0) return;

            // レアリティは1から始まるが、配列は0から始まるので -1 する
            int index = rarity - 1;

            // 配列の範囲内かチェック（レア度4以上のアイテムが来たり、設定が足りない場合の対策）
            if (index >= 0 && index < rarityVfxPrefabs.Length)
            {
                GameObject vfxToPlay = rarityVfxPrefabs[index];
                if (vfxToPlay != null)
                {
                    Instantiate(vfxToPlay, transform.position, Quaternion.identity, transform);
                }
            }
            else
            {
                // 該当するレアリティのエフェクトがない場合、とりあえず一番下のレアリティを再生しておく（保険）
                if (rarityVfxPrefabs[0] != null)
                    Instantiate(rarityVfxPrefabs[0], transform.position, Quaternion.identity, transform);
            }
        }
    }
}
