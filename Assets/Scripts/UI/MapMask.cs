using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


namespace UnityJam.UI
{
    public enum MapType
    {
        Map,
        Minimap
    }
    public static class MapUIEvents
    {
        public static event Action<MapMask> OnUIRequested;

        public static event Action<MapType> OnSetMapType;

        public static void Request(MapMask ui)
        {
            OnUIRequested?.Invoke(ui);
        }

        public static void UseMap()
        {
            OnSetMapType?.Invoke(MapType.Map);
        }

        public static void UseMiniMap()
        {
            OnSetMapType?.Invoke(MapType.Minimap);
        }
    }


    public class MapMask : MonoBehaviour
    {
        public Camera mapCamera { get; set; }
        public MapCamera targetCameraComponents {  get; set; }


        public void CameraSetting(MapCamera camera)
        {
            mapCamera = camera.GetComponent<Camera>();
            targetCameraComponents = camera; 
            
        }


        [SerializeField]
        GameObject IconPrefab;
        [SerializeField]
        GameObject map;
        [SerializeField]
        GameObject minimap;

        [SerializeField]
        int mapSize = 256;
        [SerializeField]
        RectTransform mapTextureRectTransform;

        [SerializeField]
        RectTransform minimapTextureRectTransform;
        [SerializeField]
        RectTransform minimapControllerRect;

        [SerializeField]
        RectTransform minimapMaskRectTransform;

        [SerializeField]
        RawImage mapMaskImage;
        [SerializeField]
        RawImage minimapMaskImage;

        MapType useMapType;

        Texture2D fogTexture;
        float timer;

        private readonly List<IconData> datas = new List<IconData>();
        private readonly List<GameObject> Icons = new List<GameObject>();


        float mapMaxX;
        float mapMinX;
        float mapMaxZ;
        float mapMinZ;
        // Start is called before the first frame update
        void Start()
        {
            timer = 0.0f;

            useMapType = MapType.Map;

            float halfHeight = mapCamera.orthographicSize;
            float halfWidth = mapCamera.orthographicSize * mapCamera.aspect;

            Vector3 camPos = mapCamera.transform.position;

            mapMinX = camPos.x - halfWidth;
            mapMaxX = camPos.x + halfWidth;

            mapMinZ = camPos.z - halfHeight;
            mapMaxZ = camPos.z + halfHeight;

            // Textureを共有するため一旦コメントアウト
            fogTexture = new Texture2D(mapSize, mapSize, TextureFormat.RGBA32, false);
            fogTexture.filterMode = FilterMode.Bilinear;

            // 全部黒（未探索）
            Color32[] pixels = new Color32[mapSize * mapSize];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(1, 1, 1, 1);

            fogTexture.SetPixels32(pixels);
            fogTexture.Apply();

            mapMaskImage.texture = fogTexture;
            minimapMaskImage.texture = fogTexture;


        }

        // Update is called once per frame
        void Update()
        {
            timer += Time.deltaTime;
            if (timer > 0.1f)
            {
                Reveal(targetCameraComponents.targetTransform.position);
                timer = 0.0f;

            }

            switch (useMapType)
            {
                case MapType.Map:
                    minimap.SetActive(false);
                    map.SetActive(true);

                    SetMapIcon();
                    break;

                case MapType.Minimap:
                    map.SetActive(false);
                    minimap.SetActive(true);
                    MinimapMove(targetCameraComponents.targetTransform.position);
                    SetMiniMapIcon(targetCameraComponents.targetTransform.position);
                    break;
            }

            //SetMapIcon();
            //MinimapMove(targetTransform.position);
            //SetMiniMapIcon(targetTransform.position);
        }

        Vector2 WorldToMap(Vector3 worldPos)
        {
            float u = (worldPos.x - mapMinX) / (mapMaxX - mapMinX);
            float v = (worldPos.z - mapMinZ) / (mapMaxZ - mapMinZ);
            return new Vector2(u, v);
        }

