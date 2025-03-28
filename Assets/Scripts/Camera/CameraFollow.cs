using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    public float FollowSpeed = 2f;
    public Vector2 offset = new Vector2(2f, 4f);

    public Transform target;

    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x + offset.x, target.position.y + offset.y, -10f);
        transform.position = Vector3.Lerp(transform.position, newPos, FollowSpeed*Time.deltaTime);
    }
}
