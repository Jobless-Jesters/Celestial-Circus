using UnityEngine;

public class BouncyPlatform : MonoBehaviour
{
    [Header("Bounce Parameters")]
    [SerializeField] private float bouncePower = 45f;
    [SerializeField] private float bounceDecay = 10f; // How fast to decay the bounce
    [SerializeField] private float bounceCooldown = 2f;

    private int bounceCount = 0;
    private float lastBounceTime = -Mathf.Infinity;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Movement movement = collision.rigidbody.GetComponent<Movement>();
        if (collision.gameObject.CompareTag("Player") && collision.GetContact(0).normal.y < -0.5f) // Check if player is falling downward onto platform
        {
            // Reset cooldown if time since last bounce is more than cooldown
            if (Time.time - lastBounceTime > bounceCooldown) 
            {
                bounceCount = 0;
            }

            float bounce = Mathf.Max(0, bouncePower - bounceDecay * bounceCount);
            if (bounce > 0f) {
                movement.Bounce(bounce);
                lastBounceTime = Time.time;
                bounceCount++;
            };
           
        }
    }
}
