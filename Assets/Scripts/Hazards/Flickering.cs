using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flickering : MonoBehaviour
{
    [Header("Light Settings")]
    public Light2D coreLight;
    public ParticleSystem sparks;
    public float minIntensity = 0.4f;
    public float maxIntensity = 0.6f;

    [Header("Speed Settings")]
    public float flickerInterval = 0.1f;
    public float sparkInterval;
    public float minSparkInterval = 5f;
    public float maxSparkInterval = 10f;

    private float flickerTimer;
    private float sparksTimer;

    void Start()
    {
        // To prevent all hazards from sparking simultaneously, set a random timer
        sparkInterval = Random.Range(minSparkInterval, maxSparkInterval);
    }

    void Update()
    {
        // Time until next flicker
        flickerTimer += Time.deltaTime;
        sparksTimer += Time.deltaTime;

        if (flickerTimer >= flickerInterval)
        {
            // Assign random intensity value to CoreLighting light child
            coreLight.intensity = Random.Range(minIntensity, maxIntensity);
            flickerTimer = 0f;
        }

        if (sparksTimer >= sparkInterval)
        {
            sparks.Play();
            sparksTimer = 0f;
            sparkInterval = Random.Range(minSparkInterval, maxSparkInterval);
        }
    }
}
