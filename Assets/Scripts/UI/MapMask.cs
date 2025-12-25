using System.Collections;
using System.Collections.Generic;
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

    float timer;

    public int mapSize = 256;
    Texture2D fogTexture;


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
            pixels[i] = new Color32(0, 0, 0, 0);

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

        int radius = 80;

        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                int px = x + dx;
                int py = y + dy;

                if (px < 0 || px >= mapSize || py < 0 || py >= mapSize)
                    continue;

                fogTexture.SetPixel(px, py, new Color(1, 1, 1, 1));

                //Debug.Log("Debug");
            }

        fogTexture.Apply();
    }
}
