using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System;

namespace Narrative
{
    /// <summary>
    /// A script that will activate a dialogue when triggered.
    /// Must be triggered manually but it will attempt to automatically connect to a ClickableObject script.
    /// </summary>
    public class DialogueEndTrigger : MonoBehaviour
    {
        public bool repeatable = false;//whether or not this dialogue can repeat

        [Header("Conditions")]
        [SerializeField] private List<Condition> conditions = new List<Condition>();

        [Header("Set Dialogue Flag After Finishing Dialogue")]
        [Tooltip("Which flag to assign after finishing this dialogue.")]
        [SerializeField]
        private string writeToFlagId = "";
        [SerializeField] private bool writeToFlagValue = false;

        [Header("Object")]
        public UnityEvent onFinish;

        public void Update()
        {
            Trigger();
        }


        /// <summary>
        /// Call this to activate the dialogue. If condition are set they must all be satisfied.
        /// </summary>
        public void Trigger()
        {
            
            if (DialogueSystem.IsPlaying())
            {
                return; //Don't activate if already playing something
            }


            //Check conditions
            if (!AreConditionsTrue())
            {
                return; //Cancel activation if any conditions fail
            }

            //callFinish();
            DialogueSystem.OnDialogueEnd.AddListener(OnDialogueEnd);
            onFinish.Invoke();
        }

        /// <summary>
        /// Evaluates whether all conditions are satisfied.
        /// </summary>
        private bool AreConditionsTrue()
        {
            foreach (Condition condition in conditions)
            {
                /* if (DialogueFlags.GetFlagValue(condition.flagID) != condition.expectedValue)
                {
                    return false;
                } */

                if (GameController.GetFlagValue(condition.flagID) != condition.expectedValue)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Callback reciever for when dialogue ends.
        /// Writes to flags if it is set
        /// </summary>
        private void OnDialogueEnd()
        {
            if (writeToFlagId != "")
            {
                //DialogueFlags.SetFlag(writeToFlagId, writeToFlagValue);
                GameController.SetFlag(writeToFlagId, writeToFlagValue);
            }
            DialogueSystem.OnDialogueEnd.RemoveListener(OnDialogueEnd);//We shouldn't recieve this if we aren't playing something.
            if(!repeatable){
                Destroy(this);
            }
        }
    }
}