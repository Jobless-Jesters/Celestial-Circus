using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



public class Tightrope2 : MonoBehaviour
{
    // Idle:       waiting for the player to land
    // Loading:    rope is stretching down after a landing
    // JumpWindow: rope is snapping up; a jump press now gives the boost
    // Released:   player has been launched, no more input counts until they land again
    private enum RopeState { Idle, MinBoost, MedBoost, MaxBoost }

    [Header("References")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private EdgeCollider2D ropeCollider;
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private Gradient notReadyGradient;
    [SerializeField] private Gradient minBoostGradient;
    [SerializeField] private Gradient medBoostGradient;
    [SerializeField] private Gradient maxBoostGradient;


    [Header("Rope Shape")]
    // Number of line segments used to draw the rope
    [SerializeField] private int segments = 24;

    // Small amount of sag when nobody is standing on the rope
    [SerializeField] private float idleSag = 0.15f;

    // How far the rope bends when the player stands on it
    [SerializeField] private float playerSag = 0.75f;

    // How wide the player's influence is along the rope (0-1 of rope length)
    [SerializeField] private float playerInfluenceWidth = 0.25f;

    [Header("Spring")]
    // How quickly the rope tries to return to its target shape
    [SerializeField] private float springStrength = 50f;

    // How quickly the rope loses its bouncing motion
    [SerializeField] private float damping = 4f;

    // Initial downward kick when the player lands
    [SerializeField] private float landingImpulse = 2.5f;

    [Header("Launch")]
    // Upward velocity for a well timed jump (a normal jump is ~40)
    [SerializeField] private float minBoostVelocity = 50f;
    [SerializeField] private float medBoostVelocity = 80f;
    [SerializeField] private float maxBoostVelocity = 110f;


    // Extra upward snap given to the rope itself on release
    [SerializeField] private float reboundKick = 7f;

    // How far past either end the player must travel before the rope re-arms
    [SerializeField] private float rearmMarginHorizontal = 0.5f;

    // How far past the top of the rope the player must travel before the rope rearms
    [SerializeField] private float rearmMarginVertical = 3f;

    private RopeState state = RopeState.Idle;

    // Player currently touching the rope
    private GameObject currentPlayer;
    private Transform currentPlayerTransform;
    private Rigidbody2D playerRb;
    private Movement playerMovement;
    private bool playerOnRope;

    // Kept after a launch, when currentPlayer is cleared, so the rope can watch
    // for the player moving off either end
    private Transform launchedPlayer;

    // Player position along the rope: 0 = start, 1 = end
    private float playerT = 0.5f;

    // Current downward deformation of the rope under the player
    private float displacement;

    // Current speed of the deformation (positive = moving down)
    private float springVelocity;

    // Counts down while the jump window is open
    private float windowTimer;

    // Counts down after an early press, so it can still land in the window
    private float bufferedJumpTimer;
    private Vector2[] colliderPoints;


    private InputAction _moveAction;
    [SerializeField] public float tightropeTimer = 0f;
    [SerializeField] public float timeBetweenPhases = 1f;


    // Start is called before the first frame update
    void Start()
    {
        if (ropeRenderer != null)
        {
            ropeRenderer.useWorldSpace = true;
            ropeRenderer.positionCount = segments + 1;
        }
        segments = Mathf.Max(1, segments);

        if (ropeCollider == null)
        {
            ropeCollider = GetComponent<EdgeCollider2D>();
        }

        if (ropeCollider != null)
        {
            colliderPoints = new Vector2[segments + 1];
        }
        else
        {
            Debug.LogError("Tightrope2 requires an EdgeCollider2D reference.", this);
        }
        _moveAction = FindObjectOfType<PlayerInput>().actions.FindAction("Move", true);
        currentPlayer = GameObject.FindGameObjectWithTag("Player");
        currentPlayerTransform = currentPlayer.transform;
        playerRb = currentPlayer.GetComponent<Rigidbody2D>();
        playerMovement = currentPlayer.GetComponentInParent<Movement>();

        if (ropeRenderer != null)
        {
            ropeRenderer.useWorldSpace = true;
            ropeRenderer.positionCount = segments + 1;
        }

        ropeRenderer.colorGradient = notReadyGradient;
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerPosition();
        UpdateSpring();
        CheckRopeState();
        DrawRope();
    }

    private void CheckRopeState()
    {
        // Changes state of rope depending on timer
        if (tightropeTimer > 0f && tightropeTimer < timeBetweenPhases)
        {
            // short boost
            state = RopeState.MinBoost;
            ropeRenderer.colorGradient = minBoostGradient;
        }
        else if (tightropeTimer > timeBetweenPhases && tightropeTimer < timeBetweenPhases * 2)
        {
            //medium boost
            state = RopeState.MedBoost;
            ropeRenderer.colorGradient = medBoostGradient;

        }
        else if (tightropeTimer > timeBetweenPhases * 2)
        {
            // max boost
            state = RopeState.MaxBoost;
            ropeRenderer.colorGradient = maxBoostGradient;

        }
    }

    private void UpdatePlayerPosition()
    {
        if (!playerOnRope || currentPlayer == null)
            return;

        Vector2 start = startPoint.position;
        Vector2 ropeDirection = (Vector2)endPoint.position - start;
        float ropeLengthSquared = ropeDirection.sqrMagnitude;

        if (ropeLengthSquared <= 0.001f)
            return;

        // Project the player onto the rope line
        Vector2 playerPosition = currentPlayerTransform.position;
        playerT = Mathf.Clamp01(
            Vector2.Dot(playerPosition - start, ropeDirection) / ropeLengthSquared
        );
    }
    private void UpdateSpring()
    {
        float targetDisplacement = playerOnRope ? playerSag : 0f;
        float acceleration = (targetDisplacement - displacement) * springStrength;

        springVelocity += acceleration * Time.deltaTime;

        // Exponential damping behaves consistently at different frame rates
        springVelocity *= Mathf.Exp(-damping * Time.deltaTime);

        displacement += springVelocity * Time.deltaTime;
    }
    private void DrawRope()
    {
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 position = Vector3.Lerp(startPoint.position, endPoint.position, t);

            // Parabolic idle sag: 0 at both anchors, maximum in the middle
            float downwardAmount = idleSag * 4f * t * (1f - t);

            if (playerOnRope)
            {
                // Gaussian falloff so the rope bends most under the player
                float distanceFromPlayer = t - playerT;
                float influence = Mathf.Exp(
                    -(distanceFromPlayer * distanceFromPlayer)
                    / (playerInfluenceWidth * playerInfluenceWidth)
                );

                downwardAmount += displacement * influence;
            }

            position.y -= downwardAmount;
            if (ropeRenderer != null)
            {
                ropeRenderer.SetPosition(i, position);
            }

            if (ropeCollider != null)
            {
                colliderPoints[i] = ropeCollider.transform.InverseTransformPoint(position);
            }
        }
        if (ropeCollider != null)
        {
            ropeCollider.points = colliderPoints;
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnRope = true;
            if (_moveAction.ReadValue<Vector2>().y < 0f)
            {
                ropeRenderer.colorGradient = minBoostGradient;
                tightropeTimer += Time.deltaTime;
                Debug.Log("ready");
            }
            else
            {
                ropeRenderer.colorGradient = notReadyGradient;
                Boost();
            }
        }
    }

    private void Boost()
    {
        // Launches player with varying velocity depending on state
        if (state != RopeState.Idle)
        {
            springVelocity = -reboundKick;
            if (state == RopeState.MinBoost)
            {
                // short boost
                playerMovement.LaunchFromTightrope(minBoostVelocity);
            }
            else if (state == RopeState.MedBoost)
            {
                //medium boost
                playerMovement.LaunchFromTightrope(medBoostVelocity);
            }
            else if (state == RopeState.MaxBoost)
            {
                // max boost
                playerMovement.LaunchFromTightrope(maxBoostVelocity);
            }

            ResetRope();
        }
    }

    private void ResetRope()
    {
        playerOnRope = false;
        state = RopeState.Idle;
        tightropeTimer = 0f;
        ropeRenderer.colorGradient = notReadyGradient;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnRope = false;
        }
    }

}
