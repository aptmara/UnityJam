using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using UnityEngine;

public class MapUITestScript : MonoBehaviour
{
    // もしあなたがこのコードを見ているならこのcsファイルを消し飛ばしてください
    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.3f);
        GameManager.Instance.ChangeState(GameState.Gameplay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
