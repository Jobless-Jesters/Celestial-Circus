using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;



public class Tightrope2 : MonoBehaviour
{
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
    [SerializeField] private float idleSag = 0.1f;

    // How far the rope bends when the player stands on it
    [SerializeField] private float playerSag = 0.2f;
    [SerializeField] public float minBoostRopeSag = 1.5f;
    [SerializeField] public float medBoostRopeSag = 2.5f;
    [SerializeField] public float maxBoostRopeSag = 3.5f;

    // How wide the player's influence is along the rope (0-1 of rope length)
    [SerializeField] private float playerInfluenceWidth = 1f;  // player influences whole rope

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
    private RopeState state = RopeState.Idle;
    // Player currently touching the rope
    private GameObject currentPlayer;
    private Transform currentPlayerTransform;
    private Rigidbody2D playerRb;
    private Movement playerMovement;
    private Vector2[] colliderPoints;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private bool playerOnRope;
    // Player position along the rope: 0 = start 1 = end
    private float playerT = 0.5f;
    // Current downward deformation of the rope under the player
    private float displacement;
    // Current speed of the deformation (positive = moving down)
    private float springVelocity;
    // Current time charging tightrope
    [SerializeField] public float tightropeTimer = 0f;
    // Time between tightrope phases
    [SerializeField] public float timeBetweenPhases = 1f;
    // How much the rope stretches each phase
    [Header("Visuals")]
    [SerializeField] public float minBoostFlashSpeed = 5f;
    [SerializeField] public float medBoostFlashSpeed = 10f;
    [SerializeField] public float maxBoostFlashSpeed = 20f;
    [SerializeField] public float currFlashSpeed = 0f;
    [SerializeField] public float minBoostChargeFxSpeed = 20f;
    [SerializeField] public float medBoostChargeFxSpeed = 40f;
    [SerializeField] public float maxBoostChargeFxSpeed = 60f;
    [SerializeField] public float currChargeFxSpeed = 0f;
    private const int FlashKeyCount = 8;
    private readonly GradientColorKey[] flashColorKeys = new GradientColorKey[FlashKeyCount];
    private readonly GradientAlphaKey[] flashAlphaKeys = new GradientAlphaKey[FlashKeyCount];
    private readonly Gradient flashGradient = new Gradient();
    private float flashOffset;



    // Start is called before the first frame update
    void Start()
    {
        if (ropeRenderer != null)
        {
            ropeRenderer.useWorldSpace = true;
            ropeRenderer.positionCount = segments + 1;
        }
        segments = Mathf.Max(1, segments);

        if (ropeCollider != null)
        {
            colliderPoints = new Vector2[segments + 1];
        }

        _moveAction = FindObjectOfType<PlayerInput>().actions.FindAction("Move", true);
        _jumpAction = FindObjectOfType<PlayerInput>().actions.FindAction("Jump", true);

        currentPlayer = GameObject.FindGameObjectWithTag("Player");
        currentPlayerTransform = currentPlayer.transform;
        playerRb = currentPlayer.GetComponent<Rigidbody2D>();
        playerMovement = currentPlayer.GetComponentInParent<Movement>();
        ropeRenderer.colorGradient = notReadyGradient;
    }

    void Update()
    {
        CheckRopeState();
        tightropeFlash();
        if (tightropeTimer > 0f && _moveAction.ReadValue<Vector2>().y >= 0f)
            Boost();
    }
    // Keep the deforming collider in sync with the physics simulation.
    void FixedUpdate()
    {
        UpdatePlayerPosition();
        float previousDisplacement = displacement;
        UpdateSpring();
        if (playerOnRope)
            playerRb.position += Vector2.down * ((displacement - previousDisplacement) * 4f * playerT * (1f - playerT));
        DrawRope();
    }

