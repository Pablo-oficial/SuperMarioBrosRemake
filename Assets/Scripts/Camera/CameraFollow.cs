using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        
    public float followSpeed = 5f;    
    public float xOffset = 0f;     

    private float initialY;
    private float initialZ;
    private float currentX; 

    void Start()
    {
        initialY = transform.position.y;
        initialZ = transform.position.z;
        currentX = transform.position.x;
    }

    void Update()
    {
        float targetX = target.position.x + xOffset;

        if (targetX > currentX)
        {
            currentX = targetX;
        }

        Vector3 targetPos = new Vector3(currentX, initialY, initialZ);
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
