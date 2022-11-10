using UnityEngine;
using UnityEditor;
using BlazeAISpace;

[CanEditMultipleObjects]
[CustomEditor(typeof(BlazeAI))]
public class BlazeAIEditor : Editor
{
    string[] tabs = {"General", "States", "Surprised", "Distractions", "Hit", "Death"};
    int tabSelected = 0;
    int tabIndex = -1;

    // variables
    SerializedProperty useRootMotion,
    centerPosition,
    showCenterPosition,
    groundLayers,

    audioScriptable,
    
    waypoints,
    vision,
    
    checkEnemyContact,
    enemyContactRadius,
    showEnemyContactRadius,
    
    useNormalStateOnAwake,
    normalStateBehaviour,

    useAlertStateOnAwake,
    alertStateBehaviour,

    attackStateBehaviour,
    coverShooterMode,
    coverShooterBehaviour,
    goingToCoverBehaviour,

    useSurprisedState,
    surprisedStateBehaviour,

    canDistract,
    distractedStateBehaviour,
    priorityLevel,
    playDistractedAudios,

    hitStateBehaviour,
    
    deathAnim,
    deathAnimT,
    playDeathAudio,
    disableBlazeOnDeath,

    warnEmptyBehavioursOnStart;


    void OnEnable()
    {
        if (EditorPrefs.HasKey("TabSelected")) {
            tabSelected = EditorPrefs.GetInt("TabSelected");
        }else{
            tabSelected = 0;
        }   

        useRootMotion = serializedObject.FindProperty("useRootMotion");
        centerPosition = serializedObject.FindProperty("centerPosition");
        showCenterPosition = serializedObject.FindProperty("showCenterPosition");
        groundLayers = serializedObject.FindProperty("groundLayers");

        audioScriptable = serializedObject.FindProperty("audioScriptable");
        
        waypoints = serializedObject.FindProperty("waypoints");
        vision = serializedObject.FindProperty("vision");

        checkEnemyContact = serializedObject.FindProperty("checkEnemyContact");
        enemyContactRadius = serializedObject.FindProperty("enemyContactRadius");
        showEnemyContactRadius = serializedObject.FindProperty("showEnemyContactRadius");

        warnEmptyBehavioursOnStart = serializedObject.FindProperty("warnEmptyBehavioursOnStart");

        useNormalStateOnAwake = serializedObject.FindProperty("useNormalStateOnAwake");
        normalStateBehaviour = serializedObject.FindProperty("normalStateBehaviour");

        useAlertStateOnAwake = serializedObject.FindProperty("useAlertStateOnAwake");
        alertStateBehaviour = serializedObject.FindProperty("alertStateBehaviour");

        attackStateBehaviour = serializedObject.FindProperty("attackStateBehaviour");
        coverShooterMode = serializedObject.FindProperty("coverShooterMode");
        coverShooterBehaviour = serializedObject.FindProperty("coverShooterBehaviour");
        goingToCoverBehaviour = serializedObject.FindProperty("goingToCoverBehaviour");

        useSurprisedState = serializedObject.FindProperty("useSurprisedState");
        surprisedStateBehaviour = serializedObject.FindProperty("surprisedStateBehaviour");

        canDistract = serializedObject.FindProperty("canDistract");
        distractedStateBehaviour = serializedObject.FindProperty("distractedStateBehaviour");
        priorityLevel = serializedObject.FindProperty("priorityLevel");
        playDistractedAudios = serializedObject.FindProperty("playDistractedAudios");

        hitStateBehaviour = serializedObject.FindProperty("hitStateBehaviour");

        deathAnim = serializedObject.FindProperty("deathAnim");
        deathAnimT = serializedObject.FindProperty("deathAnimT");
        playDeathAudio = serializedObject.FindProperty("playDeathAudio");
        disableBlazeOnDeath = serializedObject.FindProperty("disableBlazeOnDeath");
    }

    public override void OnInspectorGUI () 
    {
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        
        StyleToolbar();
        EditorGUILayout.LabelField("There is a default behaviour script for every behaviour property with the same name.", EditorStyles.textField);
        EditorGUILayout.LabelField("Hover on any property below for insights", EditorStyles.helpBox);

        // reset the tabs
        tabIndex = -1;

        GUI.backgroundColor = oldColor;
        BlazeAI script = (BlazeAI)target;
        

        switch (tabSelected)
        {
            case 0:
                GeneralTab(script);
                break;
            case 1:
                StatesTab(script);
                break;
            case 2:
                SurprisedTab();
                break;
            case 3:
                DistractionsTab();
                break;
            case 4:
                HitTab();
                break;
            case 5:
                DeathTab();
                break;
        }

        EditorPrefs.SetInt("TabSelected", tabSelected);
        serializedObject.ApplyModifiedProperties();
    }