        void Reveal(Vector3 playerPos)
        {
            Vector2 uv = WorldToMap(playerPos);

            int x = (int)(uv.x * mapSize);
            int y = (int)(uv.y * mapSize);

            int radius = 15;

            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int px = x + dx;
                    int py = y + dy;

                    if (px < 0 || px >= mapSize || py < 0 || py >= mapSize)
                        continue;

                    fogTexture.SetPixel(px, py, new Color(0, 0, 0, 0));


                }

            fogTexture.Apply();

        }

        // アイコン処理

        void SetMapIcon()
        {
            int index = 0;
            foreach (GameObject icon in Icons)
            {
                if (!icon.activeSelf) continue;
                if (index >= datas.Count) break;

                Image iconImage = icon.GetComponent<Image>();
                IconData data = datas[index];

                if (IsExplored(data.uiTransform.position))
                {

                    Vector2 mapSize = mapTextureRectTransform.rect.size;

                    float normalizeIconPosX =
                        ((data.uiTransform.position.x - mapMinX) / (mapMaxX - mapMinX)) - 0.5f;

                    float normalizeIconPosY =
                        ((data.uiTransform.position.z - mapMinZ) / (mapMaxZ - mapMinZ)) - 0.5f;

                    float iconPosX = normalizeIconPosX * mapSize.x;
                    float iconPosY = normalizeIconPosY * mapSize.y;

                    Vector3 parentScale = mapTextureRectTransform.parent.localScale;

                    Vector2 iconPos = new Vector2(
                        iconPosX * parentScale.x,
                        iconPosY * parentScale.y);


                    RectTransform rect = icon.GetComponent<RectTransform>();
                    rect.localPosition = iconPos;

                    iconImage.color = Color.white;
                    iconImage.sprite = data.texture;
                }
                else
                {
                    // 処理をせず透明化
                    iconImage.color = new Color(1, 1, 1, 0);
                }

                index++;

            }
        }

        void MinimapMove(Vector3 playerPos)
        {

            Vector2 mapSize = minimapTextureRectTransform.rect.size;

            float nx = (playerPos.x - mapMinX) / (mapMaxX - mapMinX) - 0.5f;
            float ny = (playerPos.z - mapMinZ) / (mapMaxZ - mapMinZ) - 0.5f;

            Vector3 controllerScale = minimapControllerRect.localScale;

            Vector2 offset = new Vector2(
                -nx * mapSize.x * controllerScale.x,
                -ny * mapSize.y * controllerScale.y
            );

            minimapControllerRect.anchoredPosition = offset;

        }

        void SetMiniMapIcon(Vector3 playerPos)
        {
            int index = 0;

            Vector2 mapSize = minimapTextureRectTransform.rect.size;
            Vector3 parentScale = minimapControllerRect.localScale;

            float pnx =
                (playerPos.x - mapMinX) / (mapMaxX - mapMinX) - 0.5f;
            float pny =
                (playerPos.z - mapMinZ) / (mapMaxZ - mapMinZ) - 0.5f;

            foreach (GameObject icon in Icons)
            {
                if (!icon.activeSelf) continue;
                if (index >= datas.Count) break;

                IconData data = datas[index];

                Image iconImage = icon.GetComponent<Image>();
                bool isExplored = IsExplored(data.uiTransform.position);


                if (isExplored)
                {
                    float nx =
                        (data.uiTransform.position.x - mapMinX) / (mapMaxX - mapMinX) - 0.5f;
                    float ny =
                        (data.uiTransform.position.z - mapMinZ) / (mapMaxZ - mapMinZ) - 0.5f;
                    Vector3 minimapOffset = minimap.GetComponent<RectTransform>().anchoredPosition;
                    Vector3 minimapScale = minimap.GetComponent<RectTransform>().localScale;

                    Vector2 iconPos = new Vector2(
                        (nx - pnx) * mapSize.x * parentScale.x * minimapScale.x + minimapOffset.x,
                        (ny - pny) * mapSize.y * parentScale.y * minimapScale.y + minimapOffset.y
                    );

                    RectTransform rect = icon.GetComponent<RectTransform>();
                    rect.anchoredPosition = iconPos;

                    if(IsInsideMinimap(iconPos))
                    {
                        iconImage.color = Color.white;
                        iconImage.sprite = data.texture;
                    }
                    else
                    {
                        iconImage.color = new Color(1, 1, 1, 0);
                    }

                }
                else
                {
                    // 処理をせず透明化
                    iconImage.color = new Color(1, 1, 1, 0);
                }


                index++;
            }


        }
        void OnEnable()
        {
            MapUIEvents.Request(this);

            MiniMapEvents.OnRegister += Register;
            MiniMapEvents.OnUnregister += Unregister;
            MapUIEvents.OnSetMapType += SetMapType;

        }

