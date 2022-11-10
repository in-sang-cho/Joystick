using UnityEditor;

namespace BlazeAISpace
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(DistractedStateBehaviour))]
    public class DistractedStateBehaviourInspector : Editor
    {
        SerializedProperty timeToReact,
        checkLocation,
        timeBeforeMovingToLocation,
        checkAnim,
        checkAnimT,
        timeToCheck,
        playAudioOnCheckLocation;


        void OnEnable()
        {
            timeToReact = serializedObject.FindProperty("timeToReact");
            
            checkLocation = serializedObject.FindProperty("checkLocation");
            timeBeforeMovingToLocation = serializedObject.FindProperty("timeBeforeMovingToLocation");
            checkAnim = serializedObject.FindProperty("checkAnim");
            checkAnimT = serializedObject.FindProperty("checkAnimT");
            timeToCheck = serializedObject.FindProperty("timeToCheck");
            playAudioOnCheckLocation = serializedObject.FindProperty("playAudioOnCheckLocation");
        }


        public override void OnInspectorGUI () 
        {
            DistractedStateBehaviour script = (DistractedStateBehaviour) target;
            int spaceBetween = 20;

            EditorGUILayout.LabelField("REACTION TIME", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(timeToReact);

            
            EditorGUILayout.Space(spaceBetween);


            EditorGUILayout.LabelField("CHECKING LOCATION", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(checkLocation);
            if (script.checkLocation) {
                EditorGUILayout.PropertyField(timeBeforeMovingToLocation);
                EditorGUILayout.PropertyField(checkAnim);
                EditorGUILayout.PropertyField(checkAnimT);
                EditorGUILayout.PropertyField(timeToCheck);
            }


            EditorGUILayout.Space(spaceBetween);


            EditorGUILayout.LabelField("AUDIOS", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(playAudioOnCheckLocation);


            serializedObject.ApplyModifiedProperties();
        }
    }
}


