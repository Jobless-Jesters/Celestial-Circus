using System.Collections;
using UnityEngine;

public class TwoWayPlatform : MonoBehaviour
{
    [Header("Drop Through")]
    [SerializeField] private KeyCode dropKey = KeyCode.S;
    [SerializeField] private float inputBufferTime = 0.15f;
    [SerializeField] private float dropSpeed = 2f;
    [SerializeField] private float reenableMargin = 0.1f;

    private BoxCollider2D platformCollider;

    private float dropInputTimer;
    private bool droppingThrough;

    private void Awake()
    {
        platformCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(dropKey))
        {
            dropInputTimer = inputBufferTime;
        }
        else
        {
            dropInputTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (droppingThrough)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (dropInputTimer > 0f)
        {
            dropInputTimer = 0f;

            StartCoroutine(
                DropThrough(collision.gameObject)
            );
        }
    }

    private IEnumerator DropThrough(GameObject player)
    {
        droppingThrough = true;

        Rigidbody2D playerRb =
            player.GetComponentInParent<Rigidbody2D>();

        Collider2D[] playerColliders =
            player.GetComponentsInParent<Collider2D>();

        // Ignore this platform for ALL player colliders.
        foreach (Collider2D playerCollider in playerColliders)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                platformCollider,
                true
            );
        }

        // Give the player a small guaranteed downward movement.
        if (playerRb != null)
        {
            Vector2 velocity = playerRb.velocity;

            velocity.y = Mathf.Min(
                velocity.y,
                -dropSpeed
            );

            playerRb.velocity = velocity;
        }

        // Wait until the entire player is underneath the platform.
        bool playerBelowPlatform = false;

        while (!playerBelowPlatform)
        {
            playerBelowPlatform = true;

            float platformBottom =
                platformCollider.bounds.min.y;

            foreach (Collider2D playerCollider in playerColliders)
            {
                if (playerCollider == null)
                    continue;

                float playerTop =
                    playerCollider.bounds.max.y;

                if (playerTop >
                    platformBottom - reenableMargin)
                {
                    playerBelowPlatform = false;
                    break;
                }
            }

            yield return null;
        }

        // Safe to restore collisions now.
        foreach (Collider2D playerCollider in playerColliders)
        {
            if (playerCollider == null)
                continue;

            Physics2D.IgnoreCollision(
                playerCollider,
                platformCollider,
                false
            );
        }

        droppingThrough = false;
    }
}