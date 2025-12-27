// Assets/Scripts/Cameras/DeathCameraFocus.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityJam.Cameras
{
    /// <summary>
    /// 死亡演出用のカメラ注視・ズーム制御。
    /// 追従系（CameraRigController 等）を一時停止し、指定ターゲットを注視してズームする。
    /// </summary>
    public sealed class DeathCameraFocus : MonoBehaviour
    {
        public static DeathCameraFocus Instance { get; private set; }

        [Header("--- Camera ---")]
        [Tooltip("制御対象カメラ。Prefab配下の子Cameraを直接差すのが安全。未設定なら子から自動取得。")]
        [SerializeField] private UnityEngine.Camera cameraToControl;

        [Header("--- Disable While Focus ---")]
        [Tooltip("注視中に無効化するコンポーネント（CameraRigController 等）。※Camera(描画本体)は入れないこと。")]
        [SerializeField] private List<Behaviour> componentsToDisableWhileFocus = new List<Behaviour>();

        [Header("--- Focus Setup ---")]
        [Tooltip("カメラ位置 = target.position + focusOffset（ワールド）")]
        [SerializeField] private Vector3 focusOffset = new Vector3(0f, 1.6f, -3.0f);

        [Tooltip("注視点 = target.position + lookAtOffset（ワールド）")]
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1.2f, 0f);

        [Tooltip("ズーム時のFOV（小さいほどズームが強い）")]
        [SerializeField, Range(10f, 90f)] private float zoomFov = 35f;

        [Tooltip("寄る/回す補間時間[sec]")]
        [SerializeField, Min(0.01f)] private float blendTime = 0.20f;

        [Header("--- Optional ---")]
        [Tooltip("注視中にTimeScaleを落とす（0で無効）")]
        [SerializeField, Range(0f, 1f)] private float focusTimeScale = 0f;

        [Tooltip("演出終了後に無効化したコンポーネントを復帰する（GameOver遷移ならOFFのままでもOK）")]
        [SerializeField] private bool restoreDisabledComponentsAfterFocus = false;

        private Coroutine focusRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            // Prefab運用：Camera.main は参照ズレ事故が多いので、
            // まず自分のPrefab配下（子）から Camera を拾う。
            if (cameraToControl == null)
            {
                cameraToControl = GetComponentInChildren<UnityEngine.Camera>(true);
            }

            // それでも無ければ最後の手段
            if (cameraToControl == null)
            {
                cameraToControl = UnityEngine.Camera.main;
            }

            // Inspectorに何も入れていない場合は、最低限止めたいものを自動で拾う（任意）
            AutoCollectDisableTargetsIfNeeded();
        }

        /// <summary>
        /// 注視＆ズームを再生する（holdTimeSec後に onFinished を呼ぶ）。
        /// </summary>
        public void PlayFocus(Transform target, float holdTimeSec, Action onFinished)
        {
            if (cameraToControl == null)
            {
                onFinished?.Invoke();
                return;
            }

            if (focusRoutine != null)
            {
                StopCoroutine(focusRoutine);
            }

            focusRoutine = StartCoroutine(FocusRoutine(target, holdTimeSec, onFinished));
        }

        private void AutoCollectDisableTargetsIfNeeded()
        {
            if (cameraToControl == null) return;

            if (componentsToDisableWhileFocus == null)
            {
                componentsToDisableWhileFocus = new List<Behaviour>();
            }

            // 既に設定済みなら尊重
            if (componentsToDisableWhileFocus.Count > 0) return;

            // カメラ追従（Rig）を止めるのが最優先
            var rig = cameraToControl.GetComponentInParent<UnityJam.CameraRigController>();
            if (rig != null) componentsToDisableWhileFocus.Add(rig);

        }

        private IEnumerator FocusRoutine(Transform target, float holdTimeSec, Action onFinished)
        {
            // --- 1) 通常制御を止める（位置/回転など） ---
            // ※Camera（描画本体）を無効化すると画面が真っ黒になるので絶対に止めない
            Dictionary<Behaviour, bool> prevEnabled = null;

            if (componentsToDisableWhileFocus != null && componentsToDisableWhileFocus.Count > 0)
            {
                if (restoreDisabledComponentsAfterFocus)
                {
                    prevEnabled = new Dictionary<Behaviour, bool>(componentsToDisableWhileFocus.Count);
                }

                for (int i = 0; i < componentsToDisableWhileFocus.Count; i++)
                {
                    Behaviour b = componentsToDisableWhileFocus[i];
                    if (b == null) continue;

                    // 事故防止：Cameraを入れてしまっても無効化しない
                    if (b is UnityEngine.Camera) continue;

                    if (prevEnabled != null) prevEnabled[b] = b.enabled;
                    b.enabled = false;
                }
            }

            // --- 2) FOV上書きを止める（重要） ---
            // CameraController が FixedUpdate で FOV を戻してくる事故を防ぐ
            UnityJam.CameraController camCtrl = null;
            if (cameraToControl != null)
            {
                camCtrl = cameraToControl.GetComponent<UnityJam.CameraController>();
                if (camCtrl != null)
                {
                    camCtrl.SetFovLock(true);
                }
            }

            // --- 3) TimeScale（任意） ---
            float originalTimeScale = Time.timeScale;
            if (focusTimeScale > 0f)
            {
                Time.timeScale = focusTimeScale;
            }

            // --- 4) 補間で寄る・向ける・ズームする ---
            Transform camTf = cameraToControl.transform;

            Vector3 startPos = camTf.position;
            Quaternion startRot = camTf.rotation;
            float startFov = cameraToControl.fieldOfView;

            Vector3 endPos;
            Vector3 lookAt;

            if (target != null)
            {
                endPos = target.position + focusOffset;
                lookAt = target.position + lookAtOffset;
            }
            else
            {
                endPos = startPos;
                lookAt = startPos + camTf.forward;
            }

            Vector3 lookDir = (lookAt - endPos);
            Quaternion endRot = lookDir.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(lookDir.normalized, Vector3.up)
                : startRot;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / blendTime;
                float s = Mathf.Clamp01(t);

                camTf.position = Vector3.Lerp(startPos, endPos, s);
                camTf.rotation = Quaternion.Slerp(startRot, endRot, s);
                cameraToControl.fieldOfView = Mathf.Lerp(startFov, zoomFov, s);

                yield return null;
            }

            // --- 5) 見せる（停止時間） ---
            if (holdTimeSec > 0f)
            {
                float elapsed = 0f;
                while (elapsed < holdTimeSec)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            // --- 6) 後処理 ---
            if (focusTimeScale > 0f)
            {
                Time.timeScale = originalTimeScale;
            }

            // FOVロック解除（GameOverへ遷移するなら不要だが、復帰する運用なら解除）
            //if (camCtrl != null)
            //{
            //    camCtrl.SetFovLock(false);
            //}

            // 無効化したコンポーネントを復帰（必要な場合のみ）
            if (restoreDisabledComponentsAfterFocus && prevEnabled != null)
            {
                foreach (var kv in prevEnabled)
                {
                    if (kv.Key != null)
                    {
                        kv.Key.enabled = kv.Value;
                    }
                }
            }

            onFinished?.Invoke();
        }
    }
}
