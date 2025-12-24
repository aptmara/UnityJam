using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Interaction;

namespace UnityJam.Player
{

    public class PlayerInteractor : MonoBehaviour
    {
        [Header("--- インタラクト ---")]
        [Tooltip("インタラクトするキー")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Tooltip("インタラクト可能な距離")]
        [SerializeField] private float interactRange = 2.0f;

        [Tooltip("レイを飛ばす起点（カメラなど。空なら自分の位置）")]
        [SerializeField] private Transform rayOrigin;

        // 後付けでAnimatorを直接いじってるけど後で全部Playerが結果受け取って動かす by越智
        [Tooltip("俺が勝手にAnimatorをいじるために受け取ってるけど後でPlayer側にやらせたいby越智")]
        [SerializeField] private Animator playerAnimator;

        [Header("--- デバッグ ---")]
        [SerializeField] private InteractableBase currentTarget; // 今見ている対象

        void Update()
        {
            // 1. 目の前のインタラクト可能な物を探す
            FindTarget();

            // 2. キーが押されている間、対象の時間を進める
            if (currentTarget != null)
            {
                if (Input.GetKey(interactKey))
                {

                    // ここで時間を進める関数を呼ぶ！
                    bool isInteract = currentTarget.AddInteractTime();

                    if(isInteract)
                    {
                        playerAnimator.SetTrigger("Interact");
                    }

                    // ログで進行度確認（完成したら消してOK）
                    // Debug.Log($"Opening... {currentTarget.Progress:P0}");
                }
            }
        }

        void FindTarget()
        {
            // 起点が設定されてなければ自分の位置+少し上
            Vector3 origin = rayOrigin != null ? rayOrigin.position : transform.position + Vector3.up;
            Vector3 direction = rayOrigin != null ? rayOrigin.forward : transform.forward;

            Ray ray = new Ray(origin, direction);

            // レイキャストで探す (LayerMaskを設定するとより良いですが、一旦すべて)
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                // 当たった相手が InteractableBase を持っているか？
                InteractableBase interactable = hit.collider.GetComponent<InteractableBase>();

                // 見つかったらターゲットにする
                if (interactable != null)
                {
                    currentTarget = interactable;
                    Debug.Log("Found interactable target: " + currentTarget.name);
                    return;
                }
                else
                {
                    Debug.Log("Hit something, but it's not interactable.");
                }
            }

            // 何もなければターゲット解除
            currentTarget = null;
        }

        // 起点取得用のヘルパー関数
        Vector3 GetOrigin() => rayOrigin != null ? rayOrigin.position : transform.position + Vector3.up;
        Vector3 GetDirection() => rayOrigin != null ? rayOrigin.forward : transform.forward;

        // --- デバッグ描画の強化 ---
        private void OnDrawGizmos()
        {
            Vector3 origin = GetOrigin();
            Vector3 direction = GetDirection();

            // 1. レイの直線を引く
            // ヒットしているかチェック
            bool isHit = Physics.Raycast(origin, direction, out RaycastHit hit, interactRange);

            // 色の決定：ターゲットを捉えていれば赤、そうでなければ緑
            if (isHit && hit.collider.GetComponent<InteractableBase>() != null)
            {
                Gizmos.color = Color.red; // ヒット！
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.2f); // 当たった場所に球を表示
            }
            else
            {
                Gizmos.color = Color.green; // 探索中
                Gizmos.DrawLine(origin, origin + direction * interactRange);
            }
        }
    }
}
