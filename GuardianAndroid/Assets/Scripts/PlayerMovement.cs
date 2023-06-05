using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Animator myAnimator;

    // Speed
    private float playerSpeed = 10f;

    // Jump
    private float jumpForce = 40f;
    private int jumpCounter = 0;
    private bool isJumping = false;

    // Movement
    private float xMovement;
    private bool playerDirectionRight = true;

    // Dash
    private bool canDash = true;
    private bool isDashing;
    private float dashPower = 15f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    // Wall Slide
    private bool isWallSliding;
    private float wallSlidingSpeed = 2f;

    // Wall Jump
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(8f, 16f);

    [SerializeField] private Rigidbody2D rb2D;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private DashTrail dashTrail;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    private void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
    }

    private void Update()
    {
        xMovement = Input.GetAxisRaw("Horizontal");

        if (isDashing)
        {
            return;
        }

        if (Input.GetButtonDown("Jump") && jumpCounter < 2)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
            isJumping = true;
            jumpCounter++;
            myAnimator.SetBool("falling", false);
            myAnimator.SetTrigger("jump");
        }

        if (Input.GetButtonDown("Jump") && rb2D.velocity.y > 0f)
        {
            isJumping = true;
            myAnimator.SetBool("falling", false);
            myAnimator.SetTrigger("jump");
            rb2D.velocity = new Vector2(rb2D.velocity.x, rb2D.velocity.y * 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        if (rb2D.velocity.y < 0)
        {
            myAnimator.ResetTrigger("jump");
            myAnimator.SetBool("falling", true);
            isJumping = false;
        }
        
        if (IsGrounded() && !isJumping)
        {
            myAnimator.SetBool("falling", false);
            jumpCounter = 0;
        }

        WallSlide();
        //WallJump();

        Flip(xMovement);
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        rb2D.velocity = new Vector2(xMovement*playerSpeed, rb2D.velocity.y);
        myAnimator.SetFloat("speed", Mathf.Abs(xMovement));
        HandleLayers();

        Flip(xMovement);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.5f, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 1f, wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && xMovement != 0)
        {
            jumpCounter = 0;
            isWallSliding = true;
            rb2D.velocity = new Vector2(rb2D.velocity.x, Mathf.Clamp(rb2D.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
            isWallSliding = false; 
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb2D.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (transform.localScale.x != wallJumpingDirection)
            {
                playerDirectionRight = !playerDirectionRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= 1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb2D.gravityScale;
        rb2D.gravityScale = 0f;

        myAnimator.SetBool("dashing", true);
        rb2D.velocity = new Vector2(transform.localScale.x * dashPower, 0f);
        dashTrail.SetEnabled(true);
        yield return new WaitForSeconds(dashingTime);

        //myAnimator.SetBool("dashing", false);
        dashTrail.SetEnabled(false);
        myAnimator.SetBool("dashing", false);
        rb2D.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void Flip(float x)
    {
        if(x < 0 && playerDirectionRight || x > 0 && !playerDirectionRight)
        {
            playerDirectionRight = !playerDirectionRight;

            Vector3 playerScale = transform.localScale;
            playerScale.x *= -1f;
            transform.localScale = playerScale;
        }
    }

    private void HandleLayers()
    {
        if (!IsGrounded())
        {
            myAnimator.SetLayerWeight(1, 1);
            myAnimator.SetLayerWeight(0, 0);
        }
        else
        {
            myAnimator.SetLayerWeight(1, 0);
            myAnimator.SetLayerWeight(0, 1);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
            myAnimator.SetLayerWeight(2, 1);
        else
            myAnimator.SetLayerWeight(2, 0);
    }
}
