using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private float halfWidth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            halfWidth = col.bounds.extents.x;
    }

    void Update()
    {
        if (isDead) return;

        float move = Input.GetAxis("Horizontal");

        Vector3 pos = transform.position;
        Vector3 left = Camera.main.ViewportToWorldPoint(new Vector3(0.03f, 0f, 0f));
        Vector3 right = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f));
        if ((move < 0 && pos.x - halfWidth <= left.x) ||
            (move > 0 && pos.x + halfWidth >= right.x))
        {
            move = 0;
        }

        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
        anim?.SetFloat("Move", Mathf.Abs(move));

        bool isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim?.SetTrigger("Jump");
        }

        if (move != 0)
            transform.eulerAngles = new Vector3(0, move < 0 ? 180f : 0f, 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            bool stomped = false;
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    stomped = true;
                    break;
                }
            }

            Goomba enemy = collision.gameObject.GetComponent<Goomba>();
            if (stomped && enemy != null && !enemy.isDead)
            {
                enemy.Die();
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            else
            {
                Die();
            }
        }
    }

    void Die()
    {
        isDead = true;

        foreach (var c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;

        anim?.SetTrigger("Death");
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        rb.linearVelocity = new Vector2(0, 8f);
        yield return new WaitForSeconds(0.4f);
        rb.linearVelocity = new Vector2(0, -12f);
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
