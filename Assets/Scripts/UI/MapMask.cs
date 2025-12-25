using System;
using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MapMask : MonoBehaviour
{
    [SerializeField]
    Camera mapCamera;

    [SerializeField]
    RawImage rawImage;
    [SerializeField]
    Transform targetTransform;

    [SerializeField]
    GameObject IconPrefab;

    [SerializeField]
    int mapSize = 256;

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

        rawImage.color = Color.white;

        float halfHeight = mapCamera.orthographicSize;
        float halfWidth = mapCamera.orthographicSize * mapCamera.aspect;

        Vector3 camPos = mapCamera.transform.position;

        mapMinX = camPos.x - halfWidth;
        mapMaxX = camPos.x + halfWidth;

        mapMinZ = camPos.z - halfHeight;
        mapMaxZ = camPos.z + halfHeight;

        fogTexture = new Texture2D(mapSize, mapSize, TextureFormat.RGBA32, false);
        fogTexture.filterMode = FilterMode.Bilinear;

        // 全部黒（未探索）
        Color32[] pixels = new Color32[mapSize * mapSize];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(1, 1, 1, 1);

        fogTexture.SetPixels32(pixels);
        fogTexture.Apply();

        rawImage.texture = fogTexture;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.1f)
        {
            Reveal(targetTransform.position);
            timer = 0.0f;

            int index = 0;
            foreach(GameObject icon in Icons)
            {
                if (!icon.activeSelf) continue;
                if (index >= datas.Count) break;

                IconData data = datas[index];
                // TODO:後でMapのScaleをかける
                //float normalizeIconPosX = (((data.uiTransform.position.x - mapMinX) / (mapMaxX - mapMinX)) - 0.5f);
                //float normalizeIconPosY = (((data.uiTransform.position.z - mapMinZ) / (mapMaxZ - mapMinZ)) - 0.5f);

                //float iconPosX = normalizeIconPosX * rawImage.texture.width;
                //float iconPosY = normalizeIconPosY * rawImage.texture.height;

                RectTransform mapRT = rawImage.rectTransform;
                Vector2 mapSize = mapRT.rect.size;

                float normalizeIconPosX =
                    ((data.uiTransform.position.x - mapMinX) / (mapMaxX - mapMinX)) - 0.5f;

                float normalizeIconPosY =
                    ((data.uiTransform.position.z - mapMinZ) / (mapMaxZ - mapMinZ)) - 0.5f;

                float iconPosX = normalizeIconPosX * mapSize.x;
                float iconPosY = normalizeIconPosY * mapSize.y;

                Vector3 iconPos = new Vector3(iconPosX, iconPosY, 0.0f);

                RectTransform rect = icon.GetComponent<RectTransform>();
                rect.localPosition = iconPos;
                icon.GetComponent<Image>().sprite = data.texture;
                index++;

            }

        }


        
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

    void OnEnable()
    {
        MiniMapEvents.OnRegister += Register;
        MiniMapEvents.OnUnregister += Unregister;
    }

    void OnDisable()
    {
        MiniMapEvents.OnRegister -= Register;
        MiniMapEvents.OnUnregister -= Unregister;
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
                if(Icons[i].activeSelf)
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
        foreach(GameObject icon in Icons)
        {
            if(icon.activeSelf)
            {
                ret++;
            }
        }

        return ret;
          
    }

}
