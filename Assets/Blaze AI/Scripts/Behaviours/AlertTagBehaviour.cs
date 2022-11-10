using UnityEngine;

namespace BlazeAISpace 
{
    public class AlertTagBehaviour : MonoBehaviour
    {
        public bool checkLocation;

        [Tooltip("If check location is true, this animation will play when reaching the location. If check location is false, the animation will play instantly.")]
        public string finishedAnim;
        
        [Min(0)]
        public float finishedDuration = 2f;
        
        [Min(0), Tooltip("The transition time from current animation to either move/finished animations.")]
        public float animT = 0.25f;
        
        [Tooltip("Set your audios in the audio scriptable in the General Tab in Blaze AI.")]
        public bool playAudio;
        public int audioIndex;
        
        [Tooltip("Will call other agents to the location of the alert object.")]
        public bool callOtherAgents;
        [Min(0)]
        public float callRange = 10f;
        [Tooltip("Shows the call range as a white wire sphere in scene view.")]
        public bool showCallRange;
        public LayerMask otherAgentsLayers;

        
        BlazeAI blaze;
        AlertStateBehaviour alertStateBehaviour;
        
        bool audioPlayed;
        bool calledAgents;
        
        float _durationTimer;
    

        void Start()
        {
            blaze = GetComponent<BlazeAI>();
            alertStateBehaviour = GetComponent<AlertStateBehaviour>();
            

            // force shut if not the same state
            if (blaze.state != BlazeAI.State.sawAlertTag) {
                enabled = false;
            }
        }


        void OnDisable()
        {
            audioPlayed = false;
            calledAgents = false;
        }


        void Update()
        {
            // check if alert state behaviour isn't added
            if (alertStateBehaviour == null) {
                print("Alert State Behaviour must be added for Alert Tag behaviour to function. Please add the alert state behaviour.");
                return;
            }


            if (playAudio) {
                PlayAudio();
            }


            if (callOtherAgents) {
                CallAgents();
            }

            
            if (!checkLocation) {
                blaze.animManager.Play(finishedAnim, animT);
                DurationTimer();
                return;
            }

            
            if (blaze.MoveTo(blaze.sawAlertTagPos, alertStateBehaviour.moveSpeed, alertStateBehaviour.turnSpeed, alertStateBehaviour.moveAnim, alertStateBehaviour.animT)) {
                blaze.animManager.Play(finishedAnim, animT);
                DurationTimer();
            }
        }


        void OnDrawGizmosSelected()
        {
            if (!showCallRange || !callOtherAgents) {
                return;
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, callRange);
        }

        
        void PlayAudio()
        {
            if (!playAudio) {
                return;
            }

            if (audioPlayed) {
                return;
            }

            if (blaze.IsAudioScriptableEmpty()) {
                return;
            }

            if (blaze.PlayAudio(blaze.audioScriptable.GetAudio(AudioScriptable.AudioType.AlertTags, audioIndex))) {
                audioPlayed = true;
            }
        }


        // call agents to position
        void CallAgents()
        {
            if (calledAgents) {
                return;
            }


            Collider[] agentsColl = new Collider[20];
            int agentsNum = Physics.OverlapSphereNonAlloc(transform.position, callRange, agentsColl, otherAgentsLayers);
        
            for (int i=0; i<agentsNum; i++) {
                // if this same object then -> skip to next iteration
                if (agentsColl[i].transform.IsChildOf(transform)) {
                    continue;
                }


                BlazeAI blazeScript = agentsColl[i].transform.GetComponent<BlazeAI>();

                if (blazeScript == null) {
                    continue;
                }
                

                blazeScript.ChangeState("alert");
                blazeScript.MoveToLocation(blaze.sawAlertTagPos);
            }


            calledAgents = true;
        }


        void DurationTimer()
        {
            _durationTimer += Time.deltaTime;

            if (_durationTimer >= finishedDuration) {
                _durationTimer = 0;
                blaze.ChangeState("alert");
            }
        }
    }
}
