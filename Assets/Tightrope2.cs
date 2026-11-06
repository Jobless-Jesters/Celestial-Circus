using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



public class Tightrope2 : MonoBehaviour
{
    // Idle:       waiting for the player to land
    // Loading:    rope is stretching down after a landing
    // JumpWindow: rope is snapping up; a jump press now gives the boost
    // Released:   player has been launched, no more input counts until they land again
    private enum RopeState { Idle, Loading, JumpWindow, Released }

    [Header("References")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private Gradient notReadyGradient;
    [SerializeField] private Gradient readyGradient;


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
    [SerializeField] private float boostVelocity = 50f;

    // Upward velocity when the window is missed
    [SerializeField] private float missBounceVelocity = 22f;

    // Extra upward snap given to the rope itself on release
    [SerializeField] private float reboundKick = 7f;

    // How far past either end the player must travel before the rope re-arms
    [SerializeField] private float rearmMarginHorizontal = 0.5f;

    // How far past the top of the rope the player must travel before the rope rearms
    [SerializeField] private float rearmMarginVertical = 3f;

    private RopeState state = RopeState.Idle;

    // Player currently touching the rope
    private Transform currentPlayer;
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

    private InputAction _moveAction;

    // Start is called before the first frame update
    void Start()
    {
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
        DrawRope();
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
            ropeRenderer.SetPosition(i, position);
        }
    }


    public void OnTriggerStay()
    {
        if (_moveAction.ReadValue<Vector2>().y < 0f)
        {
            ropeRenderer.colorGradient = readyGradient;
            Debug.Log("ready");
        }
    }

    
}
