using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using UnityEngine;
using UnityJam.UI;

public class PlayerMapChange : MonoBehaviour
{
    // Player内で使いまわすためにenum分ける
    public enum PlayerMapState
    {
        UseMap,
        DontUseMap,
        NotAvailable
    }

    public PlayerMapState playerMapState { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        playerMapState = PlayerMapState.DontUseMap;
    }

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(1.8f);
        playerMapState = PlayerMapState.DontUseMap;
        MapUIEvents.UseMiniMap();
        MapTargetRegister.Register(this.gameObject.transform);
        Debug.Log("Player Enable");
    }


    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            switch (playerMapState)
            {
                case PlayerMapState.UseMap:
                    playerMapState = PlayerMapState.DontUseMap;
                    MapUIEvents.UseMiniMap();
                    break;

                case PlayerMapState.DontUseMap:
                    playerMapState = PlayerMapState.UseMap;
                    MapUIEvents.UseMap();
                    break;
            }
        }
    }

}
