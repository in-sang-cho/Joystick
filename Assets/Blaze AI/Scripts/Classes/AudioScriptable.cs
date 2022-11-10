using UnityEngine;
using System.Collections.Generic;

namespace BlazeAISpace
{
    [CreateAssetMenu(fileName = "BlazeAudioScriptable", menuName = "Blaze AI/Audio Scriptable")]
    public class AudioScriptable : ScriptableObject {
        [Tooltip("Audios to play when patrolling in normal state.")]
        public AudioClip[] normalState;

        [Tooltip("Audios to play when patrolling in alert state.")]
        public AudioClip[] alertState;

        [Tooltip("Audios to play on surprised state. (seeing enemy when in normal state)")]
        public AudioClip[] surprisedState;

        [Tooltip("Audios to play on attacking target.")]
        public AudioClip[] attacks;

        [Tooltip("Audios to play when AI is in attack state waiting for it's turn to attack the player.")]
        public AudioClip[] attackIdle;

        [Tooltip("Audios to play when in alert state and returning to normal state.")]
        public AudioClip[] returningToNormalState;

        [Tooltip("Audios to play on getting distracted.")]
        public AudioClip[] distracted;

        [Tooltip("Audios to play when checking distraction location.")]
        public AudioClip[] distractionCheckLocation;

        [Tooltip("Audios to play when hit.")]
        public AudioClip[] hit;

        [Tooltip("Audios to play on death.")]
        public AudioClip[] death;

        [Tooltip("Audios to play when vision catches an alert tag game object.")]
        public AudioClip[] alertTags;


        public enum AudioType : int {
            NormalState = 0,
            AlertState,
            SurprisedState,
            Attacks,
            AttackIdle,
            ReturningToNormalState,
            Distracted,
            DistractionCheckLocation,
            Hit,
            Death,
            AlertTags
        }


        Dictionary<int, AudioClip[]> audios = new Dictionary<int, AudioClip[]>();


        void OnEnable()
        {
            audios.Clear();

            audios.Add((int)AudioType.NormalState, normalState);
            audios.Add((int)AudioType.AlertState, alertState);
            audios.Add((int)AudioType.SurprisedState, surprisedState);
            audios.Add((int)AudioType.Attacks, attacks);
            audios.Add((int)AudioType.AttackIdle, attackIdle);
            audios.Add((int)AudioType.ReturningToNormalState, returningToNormalState);
            audios.Add((int)AudioType.Distracted, distracted);
            audios.Add((int)AudioType.DistractionCheckLocation, distractionCheckLocation);
            audios.Add((int)AudioType.Hit, hit);
            audios.Add((int)AudioType.Death, death);
            audios.Add((int)AudioType.AlertTags, alertTags);
        }


        public AudioClip GetAudio(AudioType type)
        {
            if (audios[(int)type].Length <= 0) {
                return null;
            }


            int randomSound = 0;
            randomSound = Random.Range(0, audios[(int)type].Length);

            
            if (audios[(int)type][randomSound] == null) {
                return null;
            }

            
            return audios[(int)type][randomSound];
        }


        public AudioClip GetAudio(AudioType type, int index)
        {
            if (audios[(int)type].Length <= 0) {
                return null;
            }

            if (index > audios[(int)type].Length - 1) {
                return null;
            }

            if (audios[(int)type][index] == null) {
                return null;
            }


            return audios[(int)type][index];
        }


        public AudioClip[] GetAudios(AudioType type)
        {
            if (audios[(int)type].Length <= 0) {
                return null;
            }
            
            
            return audios[(int)type];
        }
    }
}
