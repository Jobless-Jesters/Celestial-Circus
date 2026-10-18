using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TEMP_curtains : MonoBehaviour
{
    
    //Sub-object references
    [SerializeField] private GameObject top;
    [SerializeField] private GameObject left;
    [SerializeField] private GameObject right;
    [SerializeField] private GameObject topLeft;
    [SerializeField] private GameObject topRight;
    [SerializeField] private Animator animator;

    // state tracking
    [SerializeField] private bool isOpen = false;
    

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setIsOpen(bool open)
    {
        isOpen = open;
    }

    public bool getIsOpen()
    {
        return isOpen;
    }

    public void openCurtains()
    {
        setIsOpen(true);
        animator.SetBool("isOpen", true);
    }

    public void closeCurtains()
    {
        setIsOpen(false);
        animator.SetBool("isOpen", false);
    }
}
