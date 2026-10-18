using UnityEngine;


public class Hazard : MonoBehaviour
{
    public enum MovementType {
        None,
        Linear,
        Arc
    }

    public enum ArcDirection
    {
        Clockwise,
        CounterClockwise
    }

    [Header("Movement Parameters")]
    [SerializeField] public MovementType movementType;
    [SerializeField, DraggablePoint] public Vector3 endPoint;
    [SerializeField] public float cycleTime = 1f; // How long it takes to comeplete a cycle (there and back)
    [SerializeField] public ArcDirection arcDirection = ArcDirection.Clockwise;
    
    private float progress;
    private int direction; // 1 means towards end, -1 means towards start
    private Vector3 startPoint;

    private void Awake()
    {
        progress = 0;
        direction = 1;
        startPoint = transform.position;
    }

    private void FixedUpdate()
    {
        if (movementType == MovementType.None) return;

        if (progress <= 0f)
        {
            progress = 0f;
            direction = 1;
        } else if (progress >= 1f)
        {
            progress = 1f;
            direction = -1;
        }

        progress += direction * Time.deltaTime / cycleTime * 2f; // Multiply by 2 to account for there and back

        transform.position = GetNextPathPoint(startPoint, progress);
    }

    private Vector3 GetNextPathPoint(Vector3 start, float progress)
    {
        if (movementType != MovementType.Arc)
        {
            return Vector3.Lerp(start, endPoint, progress);
        }
        float radius = Vector3.Distance(start, endPoint) / 2f;
        Vector3 centerPoint = (start + endPoint) / 2f;

        float startAngle = Mathf.Atan2(start.y - centerPoint.y, start.x - centerPoint.x); // Angle made between center point and start
        float sweepAngle = arcDirection == ArcDirection.Clockwise ? -Mathf.PI : Mathf.PI; // We assume perfect circular motion

        float theta = startAngle + progress * sweepAngle;

        float x = centerPoint.x + radius * Mathf.Cos(theta); // Basic trig
        float y = centerPoint.y + radius * Mathf.Sin(theta);
        return new Vector3(x, y, 0);
    }

    private void OnDrawGizmos()
    {
        if (movementType == MovementType.None) return;

        Gizmos.color = Color.yellow;
        Vector3 start = Application.isPlaying ? startPoint : transform.position;
        if (movementType == MovementType.Linear)
        {
            Gizmos.DrawLine(start, endPoint);
        } else if (movementType == MovementType.Arc)
        {
            int segments = 32;
            Vector3 cur = start;
            for (int i = 0; i <= segments; i++)
            {
                Vector3 next = GetNextPathPoint(start,  i / (float) segments);
                Gizmos.DrawLine(cur, next);
                cur = next;
            }
            Gizmos.DrawWireSphere(start, 0.15f);
            Gizmos.DrawWireSphere(endPoint, 0.15f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameController.Instance.respawnAtCheckpoint();
        }
    }
}
