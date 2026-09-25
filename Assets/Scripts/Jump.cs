using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    public KeyCode jumpkey = KeyCode.Space;
    public float jumpPower = 30f;
    private Movement movement;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {
        // Press Space -> jump/double jump            
        if (Input.GetKeyDown(jumpkey))
        {
            movement.Jump(jumpPower);
        }

        // Holding Space tells Movement that the player wants to glide
        movement.SetGliding(Input.GetKey(jumpkey));
    }
}
