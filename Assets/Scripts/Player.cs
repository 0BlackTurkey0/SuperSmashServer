using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int ID;
    public string Name;
    public int KnockBackPoint;
    public int DeathCount;

    public float Invincible;

    public bool moveRight, moveLeft;
    public bool jump;
    public bool lightATK, heavyATK;
    public bool dodge;
    public float dodgeCD;
    public float hitRecover;

    [Header("水平速度")]
    [Range(0, 2)]
    public float speedX;

    [Header("垂直向上推力")]
    [Range(0, 150)]
    public float forceY;

    [Header("最大跳躍次數")]
    public int MaxJumpCount;

    [Header("跳躍次數")]
    public int jumpCount;

    [Header("地面圖層")]
    public LayerMask groundLayer;

    public Collider2D playerCollider2D;
    public Rigidbody2D playerRigidbody2D;
    public SpriteRenderer playerSpriteRenderer;
    public Animator playerAnimator;
    public bool grounded;

    private bool DoJump, DoMovement;
    public int horizontalDirection;

    public void InputJump () {

        Color rc;
        if (Physics2D.Raycast(playerCollider2D.bounds.center, Vector2.down, playerCollider2D.bounds.extents.y + 0.02f, groundLayer).collider != null)
            rc = Color.green;
        else
            rc = Color.red;
        Debug.DrawRay(playerCollider2D.bounds.center, Vector2.down * (playerCollider2D.bounds.extents.y + 0.02f), rc);

        if (IsGround)
            jumpCount = 0;
        if (jump)
            DoJump = true;
        jump = false;
    }

    public void Jump () {
        if (DoJump) {
            if (jumpCount < MaxJumpCount) {
                jumpCount++;
                playerRigidbody2D.velocity = new Vector2(0, 0);
                playerRigidbody2D.AddForce(Vector2.up * forceY);
            }
            DoJump = false;
        }
    }

    bool IsGround {
        get {
            return Physics2D.Raycast(playerCollider2D.bounds.center, Vector2.down, playerCollider2D.bounds.extents.y + 0.02f, groundLayer);
        }
    }

    public void InputMovement () {
        if (moveRight && !moveLeft)
            horizontalDirection = 1;
        else if (moveLeft && !moveRight)
            horizontalDirection = -1;
        moveRight = false;
        moveLeft = false;
        if (horizontalDirection != 0) {
            DoMovement = true;
            playerAnimator.SetInteger("Status", 1);
            if (horizontalDirection == 1)
                playerSpriteRenderer.flipX = false;
            else
                playerSpriteRenderer.flipX = true;
        }
        else
            playerAnimator.SetInteger("Status", 0);
    }

    public void Movement () {
        if (DoMovement) {
            playerRigidbody2D.transform.Translate(new Vector2(speedX * Time.deltaTime * horizontalDirection, 0));
            horizontalDirection = 0;
            DoMovement = false;
        }
    }

    public void Attack () {
        if (lightATK) {
            lightATK = false;
            StartCoroutine(Smash());
            hitRecover = 0.15f;
        }
        else if (heavyATK) {
            heavyATK = false;
            hitRecover = 0.8f;
        }
    }

    public void Dodge () {
        if (dodge && dodgeCD == 0) {
            dodge = false;
            dodgeCD = 5f;
            Invincible = 0.1f;
            hitRecover = 0.1f;
        }
    }

    public void KnockBack(Vector2 dir) {
        if (Invincible == 0f) {
            KnockBackPoint += 20;
            hitRecover = 0.1f;
            Invincible = 0.1f;
            playerRigidbody2D.AddForce(dir * KnockBackPoint);
        }
    }

    public IEnumerator Smash() {
        yield return new WaitForSeconds(0.05f);
        float Offset;
        if (playerSpriteRenderer.flipX)
            Offset = -0.03f;
        else
            Offset = 0.03f;
        GameObject Claw = Instantiate(Resources.Load("Claw") as GameObject, new Vector3(transform.position.x + Offset, transform.position.y, 1), Quaternion.identity);
        if (playerSpriteRenderer.flipX)
            Claw.GetComponent<SpriteRenderer>().flipX = true;
        Collider2D clawCollider2D = Claw.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider2D, clawCollider2D, true);
    }

    public IEnumerator Respawn() {
        yield return new WaitForSeconds(3f);
        hitRecover = 0f;
        playerRigidbody2D.gravityScale = 1f;
        transform.position = new Vector3(0f, 0f, 1f);
    }

    void Start () {
        playerCollider2D = GetComponent<Collider2D>();
        playerRigidbody2D = GetComponent<Rigidbody2D>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<Animator>();
        DeathCount = 0;
        Invincible = 0f;
        dodgeCD = 0f;
        DoJump = false;
        DoMovement = false;
        moveRight = false;
        moveLeft = false;
        jump = false;
        lightATK = false;
        heavyATK = false;
        dodge = false;
        KnockBackPoint = 100;
        hitRecover = 0f;
    }

    void Update () {
        if (Invincible > 0f) {
            if (Invincible - Time.deltaTime > 0f)
                Invincible -= Time.deltaTime;
            else
                Invincible = 0f;
        }
        if (dodgeCD > 0f) {
            if (dodgeCD - Time.deltaTime > 0f)
                dodgeCD -= Time.deltaTime;
            else
                dodgeCD = 0f;
        }
        if (hitRecover == 0f) {
            InputMovement();
            InputJump();
            Attack();
            Dodge();
        }
        else {
            moveRight = false;
            moveLeft = false;
            jump = false;
            lightATK = false;
            heavyATK = false;
            if (hitRecover - Time.deltaTime > 0f)
                hitRecover -= Time.deltaTime;
            else
                hitRecover = 0f;
        }
        if (dodgeCD != 0)
            dodge = false;
    }

    void FixedUpdate () {
        Movement();
        Jump();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 11) {
            DeathCount++;
            hitRecover = 5f;
            Invincible = 5f;
            KnockBackPoint = 100;
            playerSpriteRenderer.flipX = false;
            playerRigidbody2D.gravityScale = 0f;
            playerRigidbody2D.velocity = new Vector2(0f, 0f);
            StartCoroutine(Respawn());
        }
    }
}
