using UnityEngine;

public class Goomba : MonoBehaviour
{
    public float moveSpeed = 1f;
    private Rigidbody2D rb;
    private bool movingLeft = false;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private bool IsInCameraView()
    {
        if (Camera.main == null)
            return false;
        
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewportPos.x >= 0f && viewportPos.x <= 1f &&
                viewportPos.y >= 0f && viewportPos.y <= 1f &&
                viewportPos.z > 0);
    }

    void Update()
    {
        if (isDead || !IsInCameraView()) return;

        float direction = movingLeft ? -1f : 1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        movingLeft = !movingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 0.5f);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            bool stomped = false;
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y >= 0.5f)
                {
                    stomped = true;
                    break;
                }
            }

            if (stomped)
            {
                Die();
                Rigidbody2D playerRb = collision.rigidbody;
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);
                }
            }
            return;
        }
        
        Flip();
    }
}