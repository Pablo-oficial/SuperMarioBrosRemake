using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float maxSpeed = 6.5f; // Velocidade máxima normal
    public float runMaxSpeed = 10.5f; // Velocidade máxima ao correr
    public float acceleration = 0.2f; // Aceleração normal
    public float runAcceleration = 0.3f; // Aceleração ao correr
    public float deceleration = 0.3f; // Desaceleração (igual para normal e corrida)
    public float jumpForce = 16f;
    public float jumpHoldTime = 0.3f;
    public float gravityScale = 3.5f;
    public float fallingGravityScale = 5.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;
    private bool isGrounded = false;
    private bool isJumping = false;
    private float jumpTimer = 0f;
    private float moveInput = 0f;
    private float currentSpeed = 0f;
    private Collider2D[] colliders;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private float halfWidth;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        colliders = GetComponentsInChildren<Collider2D>();
        rb.gravityScale = gravityScale;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            halfWidth = col.bounds.extents.x;
    }

    void Update()
    {
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (anim != null)
            anim.SetBool("IsGrounded", isGrounded);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping)
        {
            isJumping = true;
            jumpTimer = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (anim != null)
                anim.SetTrigger("Jump");
        }

        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            jumpTimer += Time.deltaTime;
            if (jumpTimer < jumpHoldTime)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space) || jumpTimer >= jumpHoldTime)
        {
            isJumping = false;
        }

        if (anim != null)
        {
            anim.SetFloat("Move", Mathf.Abs(currentSpeed));
            // Ajustar velocidade da animação durante a corrida
            anim.speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? 1.5f : 1f;
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Verificar se está correndo (Shift pressionado)
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentMaxSpeed = isRunning ? runMaxSpeed : maxSpeed;
        float currentAcceleration = isRunning ? runAcceleration : acceleration;

        float targetSpeed = moveInput * currentMaxSpeed;
        if (Mathf.Abs(moveInput) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, currentAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);

        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallingGravityScale;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }

        Vector3 pos = transform.position;
        Vector3 left = Camera.main.ViewportToWorldPoint(new Vector3(0.03f, 0f, 10f));
        Vector3 right = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 10f));

        if (pos.x - halfWidth <= left.x && currentSpeed < 0)
        {
            currentSpeed = 0;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            pos.x = left.x + halfWidth;
        }
        else if (pos.x + halfWidth >= right.x && currentSpeed > 0)
        {
            currentSpeed = 0;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            pos.x = right.x - halfWidth;
        }

        transform.position = pos;

        if (moveInput != 0)
            transform.eulerAngles = new Vector3(0, moveInput < 0 ? 180f : 0f, 0);
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

            var enemy = collision.gameObject.GetComponent<MonoBehaviour>();
            if (stomped && enemy != null)
            {
                var method = enemy.GetType().GetMethod("Die");
                if (method != null)
                {
                    method.Invoke(enemy, null);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
            else
            {
                Die();
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            var enemyScript = enemy.GetComponent<MonoBehaviour>();
            if (enemyScript != null)
            {
                var freezeMethod = enemyScript.GetType().GetMethod("Freeze");
                if (freezeMethod != null)
                {
                    freezeMethod.Invoke(enemyScript, null);
                }
            }
        }

        foreach (var c in colliders)
            c.enabled = false;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        if (anim != null)
            anim.SetTrigger("Death");

        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        rb.linearVelocity = new Vector2(0, 12f);
        yield return new WaitForSeconds(0.5f);
        rb.linearVelocity = new Vector2(0, -8f);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}