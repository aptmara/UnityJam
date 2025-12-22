/*********************************************************************/
/**
 * @file   ProjectilePool.cs
 * @brief  Projectile のオブジェクトプール
 *
 * Responsibility:
 * - 事前生成した Projectile を再利用する
 * - Get/Return のみを提供し、弾ロジックはProjectile側に任せる
 *
 * Notes:
 * - 実行中に増やしたい場合は拡張可能（現段階は固定サイズ）
 */
/*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace DomeCannon
{
    /// <summary>
    /// Projectile のオブジェクトプール
    /// </summary>
    public sealed class ProjectilePool : MonoBehaviour
    {
        [SerializeField]
        private Projectile projectilePrefab;

        [SerializeField]
        private int prewarmCount = 32;

        [SerializeField]
        private Transform poolRoot;

        private readonly Queue<Projectile> pool = new Queue<Projectile>(64);

        private void Awake()
        {
            if (poolRoot == null)
            {
                poolRoot = transform;
            }

            Prewarm();
        }

        /// <summary>
        /// プールから弾を取得する（枯渇時は生成）
        /// </summary>
        public Projectile Get()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            return CreateOne();
        }

        /// <summary>
        /// 弾をプールへ返却する
        /// </summary>
        /// <param name="projectile">返却対象</param>
        public void Return(Projectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            projectile.transform.SetParent(poolRoot, false);
            pool.Enqueue(projectile);
        }

        private void Prewarm()
        {
            // Step 1: 事前生成
            int count = Mathf.Max(0, prewarmCount);
            for (int i = 0; i < count; i++)
            {
                Projectile p = CreateOne();
                p.gameObject.SetActive(false);
                Return(p);
            }
        }

        private Projectile CreateOne()
        {
            if (projectilePrefab == null)
            {
                return null;
            }

            Projectile p = Instantiate(projectilePrefab, poolRoot);
            p.gameObject.SetActive(false);
            return p;
        }
    }
}
