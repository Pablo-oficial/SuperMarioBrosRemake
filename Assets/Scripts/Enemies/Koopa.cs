using UnityEngine;

public class Koopa : MonoBehaviour
{
    public float moveSpeed = 1f;
    public bool isDead = false;
    private Rigidbody2D rb;
    private Animator anim;
    private bool movingLeft = true;
    private bool wasVisible = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        bool inView = IsInCameraView();

        if (inView)
        {
            wasVisible = true;
        }
        else if (wasVisible && !inView)
        {
            Destroy(gameObject);
            return;
        }

        if (isDead || !inView) return;

        float direction = movingLeft ? -1f : 1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private bool IsInCameraView()
    {
        if (Camera.main == null) return false;

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewportPos.x >= 0f && viewportPos.x <= 1f &&
                viewportPos.y >= 0f && viewportPos.y <= 1f &&
                viewportPos.z > 0);
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
        if (isDead) return;

        isDead = true;
        rb.linearVelocity = Vector2.zero;

        anim?.SetTrigger("Death");

        Invoke(nameof(DestroySelf), 0.5f);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player")) return;

        foreach (var contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > Mathf.Abs(contact.normal.y))
            {
                Flip();
                break;
            }
        }
    }
}
