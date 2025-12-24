using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] float speed = 10.0f;
    private Transform transform;
    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform!=null)
        {
            transform.Rotate(0.0f, speed * Time.deltaTime, 0.0f);
        }
    }
}
