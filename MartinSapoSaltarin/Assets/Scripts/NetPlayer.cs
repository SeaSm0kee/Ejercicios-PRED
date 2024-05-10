using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetPlayer : NetworkBehaviour
{

    [SerializeField] float speed = 5f;
    [SerializeField] float firstJumpingForce = 6.5f;
    [SerializeField] float secondJumpingForce = 5f;
    [SerializeField] AudioClip jumpClip;
    [SerializeField] AudioClip doubleJumpClip;
    [SerializeField] LayerMask mapLayer;
    [SerializeField] float castDistance = .1f;
    [SerializeField] float colliderToBoxCastDif = .01f;
    [SerializeField] TextMeshProUGUI playerText;
    public NetworkVariable<ulong> clientId;

    Rigidbody2D rb;
    Animator animator;
    AudioSource audioSource;
    SpriteRenderer spriteRenderer;
    IGameManager gm;

    float horMovement = 0f;
    bool wantsToJump = false;
    bool facingRight = true;
    bool grounded { get { return isGrounded(); } }
    bool jumping = false;
    BoxCollider2D boxCollider;
    bool onTheWall { get { return isNextToTheWall(); } }

    string playerName = "Player";

    AudioClip[] sounds;
    enum frogSounds
    {
        jump = 0,
        doubleJump
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            clientId.OnValueChanged += OnClientIdValueChanged;
            if (clientId.Value != 0)
            {
                playerName = "Player " + clientId.Value;
                playerText.text = playerName;
                gm.SetClientPlayer(gameObject, clientId.Value);
            }
        }
        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        clientId.OnValueChanged -= OnClientIdValueChanged;
        base.OnNetworkDespawn();
    }

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        var gmOb = GameObject.Find("GameManager");
        if (gmOb == null) gmOb = GameObject.Find("NetGameManager");
        gm = gmOb.GetComponent<IGameManager>();
        sounds = new AudioClip[] { jumpClip, doubleJumpClip };
    }
    public void SetPlayerId(ulong playerId)
    {
        clientId.Value = playerId;
        //SetPlayerIdClientRpc(playerId);
        playerName = "Player " + playerId;
        playerText.text = playerName;

    }

    void OnClientIdValueChanged(ulong previousValue, ulong newValue)
    {
        //clientId = playerId;
        playerName = "Player " + clientId.Value;
        playerText.text = playerName;
        gm.SetClientPlayer(gameObject, clientId.Value);
    }

    /*    [ClientRpc]
        public void SetPlayerIdClientRpc(ulong playerId)
        {
            clientId = playerId;
            playerName = "Player " + playerId;
            playerText.text = playerName;
            gm.SetClientPlayer(gameObject, playerId);
        }*/


    // Start is called before the first frame update
    private void Update()
    {
        //Process input   
        if (IsClient)
        {
            Prueba();
            /*if (clientId.Value != NetworkManager.Singleton.LocalClientId) return;
            horMovement = Input.GetAxisRaw("Horizontal");
            ProcessClientInputsServerRpc(horMovement, Input.GetButtonDown("Jump"));*/
        }
        else if (IsHost)
        {
            Debug.LogWarning("hi");
            Prueba();
            ControlCharacterOrientation();
            //Animation
            animator.SetBool("Running", horMovement != 0);
            animator.SetBool("Grounded", grounded);
            animator.SetBool("OnTheWall", onTheWall);
        }
    }

    void Prueba()
    {
        if (clientId.Value != NetworkManager.Singleton.LocalClientId) return;
        horMovement = Input.GetAxisRaw("Horizontal");
        ProcessClientInputsServerRpc(horMovement, Input.GetButtonDown("Jump"));
    }

    [ServerRpc(RequireOwnership = false)]
    public void ProcessClientInputsServerRpc(float movement, bool jumpButtonPressed)
    {
        horMovement = movement;
        if (jumpButtonPressed) wantsToJump = true;
    }

    private void ControlCharacterOrientation()
    {
        if (rb.velocity.x > 0.01f && !facingRight)
        {
            facingRight = true;
            spriteRenderer.flipX = false;
            ControlCharacterOrientationClientRpc(spriteRenderer.flipX);
        }
        else if (rb.velocity.x < -0.01f && facingRight)
        {
            facingRight = false;
            spriteRenderer.flipX = true;
            ControlCharacterOrientationClientRpc(spriteRenderer.flipX);
        }
    }

    [ClientRpc]
    private void ControlCharacterOrientationClientRpc(bool flipX)
    {
        spriteRenderer.flipX = flipX;
    }

    void FixedUpdate()
    {
        if (IsClient) return;
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
            if (IsServer) rb.velocity = new Vector2(facingRight ? -1f : 1f, secondJumpingForce);
        }
        else if (IsServer) rb.velocity = new Vector2(rb.velocity.x, secondJumpingForce);
        animator.SetTrigger("DoubleJumps");
        PlaySoundClientRpc(frogSounds.doubleJump);
    }

    private void Jump()
    {
        jumping = true;
        if (IsServer) rb.velocity = new Vector2(rb.velocity.x, firstJumpingForce);
        animator.SetTrigger("Jumps");
        PlaySoundClientRpc(frogSounds.jump);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(frogSounds sound)
    {
        audioSource.PlayOneShot(sounds[(int)sound]);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            gm.OnPlayerDeath(clientId.Value);
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
        if (boxCollider == null) return;
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
