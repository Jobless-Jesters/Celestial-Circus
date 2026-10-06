using UnityEngine;

public class MovingHazard : Hazard
{
    public enum MovementType {
        linear,
        arc
    }

    [Header("Movement Parameters")]
    [SerializeField] public MovementType movementType;
    [SerializeField] public Vector3 startPoint;
    [SerializeField] public Vector3 endPoint;
    [SerializeField] public float speed = 1.0f;
    [SerializeField] public int maxAngle = 180; // in degrees, set negative for bottom semicircle motion

    Vector3 targetPos;

    private float arcProgress = 0; // tracking position in arc
    private int arcDirection = 1; // tracks direction of swing

    private void Start()
    {
        targetPos = endPoint;
    }

    private void Update()
    {
        switch (movementType)
        {
            case MovementType.linear:
                LinearMovement();
                break;
            case MovementType.arc:
                ArcMovement();
                break;
        }
    }

    void LinearMovement()
    {
        if (Vector3.Distance(transform.position, startPoint) < 0.05f)
        {
            targetPos = endPoint;
        } else if (Vector3.Distance(transform.position, endPoint) < 0.05f)
        {
            targetPos = startPoint;
        }
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    // Calculates next point in an arc using polar coordinates (assumes perfectly circular motion)
    void ArcMovement()
    {
        Vector3 centerPoint = (startPoint + endPoint) / 2f;
        float radius = Vector3.Distance(startPoint, centerPoint);

        arcProgress += arcDirection * speed * Time.deltaTime;

        if (arcProgress >= 1f)  
        {
            arcProgress = 1;
            arcDirection = -1;
        } else if (arcProgress <= 0f)
        {
            arcProgress = 0;
            arcDirection = 1;
        }

        float theta = Mathf.Lerp(0, maxAngle * Mathf.PI / 180f, arcProgress);

        float x = centerPoint.x + radius * Mathf.Cos(theta);
        float y = centerPoint.y + radius * Mathf.Sin(theta);

        Vector3 ds = new Vector3(x, y, 0);

        transform.position = ds;        
    }
}
