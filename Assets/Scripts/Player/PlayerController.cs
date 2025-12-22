using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] Transform playerTransform;
    [SerializeField] Rigidbody playerRigidbody;

    [Header("Camera")]
    [SerializeField] CameraRigController rig;

    [Header("Speed")]
    [SerializeField] float followSpeed = 360f; // 度 / 秒（小さいほどズレる）
    [SerializeField] float moveSpeed = 5f;

    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        float yaw = rig.yaw;

        CameraMove(dt, yaw);

        PlayerMove(dt, yaw);
    }


    void CameraMove(float dt, float targetYaw)
    {
       
        // 自身の現在の向き
        float currentYaw = playerTransform.eulerAngles.y;
        // 向きを追従してくれるやつ、非常に便利
        float newYaw = Mathf.MoveTowardsAngle(
            currentYaw,
            targetYaw,
            followSpeed * dt);

        playerTransform.rotation = Quaternion.Euler(0f, newYaw, 0f);
    }

    void PlayerMove(float dt, float yaw)
    {
        Vector3 moveVector = new Vector3(0.0f, 0.0f, 0.0f);

        float radYaw = yaw * Mathf.Deg2Rad;

        Vector3 forward = new Vector3(Mathf.Sin(radYaw), 0.0f, Mathf.Cos(radYaw));
        Vector3 right = new Vector3(Mathf.Sin(radYaw + Mathf.PI / 2), 0.0f, Mathf.Cos(radYaw + Mathf.PI / 2));

        if (Input.GetKey(KeyCode.W))
        {
            moveVector += forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveVector -= forward;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveVector += right;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveVector -= right;
        }

        // Unityはこれで正規化できるらしい
        if (moveVector.sqrMagnitude > 0.0f)
        {
            moveVector.Normalize();
            moveVector *= moveSpeed * dt;
        }

        moveVector.y = playerRigidbody.velocity.y;

        playerRigidbody.velocity = moveVector;
        
        
    }

}
