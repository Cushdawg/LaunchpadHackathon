using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 25f;
    private bool isFacingRight = true;
    public new string name = "";
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    private void Start()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
    }

    void Update()
    {
        

    }

    public void UpdateX(float x)
    {
        horizontal = x;
        rb.velocity = new Vector2 (x / 10, rb.velocity.y);
        Flip();
    }

    public void Jump()
    {
        if (IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public string GetName()
    {
        return this.name;
    }

    private void FixedUpdate()
    {
        
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        Debug.Log(isFacingRight);
        Debug.Log(horizontal);
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
 
        {
            Debug.Log("flip");
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}