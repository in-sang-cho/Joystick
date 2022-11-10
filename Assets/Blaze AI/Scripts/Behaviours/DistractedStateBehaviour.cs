using UnityEngine;

namespace BlazeAISpace
{
    public class DistractedStateBehaviour : MonoBehaviour
    {
        [Tooltip("Time to pass (seconds) before turning to distraction location.")]
        public float timeToReact = 0.2f;
        [Tooltip("If enabled, will move to distraction location.")]
        public bool checkLocation = true;
        [Tooltip("Time to pass (seconds) before moving to check location.")]
        public float timeBeforeMovingToLocation = 1f;

        [Tooltip("Animation to play when reaches the distraction destination.")]
        public string checkAnim;
        public float checkAnimT = 0.25f;
        [Tooltip("Amount of time (seconds) to stay in distraction destination before going back to patrolling.")]
        public float timeToCheck = 5f;
        [Tooltip("Set the audios to play on check location in the audio scriptable in the General tab in Blaze AI.")]
        public bool playAudioOnCheckLocation;


        #region BEHAVIOUR VARS

        NormalStateBehaviour normalStateBehaviour;
        AlertStateBehaviour alertStateBehaviour;
        BlazeAI blaze;
        
        
        float _timeToCheck = 0f;
        float _timeToReact = 0f;
        float _timeBeforeMovingToLocation = 0f;
        float moveSpeed = 0;
        float turnSpeed = 0;


        bool turnedToLocation;
        bool playedLocationAudio;
        bool isIdle;

        
        string moveAnim = "";
        string leftTurn = "";
        string rightTurn = "";

        #endregion

        #region UNITY METHODS

        void Start()
        {
            blaze = GetComponent<BlazeAI>();
            normalStateBehaviour = GetComponent<NormalStateBehaviour>();
            alertStateBehaviour = GetComponent<AlertStateBehaviour>();

            if (normalStateBehaviour == null) {
                print("Distracted State Behaviour tried to get Normal State Behaviour component but found nothing. It's important to set it manually to get the movement and turning animations and speeds.");
            }

            if (alertStateBehaviour == null) {
                print("Distracted State Behaviour tried to get Alert State Behaviour component but found nothing. It's important to set it manually to get the movement and turning animations and speeds.");
            }

            // force shut if not the same state
            if (blaze.state != BlazeAI.State.distracted) {
                enabled = false;
            }
        }


        void Update()
        {   
            // if forced to stay idle by blaze public method
            if (blaze.stayIdle) {
                ReachedDistractionLocation();
                return;
            }

            GetSpeedsAndTurns();


            // the end destination in Blaze is the location the agent wants to go to
            Vector3 distractionPoint = blaze.endDestination;


            // turn to distraction first
            if (!turnedToLocation) {
                _timeToReact += Time.deltaTime;
                
                if (_timeToReact >= timeToReact) {
                    // TurnTo() turns the agent and returns true when fully turned to point
                    if (blaze.TurnTo(distractionPoint, leftTurn, rightTurn, 0.25f)) {
                        turnedToLocation = true;
                        _timeToReact = 0f;
                    }
                }else{
                    // play idle anim
                    blaze.animManager.Play(normalStateBehaviour.idleAnim[0], checkAnimT);
                }
            }


            // can't go further if haven't completely turned
            if (!turnedToLocation) {
                return;
            }


            _timeBeforeMovingToLocation += Time.deltaTime;
            if (_timeBeforeMovingToLocation < timeBeforeMovingToLocation) {
                return;
            }


            // if should check location
            if (checkLocation) {
                // MoveTo() moves the agent to the destination and returns true when reaches destination
                if (blaze.MoveTo(distractionPoint, moveSpeed, turnSpeed, moveAnim)) {
                    ReachedDistractionLocation();
                }
                else {
                    isIdle = false;
                }
            }else{
                turnedToLocation = false;
                _timeBeforeMovingToLocation = 0f;
                isIdle = true;
                blaze.SetState(blaze.previousState);
            }

            SetIdleState();
        }


        void OnDisable()
        {
            ResetDistraction();
        }

        #endregion

        #region BEHAVIOUR METHODS

        // get move & turn speeds/animations
        void GetSpeedsAndTurns()
        {
            if (blaze.previousState == BlazeAI.State.normal) {
                moveSpeed = normalStateBehaviour.moveSpeed;
                turnSpeed = normalStateBehaviour.turnSpeed;
                moveAnim = normalStateBehaviour.moveAnim;
                leftTurn = blaze.waypoints.leftTurnAnimNormal;
                rightTurn = blaze.waypoints.rightTurnAnimNormal;
            }

            if (blaze.previousState == BlazeAI.State.alert) {
                moveSpeed = alertStateBehaviour.moveSpeed;
                turnSpeed = alertStateBehaviour.turnSpeed;
                moveAnim = alertStateBehaviour.moveAnim;
                leftTurn = blaze.waypoints.leftTurnAnimAlert;
                rightTurn = blaze.waypoints.rightTurnAnimAlert;
            }
        }


        // select a random audio to play when reaching the distraction location
        void PlayAudioOnCheckLocation()
        {
            if (playedLocationAudio) {
                return;
            }

            if (blaze.IsAudioScriptableEmpty()) {
                return;
            }
                
            if (blaze.PlayAudio(blaze.audioScriptable.GetAudio(AudioScriptable.AudioType.DistractionCheckLocation))) {
                playedLocationAudio = true;
            }
        }


        void ReachedDistractionLocation()
        {
            if (playAudioOnCheckLocation) {
                PlayAudioOnCheckLocation();
            }

            blaze.animManager.Play(checkAnim, checkAnimT);
            _timeToCheck += Time.deltaTime;
            
            if (_timeToCheck >= timeToCheck) {
                ResetDistraction();
                ResetStayIdle();
                blaze.SetState(blaze.previousState);
            }

            isIdle = true;
        }


        void ResetDistraction()
        {
            _timeToCheck = 0;
            _timeBeforeMovingToLocation = 0;
            turnedToLocation = false;
            _timeToReact = 0;
            playedLocationAudio = false;
        }


        void ResetStayIdle()
        {
            blaze.stayIdle = false;
        }


        void SetIdleState()
        {
            blaze.isIdle = isIdle;
        }

        #endregion
    }
}
