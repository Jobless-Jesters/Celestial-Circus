using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    public KeyCode jumpkey = KeyCode.Space;

    public float jumpPower = 30f;
    public int extraJumps = 2; // adds double jump
    private Movement movement;
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<Movement>();
    }

    // Update is called once per frame
    void Update()
    {            
        if (Input.GetKeyDown(jumpkey))
        {
            movement.Jump(jumpPower, extraJumps);
        }
    }
}
