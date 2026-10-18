using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Narrative
{
    /// <summary>
    /// A component that recieves clicks and broadcasts a callback.
    /// Can set reciever methods in the editor.
    /// </summary>
    /* [RequireComponent(typeof(Collider2D))] */
    public class ClickableObject : MonoBehaviour
    {
        //Event Callbacks
        public UnityEvent onClick;
        private bool wasClicked = false;

        /// <summary>
        /// Called every frame when object is set as active 
        /// </summary>
        void Update()
        {
            // if statement for
            // the object has yet to be clicked on already
            // and the input is either space or enter 
            // change wasClicked and do what was assigned
            if (!wasClicked && Input.GetButtonDown("Submit"))
            {
                wasClicked = true; 
                onClick.Invoke();
            }
        }
    }
}