    private void CheckRopeState()
    {
        // Changes state of rope depending on timer
        if (tightropeTimer > 0f && tightropeTimer < timeBetweenPhases)
        {
            // short boost
            state = RopeState.MinBoost;
            playerSag = minBoostRopeSag;
            ropeRenderer.colorGradient = minBoostGradient;
            currFlashSpeed = minBoostFlashSpeed;
            playerMovement.playTightropeChargeFX(minBoostChargeFxSpeed);
        }
        else if (tightropeTimer > timeBetweenPhases && tightropeTimer < timeBetweenPhases * 2)
        {
            //medium boost
            state = RopeState.MedBoost;
            playerSag = medBoostRopeSag;
            ropeRenderer.colorGradient = medBoostGradient;
            currFlashSpeed = medBoostFlashSpeed;
            playerMovement.playTightropeChargeFX(medBoostChargeFxSpeed);
        }
        else if (tightropeTimer > timeBetweenPhases * 2)
        {
            // max boost
            state = RopeState.MaxBoost;
            playerSag = maxBoostRopeSag;
            ropeRenderer.colorGradient = maxBoostGradient;
            currFlashSpeed = maxBoostFlashSpeed;
            playerMovement.playTightropeChargeFX(maxBoostChargeFxSpeed);
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

        springVelocity += acceleration * Time.fixedDeltaTime;

        // Exponential damping behaves consistently at different frame rates
        springVelocity *= Mathf.Exp(-damping * Time.fixedDeltaTime);

        displacement += springVelocity * Time.fixedDeltaTime;
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

                downwardAmount += displacement * influence * 4f * t * (1f - t);
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
                //todo: Disable other inputs somewhere here
                _jumpAction.Disable();
                tightropeTimer += Time.fixedDeltaTime;
            }
        }
    }

    private void Boost()
    {
        // Launches player with varying velocity depending on state
        if (state != RopeState.Idle)
        {
            springVelocity = -reboundKick;
            StartCoroutine(ReleasePlayerFromRope());
            if (state == RopeState.MinBoost)
            {
                // short boost
                playerMovement.LaunchFromTightrope(minBoostVelocity, true);
            }
            else if (state == RopeState.MedBoost)
            {
                //medium boost
                playerMovement.LaunchFromTightrope(medBoostVelocity, true);
            }
            else if (state == RopeState.MaxBoost)
            {
                // max boost
                playerMovement.LaunchFromTightrope(maxBoostVelocity, true);
            }
            ResetRope();
        }
    }

    private IEnumerator ReleasePlayerFromRope()
    {
        // Temporarily disable the tightrope collider to prevent clown from getting stuck in the rope
        Collider2D[] colliders = currentPlayer.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D playerCollider in colliders)
            if (!playerCollider.isTrigger)
                Physics2D.IgnoreCollision(playerCollider, ropeCollider, true);

        yield return new WaitForSeconds(0.15f);

        foreach (Collider2D playerCollider in colliders)
            if (playerCollider != null && !playerCollider.isTrigger)
                Physics2D.IgnoreCollision(playerCollider, ropeCollider, false);
    }

    private void ResetRope()
    {
        playerOnRope = false;
        _jumpAction.Enable();
        state = RopeState.Idle;
        tightropeTimer = 0f;
        playerSag = 0.2f;
        ropeRenderer.colorGradient = notReadyGradient;
        currFlashSpeed = 0;
        flashOffset = 0f;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ResetRope();
        }
    }

    private void tightropeFlash()
    {
        if (state == RopeState.Idle)
            return;

        Gradient source = state == RopeState.MinBoost ? minBoostGradient :
            state == RopeState.MedBoost ? medBoostGradient : maxBoostGradient;
        float ropeLength = Vector2.Distance(startPoint.position, endPoint.position);
        if (ropeLength <= Mathf.Epsilon)
            return;

        flashOffset = Mathf.Repeat(flashOffset + currFlashSpeed * Time.deltaTime / ropeLength, 1f);

        for (int i = 0; i < FlashKeyCount; i++)
        {
            float time = i / (float)(FlashKeyCount - 1);
            Color color = source.Evaluate(Mathf.Repeat(time - flashOffset, 1f));
            flashColorKeys[i] = new GradientColorKey(color, time);
            flashAlphaKeys[i] = new GradientAlphaKey(color.a, time);
        }

        flashGradient.SetKeys(flashColorKeys, flashAlphaKeys);
        flashGradient.mode = source.mode;
        ropeRenderer.colorGradient = flashGradient;
    }
}
