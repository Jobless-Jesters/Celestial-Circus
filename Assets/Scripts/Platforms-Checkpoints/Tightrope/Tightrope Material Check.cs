using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TightropeMaterialCheck : MonoBehaviour
{
    [SerializeField] private bool ropeMaterial = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // Set material for walkSFX
    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.attachedRigidbody == null ||
        !other.attachedRigidbody.CompareTag("Player"))
    {
        return;
    }

    AudioController.Instance.ropeMaterial = ropeMaterial;

    }

    private void OnTriggerExit2D(Collider2D other)
    {
    if (other.attachedRigidbody == null ||
        !other.attachedRigidbody.CompareTag("Player"))
        return;

    AudioController.Instance.ropeMaterial = false;
    }
}
