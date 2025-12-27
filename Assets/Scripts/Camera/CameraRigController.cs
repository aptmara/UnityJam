using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityJam
{
    public class CameraRigController : MonoBehaviour
    {
        // Headerを使うことでUnityで数値を入れるときの視認性が良くなる
        [Header("Player")]
        [SerializeField] Transform player;
        // ついぞCameraRigがプレイヤー以外につくことはなかったので時間もないし一旦Playerへがっつり依存する
        [SerializeField] PlayerMapChange mapChange;

        [Header("Camera")]
        [SerializeField] Transform useCamera;

        [Header("Rotation")]
        [SerializeField] float sensitivity = 180f;
        [SerializeField] float minPitch = -30f;
        [SerializeField] float maxPitch = 60f;

        [Header("Distance")]
        [SerializeField] float distance = 3.0f;
        [SerializeField] float smoothnees = 0.3f;
        [SerializeField] Vector3 offset = new Vector3(0f, 1.5f, 0f);

        [Header("Wall Check")]
        [SerializeField] LayerMask wallLayers;
        [SerializeField] float wallOffset = 0.1f; // 壁から少し浮かす

        public float yaw { get; private set; }
        public float pitch { get; private set; }

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            Vector3 euler = useCamera.rotation.eulerAngles;
            yaw = euler.y;
            pitch = euler.x;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            
            // カーソルロック時のみカメラ操作可能
            if (Cursor.lockState != CursorLockMode.Locked) return;

            if (mapChange.playerMapState == PlayerMapChange.PlayerMapState.UseMap) return;

            // マウス移動を取ってカメラを移動
            float mouseX = Input.GetAxisRaw("Mouse X");
            float mouseY = Input.GetAxisRaw("Mouse Y");

            yaw += mouseX * sensitivity * dt;
            pitch -= mouseY * sensitivity * dt;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            useCamera.rotation = rotation;

            Vector3 worldOffset = rotation * offset;

            Vector3 targetPos = player.position + worldOffset;

            Vector3 desiredCameraPos = targetPos - rotation * Vector3.forward * distance;

            // 自身の位置からの距離を使ってRayの向きを作る
            Vector3 rayDir = desiredCameraPos - targetPos;
            float rayLength = rayDir.magnitude;
            rayDir.Normalize();

            // Rayを作成、Camera座標は一度当たらなかった場合の位置を入れる
            RaycastHit hit;
            Vector3 finalCameraPos = desiredCameraPos;

            // Sphere型のRawが当たったかどうか確認する
            if (Physics.SphereCast(
                    targetPos,
                    smoothnees,
                    rayDir,
                    out hit,
                    rayLength,
                    wallLayers,
                    QueryTriggerInteraction.Ignore))
            {
                // 当たった場合当たった位置をカメラ座標に設定
                finalCameraPos = hit.point + hit.normal * wallOffset;
            }

            useCamera.position = finalCameraPos;


        }
    }


}
