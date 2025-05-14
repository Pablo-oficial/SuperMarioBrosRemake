using System.Collections;
using UnityEngine;

public class Goomba : MonoBehaviour
{
    public float moveSpeed = 1f;
    private bool isDead = false;
    private bool isFrozen = false;
    private Rigidbody2D rb;
    private Animator anim;
    private bool movingLeft = true;
    private bool wasVisible = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.linearVelocity = new Vector2(-moveSpeed, 0);
    }

    void Update()
    {
        if (isDead || isFrozen) return;

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

        if (!inView) return;

        float direction = movingLeft ? -1f : 1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (rb.linearVelocity.x != 0)
            transform.eulerAngles = new Vector3(0, rb.linearVelocity.x < 0 ? 0 : 180, 0);
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
        transform.eulerAngles = new Vector3(0, movingLeft ? 0 : 180, 0);
    }

    public void Die()
    {
        if (isDead || isFrozen) return;

        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        if (anim != null)
            anim.SetTrigger("Death");

        StartCoroutine(DeathSequence());
    }

    public void Freeze()
    {
        if (isDead || isFrozen) return;

        isFrozen = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        if (anim != null)
            anim.speed = 0f;
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isFrozen) return;

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