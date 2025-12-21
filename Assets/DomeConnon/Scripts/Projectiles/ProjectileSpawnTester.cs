/*********************************************************************/
/**
 * @file   ProjectileSpawnTester.cs
 * @brief  Step4用：弾の生成・飛行・衝突を確認する簡易スポーナー
 *
 * Responsibility:
 * - 一定間隔で弾を発射して挙動を確認する
 *
 * Notes:
 * - 本番Spawner（難易度/予告UI）は次ステップ以降で実装する
 */
/*********************************************************************/
using UnityEngine;

namespace DomeCannon
{
    /// <summary>
    /// Step4用：弾の生成・飛行・衝突を確認する簡易スポーナー
    /// </summary>
    public sealed class ProjectileSpawnTester : MonoBehaviour
    {
        [SerializeField]
        private ProjectilePool pool;

        [SerializeField]
        private SpawnPointGroup spawnPointGroup;

        [SerializeField]
        private float intervalSec = 1.0f;

        [SerializeField]
        private float speed = 12.0f;

        private float timer;

        private void Update()
        {

            if (pool == null || spawnPointGroup == null)
            {
                return;
            }

            if (spawnPointGroup.Points.Count == 0)
            {
                return;
            }

            timer -= Time.deltaTime;
            if (timer > 0.0f)
            {
                return;
            }

            // Step 1: 次回までの待ち
            timer = Mathf.Max(0.01f, intervalSec);

            // Step 2: SpawnPointを順番に使う（テスト簡易）
            int index = Time.frameCount % spawnPointGroup.Points.Count;
            Transform sp = spawnPointGroup.Points[index];
            if (sp == null)
            {
                return;
            }

            // Step 3: 発射
            Projectile p = pool.Get();
            if (p == null)
            {
                return;
            }

            Vector3 dir = sp.forward;
            float lifetime = p.DefaultLifetimeSec;

            p.Activate(pool, sp.position, dir, speed, lifetime);
        }
    }
}
