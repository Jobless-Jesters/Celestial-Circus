using UnityEngine;

/// <summary>
/// Forwards player enter/exit events from the rope's trigger collider
/// to the TightropeController.
/// </summary>
public class TightropeTrigger : MonoBehaviour
{
    [SerializeField] private TightropeController tightrope;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tightrope.PlayerEntered(other.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tightrope.PlayerExited(other.transform);
        }
    }
}