        void OnDisable()
        {
            MiniMapEvents.OnRegister -= Register;
            MiniMapEvents.OnUnregister -= Unregister;
        }

        void SetMapType(MapType type)
        {
            useMapType = type;
        }

        void Register(IconData data)
        {
            if (!datas.Contains(data))
                datas.Add(data);

            // アクティブなIcon数よりdataのIcon数の方が大きいか確認
            if (activIconCount() < datas.Count)
            {
                // 非アクティブ含めたIcon数よりdataのIcon数の方が大きいか確認
                if (Icons.Count < datas.Count)
                {
                    GameObject addIcon = Instantiate(IconPrefab, transform, false);
                    addIcon.SetActive(true);
                    Icons.Add(addIcon);

                }
                else // Icon数自体はData数より多いのでアクティブなアイコンを増やす
                {
                    // 非アクティブなIconをアクティブにする
                    for (int i = Icons.Count - 1; i >= 0; i--)
                    {
                        if (!Icons[i].activeSelf)
                        {
                            Icons[i].SetActive(true);
                            break;
                        }
                    }
                }

            }


        }

        void Unregister(IconData data)
        {
            datas.Remove(data);
            // 念のため確認
            if (activIconCount() > datas.Count)
            {
                // dataが減ったのでアイコンを一つActivにする
                for (int i = Icons.Count - 1; i >= 0; i--)
                {
                    if (Icons[i].activeSelf)
                    {
                        Icons[i].SetActive(false);
                        break;
                    }
                }
            }
        }

        int activIconCount()
        {
            int ret = 0;
            foreach (GameObject icon in Icons)
            {
                if (icon.activeSelf)
                {
                    ret++;
                }
            }

            return ret;

        }


        bool IsExplored(Vector3 worldPos)
        {
            float nx = (worldPos.x - mapMinX) / (mapMaxX - mapMinX);
            float ny = (worldPos.z - mapMinZ) / (mapMaxZ - mapMinZ);

            if (nx < 0f || nx > 1f || ny < 0f || ny > 1f)
                return false;

            int px = Mathf.FloorToInt(nx * fogTexture.width);
            int py = Mathf.FloorToInt(ny * fogTexture.height);

            Color32 c = fogTexture.GetPixel(px, py);

            return c.a == 0;
        }
        bool IsInsideMinimap(Vector2 iconPos)
        {
            RectTransform maskRT = minimapMaskRectTransform;
            RectTransform minimapRT = minimap.GetComponent<RectTransform>();
            Vector2 halfSize = maskRT.rect.size * 0.5f * minimapRT.localScale;
            Vector2 center = maskRT.anchoredPosition + minimapRT.anchoredPosition;

            Vector2 local = iconPos - center;

            return
                local.x >= -halfSize.x &&
                local.x <= halfSize.x &&
                local.y >= -halfSize.y &&
                local.y <= halfSize.y;
        }

    }

}
