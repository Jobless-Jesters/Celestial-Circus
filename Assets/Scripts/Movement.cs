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
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Physics")]
    [SerializeField] protected float gravity = 6f;
    [SerializeField] protected float fallSpeedMultiplier = 2f;
    [SerializeField] protected float maxFallSpeed = 18f;
    [SerializeField] protected float mass = 1;
    [SerializeField] public LayerMask wallLayer;

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
    }

    protected virtual void FixedUpdate()
    {
        // Actually move in fixed update to avoid kinematic body glitches
        Move();
    }

    private void Move()
    {
        // Says "right", but _horizontalInput flips the direction left if player presses Left
        transform.position += Vector3.right * (_horizontalInput * Time.deltaTime * baseSpeed);
    }

    private void _ApplyGravity()
    {
        // Gravity is applied to the player's RigidBody2D, falling no faster than the maxFallSpeed
        if (currentState == STATE.Falling)
        {
            _playerBody.gravityScale = gravity * fallSpeedMultiplier;  // Fall increasingly faster
            _playerBody.velocity = new Vector2(_playerBody.velocity.x, Mathf.Max(_playerBody.velocity.y, -maxFallSpeed));
        }
    }

    public void Jump(float jumpVelocity)
    {
        // Gives coyote time on the first jump
        if (jumpsRemaining > 1 && coyoteTimeCounter > 0f)
        {
            _playerBody.velocity = new Vector2(_playerBody.velocity.x, jumpVelocity);
            jumpsRemaining--;

            coyoteTimeCounter = 0f;
        }

        // Double jumps don't need coyote time. Player can double jump normally
        else if (jumpsRemaining > 0)
        {
            _playerBody.velocity = new Vector2(_playerBody.velocity.x, jumpVelocity);
            jumpsRemaining--;
        }
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
                coyoteTimeCounter = coyoteTime;

            }
        }
        else
        {
            currentState = STATE.Falling;
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draws a bounding box for the ground detection collider
        // You can turn this off by deselcting Gizmos in the Scene view
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }
}
