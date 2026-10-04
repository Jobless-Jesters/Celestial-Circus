using UnityEngine;

/// <summary>
/// Draws a springy tightrope and launches the player upward when they land on it.
/// The rope is a LineRenderer whose points sag toward the player's position.
/// </summary>
public class TightropeController : MonoBehaviour
{
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

    [Header("Launch")]
    // Upward velocity given to the player (a normal jump is ~40)
    [SerializeField] private float launchForce = 8f;

    // How far the rope must stretch before it counts as loaded
    [SerializeField] private float launchCompression = 0.28f;

    // Small pause at maximum stretch before release
    [SerializeField] private float loadedHoldTime = 0.07f;

    // Upward speed given to the rope itself when it releases
    [SerializeField] private float reboundKick = 7f;

    // Player currently touching the rope
    private Transform currentPlayer;
    private Rigidbody2D playerRb;
    private Movement playerMovement;

    private bool playerOnRope;

    // The rope only launches when armed. It disarms after a launch and
    // re-arms once the player has moved off the rope's ends.
    private bool launchArmed = true;
    private bool waitingForLaunch;
    private bool ropeLoaded;
    private Transform lastLaunchedPlayer;

    // Player position along the rope: 0 = start, 1 = end
    private float playerT = 0.5f;

    // Current downward deformation of the rope under the player
    private float displacement;

    // Current speed of the deformation
    private float springVelocity;

    // How long the rope has been held at maximum tension
    private float loadedTimer;

    private void Awake()
    {
        if (ropeRenderer != null)
        {
            ropeRenderer.useWorldSpace = true;
            ropeRenderer.positionCount = segments + 1;
        }
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null || ropeRenderer == null)
            return;

        UpdatePlayerPosition();
        UpdateSpring();
        CheckForLaunch();
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

    // Called by TightropeTrigger when the player touches the rope
    public void PlayerEntered(Transform player)
    {
        currentPlayer = player;
        playerOnRope = true;

        playerRb = player.GetComponentInParent<Rigidbody2D>();
        playerMovement = player.GetComponentInParent<Movement>();

        if (playerRb == null || playerMovement == null)
            return;

        if (!launchArmed)
            return;

        // Only load the rope when the player lands on it, not when rising through it
        if (playerRb.velocity.y > 0f)
            return;

        // Start a fresh loading cycle
        waitingForLaunch = true;
        ropeLoaded = false;
        loadedTimer = 0f;

        // Harder landings kick the rope down harder
        float fallSpeed = Mathf.Abs(playerRb.velocity.y);
        float impactMultiplier = Mathf.Clamp(fallSpeed / 10f, 1f, 2.5f);

        springVelocity += landingImpulse * impactMultiplier;
    }

    // Called by TightropeTrigger when the player leaves the rope
    public void PlayerExited(Transform player)
    {
        if (currentPlayer != player)
            return;

        playerOnRope = false;
        currentPlayer = null;
    }

    // Launches the player once the rope has stretched and held for a moment
    private void CheckForLaunch()
    {
        if (!waitingForLaunch || playerRb == null)
            return;

        if (!ropeLoaded)
        {
            // Loaded once stretched far enough and no longer moving down quickly
            if (displacement >= launchCompression && springVelocity <= 0.5f)
            {
                ropeLoaded = true;
                loadedTimer = 0f;
            }
            return;
        }

        // Hold the rope at maximum tension briefly before release
        loadedTimer += Time.deltaTime;

        if (loadedTimer < loadedHoldTime)
            return;

        waitingForLaunch = false;
        ropeLoaded = false;
        launchArmed = false;

        // Rope snaps upward
        springVelocity = -reboundKick;

        // Remember the player so the rope can re-arm once they leave
        lastLaunchedPlayer = playerRb.transform;

        // Launch through the player's movement controller so it owns the physics
        playerMovement.LaunchFromTightrope(launchForce);

        playerRb = null;
        playerMovement = null;
    }

    // Re-arms the rope once the launched player has moved past either end
    private void CheckForRearm()
    {
        if (launchArmed || lastLaunchedPlayer == null)
            return;

        float playerX = lastLaunchedPlayer.position.x;
        float leftEdge = Mathf.Min(startPoint.position.x, endPoint.position.x);
        float rightEdge = Mathf.Max(startPoint.position.x, endPoint.position.x);

        if (playerX < leftEdge || playerX > rightEdge)
        {
            launchArmed = true;
            lastLaunchedPlayer = null;

            // Reset the previous launch cycle
            waitingForLaunch = false;
            ropeLoaded = false;
            loadedTimer = 0f;

            // Start the next landing from an unloaded rope
            displacement = 0f;
            springVelocity = 0f;
        }
    }
}
