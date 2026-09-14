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
    [SerializeField] protected int jumpsRemaining;

    [Header("Kinematics")]
    [SerializeField] protected float baseSpeed = 5f;
    [SerializeField] protected float horizontalAcceleration = 10;
    [SerializeField] protected float maxHorizontalVelocity = 20;
    [SerializeField] protected float maxVerticalVelocity = 20;

    [Header("Physics")]
    [SerializeField] protected float gravity = 6f;
    [SerializeField] protected float mass = 1;
    [SerializeField] protected float groundBuffer = 0.08f; // for ground detection
    private float GROUND_CHECK_DEPTH = 0.7f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    // Player States
    public enum STATE { Grounded, Falling }
    private STATE _currentState = STATE.Falling;
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
                    OnGrounded_Hook();
                    break;
                // case STATE.Rising: OnRising_Hook();
                //     break;
                // case STATE.Hanging: OnHanging_Hook();
                //     break;
                case STATE.Falling:
                    OnFalling_Hook();
                    break;
            }
        }
    }
    //Jump hooks
    protected virtual void OnGrounded_Hook() { }
    protected virtual void OnRising_Hook() { }
    protected virtual void OnHanging_Hook() { }
    protected virtual void OnFalling_Hook() { }
    protected virtual void OnJump_Hook() { }

    // Internal Movement Variables
    private Vector2 _currentVelocity = Vector2.zero;
    protected float _horizontalInput = 0; // 0 is idle, -1 is left, 1 is right

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

        _CheckGrounded();
        _calculateHorizontalVelocity();
        _ApplyGravity();
    }

    protected virtual void FixedUpdate()
    {
        // Actually move in fixed update to avoid kinematic body glitches
        _TryMove();
    }

    private void _TryMove()
    {
        float dt = Time.fixedDeltaTime;
        Vector2 position = transform.position;
        Vector2 newPosition = position + _currentVelocity * dt;
        Vector2 bounds = _playerCollider.bounds.extents; // Player bounding box edge
        if (_currentVelocity.y <= 0) // Groundcheck still happens even if the vert v is 0 
        { // MAKE SURE PLAYER IS ON "IGNORE RAYCAST" LAYER IN UNITY IN TOP RIGHT
            Vector2 boundPosition = new Vector2(position.x, position.y - bounds.y);
            RaycastHit2D hit = Physics2D.Raycast(boundPosition, Vector2.down, Mathf.Infinity, groundLayer); // Shoot invisible line straight down

            if (hit.collider) // If line hits something
            {
                float distance = hit.distance;
                if (distance <= groundBuffer && distance > 0) // Distance is close enough to ground
                {
                    currentState = STATE.Grounded;
                    _currentVelocity.y = 0;
                    newPosition = new Vector2(newPosition.x, position.y);
                }
                else if (distance < Mathf.Abs(_currentVelocity.y * dt) + groundBuffer && distance > 0)

                { // Still falling
                    newPosition = new Vector2(
                        newPosition.x, position.y - distance + groundBuffer
                    );
                }
            }
        }

        _playerBody.MovePosition(newPosition);
    }

    // v_final = v_initial + direction * (acceleration * time)
    private void _calculateHorizontalVelocity()
    {
        _currentVelocity += new Vector2(_horizontalInput, 0) * (horizontalAcceleration * Time.deltaTime);
    }

    // same formula as _HorizontalMove but down
    private void _ApplyGravity()
    {
        if (currentState == STATE.Falling)
        {
            _currentVelocity += new Vector2(0, -1) * (gravity * mass * Time.deltaTime);
        }
    }

    public void Jump(float jumpVelocity, int extraJumps)
    {
        if (currentState == STATE.Grounded)
        {
            jumpsRemaining = extraJumps;  // reset jumps when we touch the ground
            _currentVelocity.y = jumpVelocity;
            currentState = STATE.Falling;
            OnJump_Hook();
        }
        // Double jump
        else if (currentState == STATE.Falling && jumpsRemaining > 0)
        {
            jumpsRemaining--;
            _currentVelocity.y = jumpVelocity;
            OnJump_Hook();
        }
    }

    private void _CheckGrounded()
    {
        // Draws a line of GROUND_CHECK_DEPTH length extending from the player down to the ground 
        // If the line isn't colliding with anything, we must be not be in contact with the ground
        // ENSURE ANY PLATFORMS IN THE LEVEL ARE ON THE LAYER "3: GROUND" or the player will fall through the floor!
        Vector2 position = transform.position;
        Vector2 extents = _playerCollider.bounds.extents;
        Vector2 rayPosition = new Vector2(position.x, position.y - extents.y);

        if (currentState == STATE.Grounded)
        {
            Debug.DrawRay(rayPosition, Vector2.down * (GROUND_CHECK_DEPTH));
            RaycastHit2D checkValid = Physics2D.Raycast(rayPosition, Vector2.down, GROUND_CHECK_DEPTH, groundLayer);
            if (!checkValid.collider)
            {
                currentState = STATE.Falling;
            }
        }
    }
}