    void StyleToolbar()
    {
        // unselected btns style
        var unSelectedBtn = new GUIStyle(GUI.skin.button);
        unSelectedBtn.fixedHeight = 45;
        unSelectedBtn.normal.textColor = Color.red;
        unSelectedBtn.fontSize = 13;

        // selected btn style
        var selectedBtn = new GUIStyle(GUI.skin.button);
        selectedBtn.fixedHeight = 45;
        selectedBtn.normal.textColor = Color.white;
        selectedBtn.fontSize = 15;
        selectedBtn.normal.background = MakeTex(0, 45, Color.gray);

        // render the toolbar
        GUILayout.BeginHorizontal("box");
        foreach (var item in tabs) {
            tabIndex++;
            if (tabIndex == tabSelected) {
                GUILayout.Button(item, selectedBtn);
            }else{
                if (GUILayout.Button(item, unSelectedBtn)) {
                    tabSelected = tabIndex;
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    Texture2D MakeTex( int width, int height, Color col )
    {
        Color[] pix = new Color[width * height];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix[ i ] = col;
        }
        Texture2D result = new Texture2D( width, height );
        result.SetPixels( pix );
        return result;
    }

    // render the general tab properties
    void GeneralTab(BlazeAI script)
    {
        EditorGUILayout.PropertyField(useRootMotion);
        EditorGUILayout.PropertyField(centerPosition);
        EditorGUILayout.PropertyField(showCenterPosition);
        EditorGUILayout.PropertyField(groundLayers);
        EditorGUILayout.PropertyField(audioScriptable);
        
        EditorGUILayout.Space(7);
        EditorGUILayout.PropertyField(waypoints);
        
        EditorGUILayout.Space(7);
        EditorGUILayout.PropertyField(vision);


        EditorGUILayout.Space(7);
        EditorGUILayout.PropertyField(checkEnemyContact);
        if (script.checkEnemyContact) {
            EditorGUILayout.PropertyField(enemyContactRadius);
            EditorGUILayout.PropertyField(showEnemyContactRadius);
        }


        EditorGUILayout.Space(7);
        EditorGUILayout.PropertyField(warnEmptyBehavioursOnStart);

        EditorGUILayout.Space(10);
    }

    // render the states classes
    void StatesTab(BlazeAI script)
    {
        EditorGUILayout.PropertyField(useNormalStateOnAwake);
        EditorGUILayout.PropertyField(normalStateBehaviour);

        EditorGUILayout.PropertyField(useAlertStateOnAwake);
        EditorGUILayout.PropertyField(alertStateBehaviour);     

        EditorGUILayout.Space(7);
        EditorGUILayout.LabelField("ATTACK STATE", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(script.coverShooterMode);
            EditorGUILayout.PropertyField(attackStateBehaviour);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.PropertyField(coverShooterMode);

        EditorGUI.BeginDisabledGroup(!script.coverShooterMode);
            EditorGUILayout.PropertyField(coverShooterBehaviour);
            EditorGUILayout.PropertyField(goingToCoverBehaviour);
        EditorGUI.EndDisabledGroup();


        EditorGUILayout.Space(10);
    }

    void SurprisedTab()
    {
        EditorGUILayout.LabelField("Surprised State triggers when the AI finds a target in Normal State.", EditorStyles.textField);
        EditorGUILayout.PropertyField(useSurprisedState);
        EditorGUILayout.PropertyField(surprisedStateBehaviour);
    }

    // render the distractions tab class
    void DistractionsTab()
    {
        EditorGUILayout.PropertyField(canDistract);
        EditorGUILayout.PropertyField(distractedStateBehaviour);
        EditorGUILayout.PropertyField(priorityLevel);

        EditorGUILayout.PropertyField(playDistractedAudios);
        
        EditorGUILayout.Space(10);
    }

    // render the hits tab class
    void HitTab()
    {
        EditorGUILayout.PropertyField(hitStateBehaviour);
        EditorGUILayout.Space(10);
    }

    // render the death tab class
    void DeathTab()
    {
        EditorGUILayout.PropertyField(deathAnim);
        EditorGUILayout.PropertyField(deathAnimT);
        EditorGUILayout.PropertyField(playDeathAudio);
        EditorGUILayout.PropertyField(disableBlazeOnDeath);

        EditorGUILayout.Space(10);
    }
}
