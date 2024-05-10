using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] float speed = 5f;
    [SerializeField] float firstJumpingForce = 6.5f;
    [SerializeField] float secondJumpingForce = 5f;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip doubleJumpClip;
    [SerializeField] LayerMask mapLayer;
    [SerializeField] float castDistance = .1f;
    [SerializeField] float colliderToBoxCastDif = .01f;

    Rigidbody2D rb;
    Animator animator;
    AudioSource audioSource;
    IGameManager gm;

    float horMovement = 0f;
    bool wantsToJump = false;
    bool facingRight = true;
    bool grounded { get { return isGrounded(); } }
    bool jumping = false;
    BoxCollider2D boxCollider;
    bool onTheWall { get { return isNextToTheWall(); } }


    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        var gmOb = GameObject.Find("GameManager");
        if (gmOb == null) gmOb = GameObject.Find("NetGameManager");
        gm = gmOb.GetComponent<IGameManager>();

    }

    // Start is called before the first frame update
    private void Update()
    {
        //Process input   
        horMovement = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) wantsToJump = true;

        ControlCharacterOrientation();
        //Animation
        animator.SetBool("Running", horMovement != 0);
        animator.SetBool("Grounded", grounded);
        animator.SetBool("OnTheWall", onTheWall);
    }

    private void ControlCharacterOrientation()
    {
        if (rb.velocity.x > 0.01f && !facingRight)
        {
            facingRight = true;
            transform.localScale = Vector3.one;
        }
        else if (rb.velocity.x < -0.01f && facingRight)
        {
            facingRight = false;
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(horMovement * speed, rb.velocity.y);
        if (wantsToJump)
        {
            if (grounded) Jump();
            else if (jumping || isNextToTheWall()) DoubleJump();
            wantsToJump = false;
        }
    }

    private void DoubleJump()
    {
        jumping = false;
        if (isNextToTheWall())
        {
            rb.velocity = new Vector2(facingRight ? -1f : 1f, secondJumpingForce);
        }
        else rb.velocity = new Vector2(rb.velocity.x, secondJumpingForce);
        animator.SetTrigger("DoubleJumps");
        audioSource.PlayOneShot(doubleJumpClip);
    }

    private void Jump()
    {
        jumping = true;
        rb.velocity = new Vector2(rb.velocity.x, firstJumpingForce);
        animator.SetTrigger("Jumps");
        audioSource.PlayOneShot(jumpClip);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            gm.OnPlayerDeath();
            Destroy(gameObject);
        }
    }

    bool isGrounded()
    {
        var boxCastOrigin = boxCollider.bounds.center;
        var boxCastSize = new Vector2(boxCollider.bounds.size.x - colliderToBoxCastDif, boxCollider.bounds.size.y);
        var boxCastHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0, Vector2.down, castDistance, mapLayer);
        return boxCastHit.collider != null;

        /*Vector2 rayCastOrigin = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        var rayCastHit = Physics2D.Raycast(rayCastOrigin, Vector3.down, castDistance, mapLayer);
        return rayCastHit.collider != null;*/
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var cubeOrigin = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.center.y - castDistance);
        var cubeSize = new Vector2(boxCollider.bounds.size.x - colliderToBoxCastDif, boxCollider.bounds.size.y);
        Gizmos.DrawWireCube(cubeOrigin, cubeSize);

        /*Vector2 rayCastOrigin = new Vector2(boxCollider.bounds.center.x, boxCollider.bounds.min.y);
        Gizmos.DrawLine(rayCastOrigin, rayCastOrigin - new Vector2(0, castDistance));*/
    }

    bool isNextToTheWall()
    {
        Vector2 directionToTest = facingRight ? Vector2.right : Vector2.left;
        var boxCastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, directionToTest, castDistance, mapLayer);
        return boxCastHit.collider != null;
    }
}
