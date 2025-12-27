using System.Collections;
using System.Collections.Generic;
using Unity.UI;
using UnityEngine;

public class ChangeUi : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.ChangeState(GameState.Gameplay);
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
