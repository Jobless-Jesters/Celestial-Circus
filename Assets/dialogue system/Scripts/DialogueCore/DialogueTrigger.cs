using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Narrative
{
    /// <summary>
    /// A script that will activate a dialogue when triggered.
    /// Must be triggered manually but it will attempt to automatically connect to a ClickableObject script.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour
    {
        public bool repeatable = false;//whether or not this dialogue can repeat

        /// <summary> The csv file containing the dialogue to be played. </summary>
        [SerializeField] private TextAsset dialogueCSV;

        

        [Header("Conditions")]
        [SerializeField] private List<Condition> conditions = new List<Condition>();


        [Header("Set Dialogue Flag After Finishing Dialogue")]
        [Tooltip("Which flag to assign after finishing this dialogue.")]
        [SerializeField]
        private string writeToFlagId = "";
        [SerializeField] private bool writeToFlagValue = false;

        [Header("Event")]
        public UnityEvent onFinish;
        //[SerializeField] public GameObject host;


        /// <summary>
        /// Call this to activate the dialogue. If condition are set they must all be satisfied.
        /// </summary>
        public void Trigger()
        {
            Debug.LogWarning("DialogueTrigger Trigger() start");
            
            if (DialogueSystem.IsPlaying())
            {
                Debug.LogWarning("DialogueTrigger IsPlaying if statement inside");
                return;//Don't activate if already playing something
            }


            //Check conditions
            if (!AreConditionsTrue())
            {
                Debug.LogWarning("DialogueTrigger !AreConditionsTrue() if statement inside");
                return; //Cancel activation if any conditions fail
            }

            //Activate Dialogue
            DialogueSystem.OnDialogueEnd.AddListener(OnDialogueEnd);
            DialogueSystem.PlaySequence(dialogueCSV);
            Debug.LogWarning("end of trigger code");
        }

        /// <summary>
        /// Evaluates whether all conditions are satisfied.
        /// </summary>
        private bool AreConditionsTrue()
        {
            foreach (Condition condition in conditions)
            {
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
                GameController.SetFlag(writeToFlagId, writeToFlagValue);
            }
            DialogueSystem.OnDialogueEnd.RemoveListener(OnDialogueEnd);//We shouldn't recieve this if we aren't playing something.
            if(!repeatable){
                Destroy(this);
            }
            onFinish.Invoke();
            Debug.LogWarning("DialogueTrigger onDialogueEnd() run");
        }
    }
}