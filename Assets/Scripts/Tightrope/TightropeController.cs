using UnityEngine;

/// <summary>
/// Draws a springy tightrope with a Celeste-style timed jump.
/// Landing stretches the rope down. When it starts snapping back a short jump window
/// opens: pressing jump in time gives a strong boost, missing it gives a weak bounce.
/// One rebound per landing. Movement still owns the player's velocity.
/// </summary>
public class TightropeController : MonoBehaviour
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

    [Header("Timing")]
    // How long the player has to press jump once the rope starts rebounding
    [SerializeField] private float jumpWindow = 0.12f;

    // Presses this long before the window opens still count
    [SerializeField] private float jumpBuffer = 0.08f;

    [Header("Launch")]
    // Upward velocity for a well timed jump (a normal jump is ~40)
    [SerializeField] private float boostVelocity = 50f;

    // Upward velocity when the window is missed
    [SerializeField] private float missBounceVelocity = 22f;

    // Extra upward snap given to the rope itself on release
    [SerializeField] private float reboundKick = 7f;

    // How far past either end the player must travel before the rope re-arms
    [SerializeField] private float rearmMargin = 0.5f;

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

    private void Awake()
    {
        if (ropeRenderer != null)
        {
            ropeRenderer.useWorldSpace = true;
            ropeRenderer.positionCount = segments + 1;
        }
    }

    private void OnDisable()
    {
        ResetRope();
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null || ropeRenderer == null)
            return;

        UpdatePlayerPosition();
        UpdateSpring();
        UpdateTiming();
        CheckForRearm();
        DrawRope();
    }

    // Works out where the player is along the rope (playerT)
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
        Vector2 playerPosition = currentPlayer.position;
        playerT = Mathf.Clamp01(
            Vector2.Dot(playerPosition - start, ropeDirection) / ropeLengthSquared
        );
    }

    // Spring toward the sagged shape while the player is on the rope, or flat otherwise
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
            ropeRenderer.SetPosition(i, position);
        }
    }

    // Runs the Loading -> JumpWindow -> Released timing
    private void UpdateTiming()
    {
        bufferedJumpTimer -= Time.deltaTime;

        switch (state)
        {
            case RopeState.Loading:
                // The rope has bottomed out and started rising, so open the window
                if (springVelocity <= 0f)
                {
                    state = RopeState.JumpWindow;
                    windowTimer = jumpWindow;

                    // An early press that is still buffered counts immediately
                    if (bufferedJumpTimer > 0f)
                    {
                        Launch(true);
                    }
                }
                break;

            case RopeState.JumpWindow:
                windowTimer -= Time.deltaTime;

                // Window closed without a press, so the player gets the weak bounce
                if (windowTimer <= 0f)
                {
                    Launch(false);
                }
                break;
        }
    }

    // Called by TightropeTrigger when the player touches the rope
    public void PlayerEntered(Transform player)
    {
        currentPlayer = player;
        playerOnRope = true;

        playerRb = player.GetComponentInParent<Rigidbody2D>();
        playerMovement = player.GetComponentInParent<Movement>();

        if (playerRb == null || playerMovement == null)
            return;

        // Only a landing starts the mechanic, not passing up through the rope.
        // Idle is the only state that accepts one, so overlapping triggers or a
        // re-entry during the launch cannot fire it twice
        if (state != RopeState.Idle || playerRb.velocity.y > 0f)
            return;

        BeginLoading(Mathf.Abs(playerRb.velocity.y));
    }

    // Called by TightropeTrigger when the player leaves the rope
    public void PlayerExited(Transform player)
    {
        if (currentPlayer != player)
            return;

        playerOnRope = false;
        currentPlayer = null;

        // A launch carries the player straight up out of the trigger, and they may
        // well fall back onto the rope. Released is kept so that does not count as a
        // new landing, and CheckForRearm decides when the visit is really over.
        // Any other state means they left mid-bounce, so the rope re-arms now
        if (state != RopeState.Released)
        {
            ResetRope();
        }
    }

    // Re-arms the rope once the player has moved horizontally past either end.
    // Without this a weak bounce would drop them back in and auto-bounce forever
    private void CheckForRearm()
    {
        if (state != RopeState.Released || launchedPlayer == null)
            return;

        float playerX = launchedPlayer.position.x;
        float leftEdge = Mathf.Min(startPoint.position.x, endPoint.position.x) - rearmMargin;
        float rightEdge = Mathf.Max(startPoint.position.x, endPoint.position.x) + rearmMargin;

        if (playerX < leftEdge || playerX > rightEdge)
        {
            ResetRope();
        }
    }

    // Starts the stretch after a landing at the given speed
    private void BeginLoading(float impactSpeed)
    {
        // Harder landings stretch the rope further. Always starts moving down
        float impactMultiplier = Mathf.Clamp(impactSpeed / 10f, 1f, 2.5f);
        springVelocity = Mathf.Max(springVelocity, 0f) + landingImpulse * impactMultiplier;

        state = RopeState.Loading;
        bufferedJumpTimer = 0f;

        // Take over jump presses for the length of this bounce
        playerMovement.JumpInterceptor = TryBoost;
    }

    // Called by Movement when jump is pressed. Returns true if the rope used the
    // press, in which case Movement skips the normal jump
    private bool TryBoost()
    {
        switch (state)
        {
            case RopeState.Loading:
                // Too early for the window, so hold the press for a moment
                bufferedJumpTimer = jumpBuffer;
                return true;

            case RopeState.JumpWindow:
                Launch(true);
                return true;

            default:
                // Released or Idle, so jumping behaves normally again
                return false;
        }
    }

    // Sends the player up through Movement and ends this bounce
    private void Launch(bool boosted)
    {
        // Visible snap upward, stronger on a boost
        springVelocity = -reboundKick * (boosted ? 1f : 0.5f);

        // Watched by CheckForRearm until they move off the rope
        launchedPlayer = playerMovement.transform;

        playerMovement.LaunchFromTightrope(
            boosted ? boostVelocity : missBounceVelocity,
            boosted
        );

        // Stop claiming jump presses so the player can double jump out of the launch
        playerMovement.JumpInterceptor -= TryBoost;

        state = RopeState.Released;
        bufferedJumpTimer = 0f;
    }

    // Returns the rope to Idle and drops any state tied to the current player
    private void ResetRope()
    {
        if (playerMovement != null)
        {
            playerMovement.JumpInterceptor -= TryBoost;
        }

        playerOnRope = false;
        currentPlayer = null;
        playerRb = null;
        playerMovement = null;
        launchedPlayer = null;

        state = RopeState.Idle;
        windowTimer = 0f;
        bufferedJumpTimer = 0f;
    }
}
