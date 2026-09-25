using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Input Bindings")]
    [SerializeField] protected KeyCode left = KeyCode.A;
    [SerializeField] protected KeyCode down = KeyCode.S;
    [SerializeField] protected KeyCode right = KeyCode.D;

    [Header("Movement Settings")]
    [SerializeField] protected float baseSpeed = 5f;
    [SerializeField] protected int jumpsRemaining;
    [SerializeField] public int maxJumps = 2;

    [SerializeField] protected bool facingRight = true;
    protected float _horizontalInput = 0; // 0 is idle, -1 is left, 1 is right

    [Header("Physics")]
    [SerializeField] protected float gravity = 6f;
    [SerializeField] protected float fallSpeedMultiplier = 2f;
    [SerializeField] protected float maxFallSpeed = 18f;
    [SerializeField] protected float mass = 1;
    [SerializeField] public LayerMask wallLayer;

    [Header("Glide Settings")]
    [SerializeField] protected float glideGravity = 2f;
    [SerializeField] protected float glideMaxFallSpeed = 4f;
    [SerializeField] protected float glideSpeedMultiplier = 1.2f;

    [Header("Glide Visual")]
    [SerializeField] private Color glideColor = new Color(0.4f, 0.75f, 1f, 1f);
    private bool WantsToGlide = false;
    private SpriteRenderer spriteRenderer;
    private Color normalColor;


    [Header("Collision Variables")]
    [SerializeField] public Transform groundCheckPos;
    [SerializeField] public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
    [SerializeField] public LayerMask groundLayer;

    // Player States
    public enum STATE { Grounded, Falling }
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
                // case STATE.Rising: OnRising_Hook();
                //     break;
                // case STATE.Hanging: OnHanging_Hook();
                //     break;
                case STATE.Falling:
                    break;
            }
        }
    }


    // Unity Components
    private Rigidbody2D _playerBody;
    private BoxCollider2D _playerCollider;


    // Start is called before the first frame update
    protected void Start()
    {
        _playerBody = GetComponent<Rigidbody2D>();
        _playerCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Sprite may eventually be a child of the player object hence using getcomponentinchildren
        if(spriteRenderer != null){
            normalColor = spriteRenderer.color;
        }
    }

    // Update is called once per frame
    protected void Update()
    {
        _horizontalInput = 0;
        if (Input.GetKey(right))
        {
            _horizontalInput += 1;
        }
        if (Input.GetKey(left))
        {
            _horizontalInput -= 1;
        }

        groundCheck();
        _ApplyGravity();
        UpdateGlideVisual();
    }

    protected virtual void FixedUpdate()
    {
        // Actually move in fixed update to avoid kinematic body glitches
        Move();
    }

    private void Move()
    {
        float speedMultiplier;

        if (IsGliding()){
            speedMultiplier = glideSpeedMultiplier;
        }
        else
        {
            speedMultiplier = 1f;
        }

        // Says "right", but _horizontalInput flips the direction left if player presses Left
        transform.position += Vector3.right * (_horizontalInput * Time.deltaTime * baseSpeed * speedMultiplier);
    }

    private void _ApplyGravity()
    {
        if (currentState == STATE.Falling){
            
            if (IsGliding()){
                
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

    public void Jump(float jumpVelocity)
    {
        if (jumpsRemaining > 0)
        {
            _playerBody.velocity = new Vector2(_playerBody.velocity.x, jumpVelocity);
            jumpsRemaining--;

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
    private void groundCheck()
    {
        // Overlaps is calculated with an invisible box, not an invisible ray
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            // Only change to Grounded if it wasn't already to avoid jump count errors
            if (currentState != STATE.Grounded)
            {
                currentState = STATE.Grounded;
                jumpsRemaining = maxJumps;
            }
        }
        else
        {
            currentState = STATE.Falling;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draws a bounding box for the ground detection collider
        // You can turn this off by deselcting Gizmos in the Scene view
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }

    private void UpdateGlideVisual(){
        if (spriteRenderer == null){
            return;}

        if (IsGliding()){
            spriteRenderer.color = glideColor;
        }
        else
        {
            spriteRenderer.color = normalColor;
        }
    }
}
