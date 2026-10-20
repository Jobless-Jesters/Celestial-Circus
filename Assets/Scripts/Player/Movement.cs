using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] protected float baseSpeed = 5f;
    [SerializeField] public float jumpPower = 30f;
    [SerializeField] protected int jumpsRemaining;
    [SerializeField] public int maxJumps = 2;
    [SerializeField] protected bool isFacingRight = true;
    protected float _horizontalInput = 0; // 0 is idle, -1 is left, 1 is right
    private bool jumpPressed;

    // Set by a tightrope while the player is bouncing on it. Called when Jump is pressed;
    // returns true if the rope used the press, in which case no normal jump happens.
    public Func<bool> JumpInterceptor;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;
    private bool WantsToGlide = false;

    [Header("Physics")]
    [SerializeField] protected float gravity = 6f;
    [SerializeField] protected float fallSpeedMultiplier = 2f;
    [SerializeField] protected float maxFallSpeed = 18f;
    [SerializeField] protected float mass = 1;


    [Header("Glide Settings")]
    [SerializeField] protected float glideGravity = 2f;
    [SerializeField] protected float glideMaxFallSpeed = 4f;
    [SerializeField] protected float glideSpeedMultiplier = 1.2f;

    [Header("Wall Slide Settings")]
    private float wallJumpLockTimer;
    [SerializeField] protected float wallSlideSpeed = 2f;
    [SerializeField] protected Vector2 wallJumpPower = new Vector2(12f, 30f);
    [SerializeField] private float wallJumpLockTime = 0.2f; // Controls how "long" to ignore horizontal movement for, shorter is more control/responsive, enables a "push" off a wall

    [Header("Animation")]
    public Animator animator;
    //Temporary colours for death animation
    [SerializeField] private Color deathColour = new Color(1.0f, 0.0f, 0f, 1f);
    [SerializeField] private Color normalColour = new Color(1.0f, 1.0f, 1f, 1f);

    [Header("Particle Effects")]
    public ParticleSystem dustFX;
    public ParticleSystem twirlFX;
    public ParticleSystem tightropeLaunchFX;


    [Header("Collision Variables")]
    [SerializeField] public Transform groundCheckPos;
    [SerializeField] public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public Transform wallCheckPos;
    [SerializeField] public Vector2 wallCheckSize = new Vector2(0.5f, 0.05f);
    [SerializeField] public LayerMask wallLayer;
    // Player States
    public enum STATE { Grounded, Falling, WallSliding }
    public STATE _currentState = STATE.Falling;
    [HideInInspector]
    public STATE currentState
    {
        get => _currentState;
        protected set
        {
            if (_currentState == value)
            {
                return;
            }
            _currentState = value;
            switch (_currentState)
            {
                case STATE.Grounded:
                    break;
                case STATE.Falling:
                    break;
            }
        }
    }

    // Unity Components
    private Rigidbody2D _playerBody;
    private BoxCollider2D _playerCollider;
    private SpriteRenderer spriteRenderer;
    private PlayerInput playerInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;


    // Start is called before the first frame update
    protected void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        _moveAction = playerInput.actions.FindAction("Move", true);
        _jumpAction = playerInput.actions.FindAction("Jump", true);
        playerInput.enabled = false;  // cannot move at start of level

        _playerBody = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Disable player input until the curtains are fully open
        StartCoroutine(EnableMoveAfterCurtain(playerInput));
    }

    private IEnumerator EnableMoveAfterCurtain(PlayerInput ip)
    {
        Debug.Log("Entered starting coroutine");
        // Wait specfied time for curtain open animation to finish
        yield return new WaitForSecondsRealtime(2.8f);

        Debug.Log("Exited starting coroutine, player can move");
        ip.enabled = true;  // Re-enable inputs, player can now start playing
    }

    // Update is called once per frame
    protected void Update()
    {
        // Capture key-presses in Update to allow for simultaneous vertical and horizontal movement. 
        // Otherwise, can only jump in time with the fixed update, which eats inputs
        if (_jumpAction.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        if (GameController.Instance.getPlayerIsAlive() == false)
        {
            animator.SetFloat("yVelocity", 0);
            animator.SetFloat("magnitude", 0);
            animator.SetBool("isGliding", false);
            animator.SetBool("isGrounded", false);
            animator.SetBool("isAlive", false);
            spriteRenderer.color = deathColour;
        }
        else
        {
            animator.SetBool("isAlive", true);
            spriteRenderer.color = normalColour;
        }
    }

    protected virtual void FixedUpdate()
    {
        // Actually move in fixed update to avoid kinematic body glitches
        _horizontalInput = _moveAction.ReadValue<Vector2>().x;

        groundCheck();
        _ApplyGravity();
        TwirlSFXLoop();
        Flip();
        WallSlide();
        Jump();
        Move();

        animator.SetFloat("yVelocity", _playerBody.velocity.y);
        animator.SetFloat("magnitude", _playerBody.velocity.magnitude);
        animator.SetBool("isGliding", IsGliding());
    }

    private bool _isWalking = false;
    private bool _wasTwirling = false;

    private void Move()
    {
        float speedMultiplier;

        if (IsGliding())
        {
            speedMultiplier = glideSpeedMultiplier;
        }
        else
        {
            speedMultiplier = 1f;
        }

        wallJumpLockTimer -= Time.deltaTime;
        if (wallJumpLockTimer <= 0f) // Guard against regular movement during a wall jump 
        {
            // Says "right", but _horizontalInput flips the direction left if player presses Left
            _playerBody.velocity = new Vector2(_horizontalInput * baseSpeed * Time.deltaTime * speedMultiplier, _playerBody.velocity.y);
        }

        // Sets up co-routine to check if player is walking so that walkSFX can be called outside of update
        if (currentState == STATE.Grounded && _horizontalInput != 0)
        {
            if (!_isWalking)
            {
                _isWalking = true;
                StartCoroutine(WalkSFXLoop());
            }
        }
        else
        {
            _isWalking = false;
        }
    }

    // Calling walkSFX outside of update so that it can be delayed
    IEnumerator WalkSFXLoop()
    {
        while (_isWalking)
        {
            AudioController.Instance.PlayWalkSFX();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void TwirlSFXLoop()
    {
        bool isTwirling = IsGliding();
        if (isTwirling && !_wasTwirling)
        {
            AudioController.Instance.PlayTwirlSFX();
            twirlFX.Play();
        }

        if (!isTwirling && _wasTwirling)
        {
            AudioController.Instance.StopTwirlSFX();
            twirlFX.Stop();
        }

        _wasTwirling = isTwirling;
    }

    private void _ApplyGravity()
    {
        if (currentState == STATE.Falling)
        {

            if (IsGliding())
            {
                // Reduced gravity while gliding
                _playerBody.gravityScale = glideGravity;

                // Prevent the player from falling faster than the glide speed
                _playerBody.velocity = new Vector2(
                    _playerBody.velocity.x,
                    Mathf.Max(_playerBody.velocity.y, -glideMaxFallSpeed)
                );

            }

            else
            {
                // Normal falling physics
                _playerBody.gravityScale = gravity * fallSpeedMultiplier;

                _playerBody.velocity = new Vector2(
                    _playerBody.velocity.x,
                    Mathf.Max(_playerBody.velocity.y, -maxFallSpeed)
                );
            }
        }
        else
        {
            // Reset gravity when grounded
            _playerBody.gravityScale = gravity;
        }

    }

    public void Jump()
    {
        // Capture the key press in Update, then handle physics in FixedUpdate.
        if (jumpPressed)
        {
            jumpPressed = false;
            jumpBufferCounter = jumpBufferTime;

            // A tightrope can claim the press for a timed boost
            if (JumpInterceptor != null && JumpInterceptor())
            {
                jumpBufferCounter = 0f;
            }

        }

        if (jumpBufferCounter > 0f)
        {
            // Gives coyote time on the first jump; double jumps work normally.
            if (jumpsRemaining > 1 && coyoteTimeCounter > 0f)
            {
                if (IsWallSliding())
                {
                    WallJump();
                }
                else
                {
                    _playerBody.velocity = new Vector2(_playerBody.velocity.x, jumpPower);
                    jumpsRemaining--;
                }

                animator.SetTrigger("jump");
                currentState = STATE.Falling;
                animator.SetBool("isGrounded", false);
                dustFX.Play();
                AudioController.Instance.PlayJumpSFX();
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
            }
            else if (jumpsRemaining > 0)
            {
                if (IsWallSliding())
                {
                    WallJump();
                }
                else
                {
                    _playerBody.velocity = new Vector2(_playerBody.velocity.x, jumpPower);
                    jumpsRemaining--;
                }


                animator.SetTrigger("jump");
                currentState = STATE.Falling;
                animator.SetBool("isGrounded", false);
                dustFX.Play();
                AudioController.Instance.PlayJumpSFX();
                jumpBufferCounter = 0f;
            }
            else
            {
                jumpBufferCounter -= Time.fixedDeltaTime;
            }
        }

        // Holding Space tells Movement that the player wants to glide
        SetGliding(_jumpAction.IsPressed());
    }

    public void WallJump()
    {
        float jumpDirection = -transform.localScale.x; // Opposite direction of wall
        _playerBody.velocity = new Vector2(wallJumpPower.x * jumpDirection, wallJumpPower.y);
        wallJumpLockTimer = wallJumpLockTime; // Reset wall jump duration
        if (jumpDirection != transform.localScale.x) // Flip character direction
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;
        }
    }

    public void LaunchFromTightrope(float launchVelocity, bool playJumpAnimation = true)
    {
        // Force the player into the air
        currentState = STATE.Falling;

        // Guaranteed upward velocity
        _playerBody.velocity = new Vector2(_playerBody.velocity.x, launchVelocity);

        // Give the player their air jumps after being launched
        jumpsRemaining = maxJumps - 1;

        // Prevent old grounded/coyote state from interfering
        coyoteTimeCounter = 0f;
        jumpBufferCounter = 0f;

        if (playJumpAnimation)
        {
            animator.SetTrigger("jump");
            tightropeLaunchFX.Play(); // TODO: change this tighrope launch effect to something cooler
        }
    }

    public void SetGliding(bool gliding)
    {
        WantsToGlide = gliding; //called by Jump.cs 
    }

    private bool IsGliding()
    {
        return WantsToGlide
        && currentState == STATE.Falling
        && _playerBody.velocity.y < 0; // makes sure that glide only activates when the player is actually moving downwards

    }

    private bool IsWallSliding()
    {
        return Physics2D.OverlapCircle(wallCheckPos.position, 0.2f, wallLayer); // Check if colliding with wall
    }

    private void WallSlide()
    {
        if (IsWallSliding() && currentState != STATE.Grounded)
        {
            currentState = STATE.WallSliding;
            jumpsRemaining = maxJumps - 1; // Reset to jump counter - 1
            _playerBody.velocity = new Vector2(_playerBody.velocity.x, Mathf.Clamp(_playerBody.velocity.y, -wallSlideSpeed, float.MaxValue)); // Control sliding speed
        }
        else if (currentState == STATE.WallSliding)
        {
            currentState = STATE.Falling;
        }
    }

    private void groundCheck()
    {
        // Overlaps is calculated with an invisible box, not an invisible ray
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer) && _playerBody.velocity.y <= 0.1f)
        {
            // Only change to Grounded if it wasn't already to avoid jump count errors
            if (currentState != STATE.Grounded)
            {
                animator.SetBool("isGrounded", true);
                currentState = STATE.Grounded;  // fixes animator issues
                jumpsRemaining = maxJumps;
                coyoteTimeCounter = coyoteTime;
            }
        }
        else
        {
            // Avoid repeatedly assigning the same value to a variable every tick
            if (currentState != STATE.Falling)
            {
                animator.SetBool("isGrounded", false);  // fixes animator issues
                currentState = STATE.Falling;
                coyoteTimeCounter -= Time.fixedDeltaTime;
            }
        }
    }

    private void Flip()
    {
        if ((isFacingRight && _horizontalInput < 0) || (!isFacingRight && _horizontalInput > 0))
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale = ls;

            if (currentState == STATE.Grounded)
            {
                dustFX.Play();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draws a bounding box for the ground detection collider
        // You can turn this off by deselcting Gizmos in the Scene view

        // Ground check gizmo
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        
        // Wall check gizmo
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
}