using System;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;

    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothSpeed = 0.125f;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            offset = transform.position - player.position;
        }
        else
        {
            Debug.LogWarning("no Player");
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}
