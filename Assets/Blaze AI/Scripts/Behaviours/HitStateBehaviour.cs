using UnityEngine;

namespace BlazeAISpace
{
    public class HitStateBehaviour : MonoBehaviour
    {   
        [Tooltip("Hit animation name.")]
        public string hitAnim;
        [Min(0), Tooltip("The animation transition from current animation to the hit animation.")]
        public float hitAnimT = 0.2f;
        [Min(0), Tooltip("The duration of the hit state.")]
        public float hitDuration;
        [Min(0), Tooltip("The gap time between the hit animations to avoid having the hit animation play on every single which may look bad on very fast and repitive attacks such as a machine gun.")]
        public float hitAnimGap = 0.3f;
        [Tooltip("If set to true will cancel the attack if got hit.")]
        public bool cancelAttackOnHit;
        [Tooltip("Play audio when hit. Set your audios in the audio scriptable in the General Tab in Blaze AI.")]
        public bool playAudio;


        BlazeAI blaze;
        float _duration = 0;
        float _gapTimer = 0;
        bool playedAudio;


        void Start()
        {
            blaze = GetComponent<BlazeAI>();    

            // force shut if not the same state
            if (blaze.state != BlazeAI.State.hit) {
                enabled = false;
            }
        }


        void OnDisable()
        {
            ResetTimers();
            blaze.hitEnemy = null;
            playedAudio = false;
        }


        void OnValidate()
        {
            // hit duration can't be smaller than the hit animation gap
            if (hitDuration < hitAnimGap) {
                hitAnimGap = hitDuration;
            }
        }


        void Update()
        {
            // a hit was registered
            if (blaze.hitRegistered) {
                blaze.hitRegistered = false;

                if (_duration == 0) {
                    blaze.animManager.Play(hitAnim, hitAnimT, true);
                }
                else {
                    if (_gapTimer >= hitAnimGap) {
                        blaze.animManager.Play(hitAnim, hitAnimT, true);
                        _gapTimer = 0;
                    }
                }
                
                _duration = 0;
            }


            // hit audio
            PlayAudio();


            // cancel attacks
            if (cancelAttackOnHit) {
                blaze.StopAttack();
            }


            _gapTimer += Time.deltaTime;


            // hit duration timer
            _duration += Time.deltaTime;
            if (_duration >= hitDuration) {
                FinishHitState();
            }
        }


        void FinishHitState()
        {
            ResetTimers();

            // if the enemy that did the hit is passed -> set AI to go to enemy location
            if (blaze.hitEnemy) {
                // check the passed enemy isn't the same AI
                if (blaze.hitEnemy.transform == blaze.transform) {
                    blaze.SetEnemy(null);
                    return;
                }
                
                blaze.SetEnemy(blaze.hitEnemy);
                return;
            }
            
            // if not -> turn alert
            blaze.SetEnemy(null);
        }


        void PlayAudio()
        {
            if (!playAudio) {
                return;
            }
            
            if (playedAudio) {
                return;
            }

            if (blaze.IsAudioScriptableEmpty()) {
                return;
            }

            if (blaze.PlayAudio(blaze.audioScriptable.GetAudio(AudioScriptable.AudioType.Hit))) {
                playedAudio = true;
            }
        }


        void ResetTimers()
        {
            _duration = 0;
            _gapTimer = 0;
        }
    }
}
