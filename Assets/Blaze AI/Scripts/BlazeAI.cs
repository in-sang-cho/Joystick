using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlazeAISpace;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]

public class BlazeAI : MonoBehaviour 
{
    [Header("GENERAL")]
    [Tooltip("Enabling this will make the agent use root motion, this gives more accurate and realistic movement but any move speed property will not be considered as the speed will be that of the animation.")]
    public bool useRootMotion = false;
    [Tooltip("This will be the center position of the AI used in many calculations. Best to position at the pelvis/torso area.")]
    public Vector3 centerPosition = new Vector3(0, 1.2f, 0);
    [Tooltip("Will show the center position as a red sphere in the scene view.")]
    public bool showCenterPosition = true;
    public LayerMask groundLayers;

    [Header("AUDIOS"), Tooltip("All audios for Blaze are added in a scriptable then the behaviours as well as Blaze read from this scriptable. To create an audio scriptable: Right-click in the Project window > Create > Blaze AI > Audio Scriptable.")]
    public AudioScriptable audioScriptable;

    [Header("PATROL ROUTES & TURNING ANIMS")]
    public BlazeAISpace.Waypoints waypoints;

    [Header("VISION & ADDING ENEMIES")]
    public BlazeAISpace.Vision vision;

    [Header("CHECK FOR ENEMY CONTACT"), Tooltip("Check if a hostile got too close and came in contact with the AI. If so, will turn to attack state.")]
    public bool checkEnemyContact;
    [Min(0), Tooltip("The radius for checking if a hostile came in contact.")]
    public float enemyContactRadius = 1.2f;
    [Tooltip("Shows the radius as a grey wire sphere in the scene view.")]
    public bool showEnemyContactRadius;

    [Header("NORMAL STATE")]
    public bool useNormalStateOnAwake;
    public MonoBehaviour normalStateBehaviour;

    [Header("ALERT STATE")]
    public bool useAlertStateOnAwake;
    public MonoBehaviour alertStateBehaviour;
    
    public MonoBehaviour attackStateBehaviour;
    public bool coverShooterMode;
    public MonoBehaviour coverShooterBehaviour;
    public MonoBehaviour goingToCoverBehaviour;

    [Header("SURPRISED STATE")]
    public bool useSurprisedState;
    public MonoBehaviour surprisedStateBehaviour;

    [Header("DISTRACTED STATE")]
    public bool canDistract = true;
    public MonoBehaviour distractedStateBehaviour;
    [Range(0, 100), Tooltip("If a distraction triggers a group of agents, the highest priority AI only is sent to the distraction point. Here you can set which AI is more prone to check the distraction.")]
    public float priorityLevel = 50;

    [Header("AUDIOS"), Tooltip("Play audio when distracted. Set the audios in the audio scriptable in the General tab.")]
    public bool playDistractedAudios;

    [Header("HIT STATE")]
    public MonoBehaviour hitStateBehaviour;

    [Header("DEATH STATE")]
    public string deathAnim;
    [Min(0)]
    public float deathAnimT = 0.25f;
    [Space(7), Tooltip("Set your audios in the audio scriptable in the General Tab in Blaze AI.")]
    public bool playDeathAudio;
    [Tooltip("If this is set to true the Blaze component will be disabled on death this improves performance as there's no need to have the component enabled on a dead AI so it's recommended to have this set to true. But, maybe you need it enabled for respawning or any other reason for your game. If so, set to false.")]
    public bool disableBlazeOnDeath;

    [Header("WARN EMPTY BEHAVIOURS"), Tooltip("Will print in the console to warn you if any behaviour is empty.")]
    public bool warnEmptyBehavioursOnStart = true;

    
    #region SYSTEM VARIABLES

    Animator anim;
    public NavMeshAgent navmeshAgent { get; private set; }
    CapsuleCollider capsuleCollider;
    NavMeshPath path;
    [HideInInspector] public AnimationManager animManager;

    public enum State 
    {
        normal,
        alert,
        attack,
        goingToCover,
        sawAlertTag,
        returningToAlert,
        surprised,
        distracted,
        hit,
        death
    }

    public State state { get; private set; }
    public int waypointIndex { get; private set; }
    public bool isPathReachable { get; private set; }
    public AudioSource agentAudio { get; set; }
    public Vector3 endDestination { get; private set; }
    public State previousState { get; private set; }
    public GameObject enemyToAttack { get; private set; }
    public Vector3 checkEnemyPosition { get; set; }
    public float captureEnemyTimeStamp { get; private set; }
    public Vector3 enemyColPoint { get; private set; }
    public bool isAttacking { get; set; }
    public float distanceToEnemySqrMag { get; private set; }
    public float distanceToEnemy { get; private set; }
    public Vector3 enemyPosOnSurprised { get; private set; }
    public string sawAlertTagName { get; private set; }
    public Vector3 sawAlertTagPos { get; private set; }
    
    
    // used with public methods and read by behaviours
    public bool movedToLocation { get; set; }
    public bool stayIdle { get; set; }
    public bool isIdle { get; set; }
    public bool hitRegistered { get; set; }
    public bool changedState { get; set; }
    public GameObject hitEnemy { get; set; }

    
    public Vector3 pathCorner { get; set; }
    Vector3 startPosition;
    Queue<Vector3> cornersQueue;

    bool useNormalStateOnAwakeInspectorState;
    bool useAlertStateOnAwakeInspectorState;
    bool isturningToCorner;
    MonoBehaviour lastEnabledBehaviour;

    int visionCheckElapsed = 0;
    int closestPointElapsed = 5;
    int checkSurroundingElapsed = 0;

    Collider[] visionColl = new Collider[20];

    Transform visionT;

    #endregion

    #region UNITY METHODS

    void Start()
    {
        anim = GetComponent<Animator>();
        animManager = new AnimationManager(anim);

        navmeshAgent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        cornersQueue = new Queue<Vector3>();
        
        
        startPosition = transform.position;
        waypointIndex = -1;
        ComponentsOnAwake();


        vision.CheckHostileAndAlertItemEqual();


        // set state on awake
        if (useNormalStateOnAwake) {
            SetState(State.normal);
            return;
        }
        
        SetState(State.alert);
    }

    void Update()
    {
        // set the vision to head if available
        if (vision.head == null) {
            visionT = transform;
        }
        else {
            visionT = vision.head;
        }


        // always apply the anim root speed if using root motion
        if (useRootMotion) {
            Vector3 worldDeltaPosition = navmeshAgent.nextPosition - transform.position;

            if (worldDeltaPosition.magnitude > navmeshAgent.radius) {
                navmeshAgent.nextPosition = transform.position + 0.9f * worldDeltaPosition;
            }
        }


        // enable the state's behaviour
        switch (state) {
            case State.normal:
                EnableBehaviour(normalStateBehaviour);
                break;
            case State.alert:
                EnableBehaviour(alertStateBehaviour);
                break;
            case State.attack:
                if (coverShooterMode) {
                    EnableBehaviour(coverShooterBehaviour);
                }
                else { 
                    EnableBehaviour(attackStateBehaviour);
                }
                break;
            case State.sawAlertTag:
                EnableBehaviour(vision.alertTags[vision.GetAlertTagIndex(sawAlertTagName)].behaviourScript);
                break;
            case State.distracted:
                EnableBehaviour(distractedStateBehaviour);
                break;
            case State.surprised:
                EnableBehaviour(surprisedStateBehaviour);
                break;
            case State.goingToCover:
                EnableBehaviour(goingToCoverBehaviour);
                break;
            case State.hit:
                EnableBehaviour(hitStateBehaviour);
                break;
            case State.returningToAlert:
                if (coverShooterMode) {
                    EnableBehaviour(coverShooterBehaviour);
                }
                else { 
                    EnableBehaviour(attackStateBehaviour);
                }
                break;
        }


        VisionCheck();
        SurroundingsCheck();
        RemoveMoveToLocation();
    }
    
    void OnAnimatorMove()
    {
        if (!useRootMotion) {
            return;
        }

        if (anim == null) {
            return;
        }

        Vector3 position = anim.rootPosition;
        position.y = navmeshAgent.nextPosition.y;
        transform.position = position;
    }

    void OnValidate()
    {
        // choose either UseNormalStateOnAwake or UseAlertStateOnAwake (can't be both)
        if (!useAlertStateOnAwake && !useNormalStateOnAwake) {
            useNormalStateOnAwake = true;
        }

        if (useAlertStateOnAwake && useNormalStateOnAwake) {
            useAlertStateOnAwake = !useAlertStateOnAwakeInspectorState;
            useNormalStateOnAwake = !useNormalStateOnAwakeInspectorState;
        }

        useNormalStateOnAwakeInspectorState = useNormalStateOnAwake;
        useAlertStateOnAwakeInspectorState = useAlertStateOnAwake;


        // validate waypoints system
        if (waypoints != null) {
            waypoints.WaypointsValidation(transform.position);
        }


        DisableAllBehaviours();


        if (vision != null) {
            vision.DisableAllAlertBehaviours();
            vision.CheckHostileAndAlertItemEqual(true);

            if (vision.head == null) {
                visionT = transform;
            }
            else {
                visionT = vision.head;
            }
        }
    }

    void OnDrawGizmosSelected() {
        waypoints.Draw(transform.position);
        vision.ShowVisionSpheres(visionT);
        ShowCenterPosition();
        ShowEnemyContactRadius();
    }

    // enable & set important components on awake
    void ComponentsOnAwake()
    {
        navmeshAgent.enabled = true;
        navmeshAgent.stoppingDistance = 0;
        navmeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        NavMesh.avoidancePredictionTime = 0.5f;


        agentAudio = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
        agentAudio.playOnAwake = false;


        if (warnEmptyBehavioursOnStart) {
            CheckEmptyBehaviours();
        }
    }
    
    // print in the console if a behaviour is missing a script
    void CheckEmptyBehaviours()
    {
        if (useNormalStateOnAwake) {
            if (normalStateBehaviour == null) {
                print($"Normal State Behaviour is empty in game object: {gameObject.name}.");
            }
        }

        if (useAlertStateOnAwake) {
            if (alertStateBehaviour == null) {
                print($"Alert State Behaviour is empty in game object: {gameObject.name}.");
            }
        }

        if (canDistract) {
            if (distractedStateBehaviour == null) {
                print ($"Distracted State Behaviour is empty in game object: {gameObject.name}.");
            }
        }

        if (useSurprisedState) {
            if (surprisedStateBehaviour == null) {
                print ($"Surprised State Behaviour is empty in game object: {gameObject.name}.");
            }
        }

        if (!coverShooterMode) {
            if (attackStateBehaviour == null) {
                print ($"Attack State Behaviour is empty in game object: {gameObject.name}.");
            }
        }
        else {
            if (coverShooterBehaviour == null) {
                print($"Cover Shooter Behaviour is empty in game object: {gameObject.name}.");
            }

            if (goingToCoverBehaviour == null) {
                print($"Going To Cover Behaviour is empty in game object: {gameObject.name}.");
            }
        }

        if (hitStateBehaviour == null) {
            print($"Hit State Behaviour is empty in game object: {gameObject.name}.");
        }
    }

    void OnDisable() 
    {
        DisableAllBehaviours();
    }

    void OnEnable()
    {
        // if blaze is enabled -> enable navmesh agent component
        if (navmeshAgent != null) {
            navmeshAgent.enabled = true;
        }
    }

    #endregion

    #region MOVEMENT
    
    // move to location
    public bool MoveTo(Vector3 location, float moveSpeed, float turnSpeed, string animName=null, float animT=0.25f, string dir="front", float closestPointDistance=0) {
        if (dir == "front") {
            if (!isAttacking || enemyToAttack == null) {
                // check if AI is already at the min possible distance from location
                float dist = (new Vector3(location.x, transform.position.y, location.z) - transform.position).sqrMagnitude;
                float minDis = navmeshAgent.radius * 2;
                
                if (dist <= minDis * minDis) {
                    movedToLocation = false;
                    return true;
                }
            }
        }

        // clear the corners
        cornersQueue.Clear();

        
        // calculates path corners and returns if reachable or not
        if (!IsPathReachable(location)) {
            if (dir != "front") {
                return false;
            }

            // if not set to check for closest point -> return false
            if (closestPointDistance <= 0) {
                return false;
            }
            
            closestPointElapsed++;

            // get closest point every 5 frames (for performance)
            if (closestPointElapsed > 5) {
                closestPointElapsed = 0;
                
                Vector3 point;
                if (ClosestNavMeshPoint(location, closestPointDistance, out point)) {
                    location = point;
                }
                else {
                    // vector zero means couldn't find a good point
                    if (point == Vector3.zero) {
                        return false;
                    }
                }
            }
        }
        
        
        // add the corners to queue so we can follow
        for (int i=0; i<path.corners.Length; i++) {
            cornersQueue.Enqueue(path.corners[i]);
        }
        

        // get the next corner
        GetNextCorner();

        
        return GoToCorner(animName, animT, moveSpeed, turnSpeed, dir);
    }

    // follow the path corners
    bool GoToCorner(string anim, float animT, float moveSpeed, float turnSpeed, string dir)
    {
        float currentDistance = (new Vector3(pathCorner.x, transform.position.y, pathCorner.z) - transform.position).sqrMagnitude;
        float minDistance = 0f;

        bool isLastCorner = false;
        bool isReachedEnd = false;

        
        // check if going to last corner or not
        if (cornersQueue.Count > 0) {
            minDistance = 0.3f;
        }
        else {
            minDistance = navmeshAgent.radius * 2;
            isLastCorner = true;
        }

        
        // if reached min distance of corner
        if (currentDistance <= minDistance * minDistance) {
            if (isLastCorner) {
                isReachedEnd = true;
            }else{
                GetNextCorner();
            }
        }
        

        // turning to path corner shouldn't be done in attack states
        if (state != State.attack && state != State.goingToCover) 
        {
            // turn to face path corner
            if (waypoints.useMovementTurning) {
                // check if should turning
                if (isturningToCorner) {
                    // if hadn't fully turned yet -> return
                    if (!TurnTo(pathCorner, GetTurnAnim("left"), GetTurnAnim("right"), waypoints.turningAnimT, waypoints.turnSpeed)) {
                        return false;
                    }

                    // reaching this point means turning has completed
                    isturningToCorner = false;
                }

                
                // calculate the dot prod of the path corner
                float dotProd = Vector3.Dot((pathCorner - transform.position).normalized, transform.forward);
                
                
                // determine if should be turning
                if (dotProd < Mathf.Clamp(waypoints.movementTurningSensitivity, -1, 0.97f)) {
                    // if should turn then flag as so and return
                    isturningToCorner = true;
                    return false;
                }
            }
        }


        // rotate to corner
        RotateTo(pathCorner, turnSpeed);
        
        
        // play passed move animation
        animManager.Play(anim, animT);

        
        // only applied if not using root motion -> if using root motion, speed apply is in OnAnimatorMove()
        if (!useRootMotion) {
            Vector3 transformDir;

            switch (dir) {
                case "backwards":
                    transformDir = -transform.forward;
                    break;
                case "left":
                    transformDir = -transform.right;
                    break;
                case "right":
                    transformDir = transform.right;
                    break;
                default:
                    transformDir = transform.forward;
                    break;
            }

            navmeshAgent.Move(transformDir * Time.deltaTime * moveSpeed);
        }


        return isReachedEnd;
    }

    // get the next corner
    void GetNextCorner()
    {
        if (cornersQueue.Count > 0) {
            pathCorner = cornersQueue.Dequeue();
            pathCorner = new Vector3(pathCorner.x, transform.position.y, pathCorner.z);
        }
    }

    // smooth rotate agent to location
    public void RotateTo(Vector3 location, float speed)
    {
        Quaternion lookRotation = Quaternion.LookRotation((location - transform.position).normalized);
        lookRotation = new Quaternion(0f, lookRotation.y, 0f, lookRotation.w);
        transform.rotation = Quaternion.Slerp(new Quaternion(0f, transform.rotation.y, 0f, transform.rotation.w), lookRotation, speed * Time.deltaTime);
    }

    // set waypoint index to the next waypoint
    public Vector3 NextWayPoint()
    {   
        if (waypointIndex >= waypoints.waypoints.Length - 1) {
            if (waypoints.loop) waypointIndex = 0;
        }
        else {
            waypointIndex++;
        }

        endDestination = waypoints.waypoints[waypointIndex];
        return endDestination;
    }

    // returns true if there is a waypoint rotation
    public bool CheckWayPointRotation()
    {
        if ((waypoints.waypointsRotation[waypointIndex].x != 0 || waypoints.waypointsRotation[waypointIndex].y != 0)) {
            float dotProd = Vector3.Dot((new Vector3(transform.position.x + waypoints.waypointsRotation[waypointIndex].x, transform.position.y, transform.position.z + waypoints.waypointsRotation[waypointIndex].y) - transform.position).normalized, transform.forward);
            if (dotProd < 0.97f) {
                return true;
            }else{
                return false;
            }
        }
        else {
            return false;
        }
    }
    
    // turns AI to waypoint rotations and returns true when done
    public bool WayPointTurning()
    {   
        // set the turning anims of the state
        string leftTurnAnim = GetTurnAnim("left");
        string rightTurnAnim = GetTurnAnim("right");
        float animT = waypoints.turningAnimT;

        if ((waypoints.waypointsRotation[waypointIndex].x != 0 || waypoints.waypointsRotation[waypointIndex].y != 0)) {
            Vector3 wayPointDir = new Vector3(transform.position.x + waypoints.waypointsRotation[waypointIndex].x, transform.position.y, transform.position.z + waypoints.waypointsRotation[waypointIndex].y);
            return TurnTo(wayPointDir, leftTurnAnim, rightTurnAnim, animT);
        }else{
            return true;
        }
    }

    // turn to location and returns true when done
    public bool TurnTo(Vector3 location, string leftTurnAnim = null, string rightTurnAnim = null, float animT = 0.25f, float turnSpeed=0)
    {
        location = new Vector3(location.x, transform.position.y, location.z);

        // get dir (left or right)
        int waypointTurnDir = AngleDir
        (transform.forward, 
        location - transform.position, 
        transform.up);

        
        float dotProd = Vector3.Dot((location - transform.position).normalized, transform.forward);
        if (dotProd >= 0.97f) {
            return true;
        }


        // turn right if dir is 1
        if (waypointTurnDir == 1) {
            animManager.Play(rightTurnAnim, animT);
        }else{
            animManager.Play(leftTurnAnim, animT);
        }


        if (turnSpeed == 0) {
            turnSpeed = waypoints.turnSpeed;
        }

        RotateTo(location, turnSpeed);

       
       return false;
    }

    // return 1 if location is to the right and -1 if left
    int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up) 
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
        
        if (dir > 0f) {
            return 1;
        } else if (dir < 0f) {
            return -1;
        } else {
            return 0;
        }
    }

    // return the turn anim name depending on current state
    string GetTurnAnim(string dir) 
    {
        if (state == State.normal) {
            if (dir == "right") return waypoints.rightTurnAnimNormal;
            else return waypoints.leftTurnAnimNormal;
        }
        else{
            if (dir == "right") return waypoints.rightTurnAnimAlert;
            else return waypoints.leftTurnAnimAlert;
        }
    }

    #endregion

    #region DISTRACTED STATE
    
    // distract the AI
    public void Distract(Vector3 location, bool playAudio = true)
    {
        if (!canDistract || state == State.attack || !enabled || state == State.hit || state == State.death) {
            return;
        }
        
        
        // get nearest navmesh position
        Vector3 pos = GetSamplePosition(ValidateYPoint(location), navmeshAgent.radius * 2);
        if (pos == Vector3.zero || pos == endDestination) {
            return;
        }

        
        endDestination = pos;


        if (state != State.distracted) {
            if (state == State.returningToAlert) previousState = State.alert;
            else previousState = state;
        }
        else {
            // if already in distracted state -> re-enable behaviour to reset
            distractedStateBehaviour.enabled = false;
            distractedStateBehaviour.enabled = true;
        }


        // sometimes this parameter is passed as false to avoid all distracted agents playing audio
        // which will sound distorted -> so only one agent in a group plays the audio
        if (playAudio) {
            if (playDistractedAudios) {
                // play audio only if not already in distracted state
                if (state != State.distracted) {
                    if (!IsAudioScriptableEmpty()) {
                        PlayAudio(audioScriptable.GetAudio(AudioScriptable.AudioType.Distracted));
                    }
                }
            }
        }


        // change the state to distracted
        SetState(State.distracted);
    }
    
    #endregion

    #region NAVMESH

    // get random point from navmesh
    public Vector3 RandomNavMeshLocation() 
    {
        Vector3 randomDirection = Random.insideUnitSphere * waypoints.randomizeRadius;
        randomDirection += startPosition;
        
        NavMeshHit hit;
        Vector3 point;


        NavMesh.SamplePosition(randomDirection, out hit, waypoints.randomizeRadius, 1);
        point = hit.position;


        float distance = (new Vector3(point.x, transform.position.y, point.y) - transform.position).sqrMagnitude;
        float radius = navmeshAgent.radius * 2;


        if (distance <= radius * radius) {
            RandomNavMeshLocation();
        }

        endDestination = point;
        return point;
    }

    // check whether point is on navmesh or not
    public bool IsPointOnNavMesh(Vector3 point, float radius = 2f)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(point, out hit, radius, NavMesh.AllAreas)) return true;
        else return false;
    }

    // get nearest position within point
    public Vector3 GetSamplePosition(Vector3 point, float range)
    {
        NavMeshHit hit;

        if (NavMesh.SamplePosition(point, out hit, range, NavMesh.AllAreas)) return hit.position;
        else return Vector3.zero;
    }

    // get the correct y position of an enemy
    public Vector3 ValidateYPoint(Vector3 pos)
    {
        if (!IsPointOnNavMesh(pos, 0.3f)) {
            RaycastHit downHit;
            if (Physics.Raycast(pos, -Vector3.up, out downHit, Mathf.Infinity, NavMesh.AllAreas)) {
                return downHit.point;
            }
            else {
                return new Vector3(pos.x, transform.position.y, pos.z);
            }
        }else{
            return pos;
        }
    }

    // is path status complete
    public bool IsPathReachable(Vector3 position) 
    {
        if (position.x == Mathf.Infinity || position.z == Mathf.Infinity || position.y == Mathf.Infinity) {
            return false;
        }

        bool pathValidation = NavMesh.CalculatePath(ValidateYPoint(transform.position), ValidateYPoint(position), NavMesh.AllAreas, path);

        if (path.status == NavMeshPathStatus.PathComplete) {
            isPathReachable = true;
            return true;
        }
        
        isPathReachable = false;
        return false;
    }

    // get closest point to navmesh
    bool ClosestNavMeshPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < range; i++) {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(center, out hit, range, NavMesh.AllAreas)) {
                if (IsPathReachable(hit.position)) {
                    result = hit.position;
                    return true;
                }
            }
        }

        result = Vector3.zero;
        return false;
    }

    #endregion
    
    #region VISION

    // vision pass
    void VisionCheck()
    {
        // don't run vision if dead
        if (state == State.death) {
            return;
        }


        // run method once every pulse rate
        if (visionCheckElapsed < vision.pulseRate) {
            visionCheckElapsed++;
            return;
        }


        visionCheckElapsed = 0;
        List<Collider> enemiesToAttack = new List<Collider>();
        Vector3 npcDir = transform.position + centerPosition;


        // set the vision range and angle according to state
        float range, angle;
        switch (state) {
            case State.normal:
                angle = vision.visionDuringNormalState.coneAngle;
                range = vision.visionDuringNormalState.sightRange;
                break;
            case State.alert:
                angle = vision.visionDuringAlertState.coneAngle;
                range = vision.visionDuringAlertState.sightRange;
                break;
            case State.attack:
                angle = vision.visionDuringAttackState.coneAngle;
                range = vision.visionDuringAttackState.sightRange;
                break;
            case State.goingToCover:
                angle = vision.visionDuringAttackState.coneAngle;
                range = vision.visionDuringAttackState.sightRange;
                break;
            case State.returningToAlert:
                angle = vision.visionDuringAlertState.coneAngle;
                range = vision.visionDuringAlertState.sightRange;
                break;
            case State.sawAlertTag:
                angle = vision.visionDuringAlertState.coneAngle;
                range = vision.visionDuringAlertState.sightRange;
                break;
            default:
                angle = PreviousStateVAngle();
                range = PreviousStateVRange();
                break;
        }

        
        // get the hostiles and alerts
        int visionCollNum = Physics.OverlapSphereNonAlloc(transform.position, range, visionColl, vision.hostileAndAlertLayers);
        
        for (int i=0; i<visionCollNum; i++) {
            // check for alert tag
            int alertTagIndex = vision.GetAlertTagIndex(visionColl[i].tag);

            if (alertTagIndex >= 0) {
                GameObject alertObj = visionColl[i].transform.gameObject;
                
                // check if within vision angle
                if (Vector3.Angle(visionT.forward, (alertObj.transform.position - npcDir)) < (angle * 0.5f)) {
                    // check height
                    float alertheight = alertObj.transform.position.y - (centerPosition.y + visionT.position.y + vision.sightLevel + vision.maxSightLevel);
                    
                    if (alertheight < 0f) {
                        SawAlertTag(alertObj, alertTagIndex);
                    }
                }
            }


            // THE STARTING CODE FOR HOSTILE SEARCH


            if (enemiesToAttack.Count >= 5) break;
            
            // check for hostile tags
            if (System.Array.IndexOf(vision.hostileTags, visionColl[i].tag) < 0) {
                continue;
            }

            
            Collider hostile = visionColl[i];

            
            // check if not within vision angle
            if (Vector3.Angle(visionT.forward, (hostile.transform.position - npcDir)) > (angle * 0.5f))
            {
                continue;
            }

            
            // check height
            float suspectHeight = hostile.transform.position.y - (centerPosition.y + visionT.position.y + vision.sightLevel + vision.maxSightLevel);
            if (suspectHeight > 0f)
            {
                continue;
            }


            Collider[] enemyToAttackColliders = hostile.transform.GetComponentsInChildren<Collider>();
            int colSize = enemyToAttackColliders.Length;


            // set the raycast layers
            int layersToHit;
            if (state != State.attack && state != State.goingToCover) layersToHit = vision.layersToDetect | vision.hostileAndAlertLayers;
            else {
                if (coverShooterMode) layersToHit = vision.hostileAndAlertLayers;
                else layersToHit = vision.layersToDetect | vision.hostileAndAlertLayers;
            }


            // prevent adding colliders of the same gameobject
            bool exists = false;
            if (enemiesToAttack.Count > 0) {
                foreach (var coll in enemiesToAttack) {
                    if (coll.transform.IsChildOf(hostile.transform)) {
                        exists = true;
                        break;
                    }
                }
            }
            

            if (!exists) {
                // check the detection score
                if (colSize <= 2) {
                    if (RayCastObjectColliders(hostile.transform.gameObject, layersToHit, 1)) {
                        if (enemiesToAttack.Count < 5) {
                            enemiesToAttack.Add(hostile);
                        }
                    }
                }else{
                    // enemy is seen if more than half of it's colliders are seen
                    if (RayCastObjectColliders(hostile.transform.gameObject, layersToHit, colSize/2)) {
                        if (enemiesToAttack.Count < 5) {
                            enemiesToAttack.Add(hostile);
                        }
                    }
                }
            }
        }


        // if no valid enemies -> return
        if (enemiesToAttack.Count <= 0) {
            enemyToAttack = null;
            return;
        }


        // if in hit state then don't continue until the state is changed
        if (state == State.hit) {
            return;
        }
        

        // order the enemies by distance
        enemiesToAttack.Sort((x, y) => { return (transform.position - x.transform.position).sqrMagnitude.CompareTo((transform.position - y.transform.position).sqrMagnitude); });

        // target the least distance -> first item (index 0)
        enemyToAttack = enemiesToAttack[0].transform.gameObject;
        enemyColPoint = enemiesToAttack[0].ClosestPoint(enemiesToAttack[0].bounds.center);
        
        checkEnemyPosition = Vector3.zero;

        // make a timestamp
        captureEnemyTimeStamp = Time.time;


        // track distances
        distanceToEnemySqrMag = (new Vector3(enemyToAttack.transform.position.x, transform.position.y, enemyToAttack.transform.position.z) - transform.position).sqrMagnitude;
        distanceToEnemy = Vector3.Distance(new Vector3(enemyToAttack.transform.position.x, transform.position.y, enemyToAttack.transform.position.z), transform.position);


        // activate state
        if (state == State.normal) {
            Surprised();
        }
        else {
            if (state != State.distracted) {
                TurnToAttackState();
                return;
            }

            if (previousState != State.normal) {
                TurnToAttackState();
                return;
            }

            Surprised();
        }
    }

    // this method will trigger when the AI sees an alert tag
    void SawAlertTag(GameObject alertObj, int index)
    {
        // save the saw tag name and the object's position
        sawAlertTagName = alertObj.tag;
        sawAlertTagPos = ValidateYPoint(alertObj.transform.position);

        int layers = vision.layersToDetect | vision.hostileAndAlertLayers;
        
        // check if any collider is caught
        if (!RayCastObjectColliders(alertObj, layers, 1)) {
            return;
        }

        string fallBackTag;


        // check whether a fallback tag is set
        if (vision.alertTags[index].fallBackTag.Length <= 0) {
            fallBackTag = "Untagged";
        }
        else {
            fallBackTag = vision.alertTags[index].fallBackTag;
        }


        // if behaviour is empty -> tell the user
        if (vision.alertTags[index].behaviourScript == null) {
            print($"Alert Tag: {sawAlertTagName} behaviour is empty so nothing will be enabled.");
        }


        // change the tag name of the alert object to the fallback tag
        alertObj.tag = fallBackTag;

        // set the state to saw alert tag and Update() enables the corresponding behaviour
        SetState(State.sawAlertTag);
    }
    
    // check if colliders of a gameobject are seen
    bool RayCastObjectColliders(GameObject go, int layersToHit, int minDetectionScore)
    {
        Collider[] objColls = go.transform.GetComponentsInChildren<Collider>();
        int colSize = objColls.Length;
        int detectionScore = 0;
        

        // check if raycast can hit target colliders
        for (int i=0; i<colSize; i++) {
            Collider item = objColls[i];
            RaycastHit rayHit;

            Vector3 npcDir = transform.position + centerPosition;
            Vector3 colDir = item.ClosestPoint(item.bounds.center) - npcDir;

            
            // start with center raycast, if caught nothing -> top left, if caught nothing -> top right
            if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, layersToHit)) {
                if (item.transform.IsChildOf(rayHit.transform) || rayHit.transform.IsChildOf(item.transform)) {
                    detectionScore++;
                }
                else {
                    // checking top left
                    colDir = (item.ClosestPoint(item.bounds.max) - npcDir);

                    if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, layersToHit)) {
                        if (item.transform.IsChildOf(rayHit.transform) || rayHit.transform.IsChildOf(item.transform)) {
                            detectionScore++;
                        }
                        else {
                            // checking top right
                            colDir = (item.ClosestPoint(new Vector3(item.bounds.center.x - item.bounds.extents.x, item.bounds.center.y + item.bounds.extents.y, item.bounds.center.z + item.bounds.extents.z)) - npcDir);
                            
                            if (Physics.Raycast(npcDir, colDir, out rayHit, Mathf.Infinity, layersToHit)) {
                                if (item.transform.IsChildOf(rayHit.transform) || rayHit.transform.IsChildOf(item.transform)) {
                                    detectionScore++;
                                }
                            }
                        }
                    }
                }
            }
        }

        // if detection score is bigger or equal to the minimum required -> return true
        if (detectionScore >= minDetectionScore) {
            return true;
        }

        return false;
    }

    // get the vision angle of the previous state
    float PreviousStateVAngle()
    {
        if (previousState == State.normal) {
            return vision.visionDuringNormalState.coneAngle;
        }

        if (previousState == State.alert) {
            return vision.visionDuringAlertState.coneAngle;
        }

        if (previousState == State.attack) {
            return vision.visionDuringAttackState.coneAngle;
        }

        return 0;
    }

    // get the vision range of the previous state
    float PreviousStateVRange()
    {
        if (previousState == State.normal) {
            return vision.visionDuringNormalState.sightRange;
        }

        if (previousState == State.alert) {
            return vision.visionDuringAlertState.sightRange;
        }

        if (previousState == State.attack) {
            return vision.visionDuringAttackState.sightRange;
        }

        return 0;
    }

    // check for an enemy character specific radius
    GameObject CheckSurroundingForTarget()
    {
        if (state == State.attack || state == State.goingToCover || state == State.surprised) {
            return null;
        }

        checkSurroundingElapsed++;

        if (checkSurroundingElapsed < 5) {
            return null;
        }

        checkSurroundingElapsed = 0;

        int maxColliders = 10;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position + centerPosition, enemyContactRadius, hitColliders, vision.hostileAndAlertLayers);

        for(int i=0; i<numColliders; i++) {
            if (System.Array.IndexOf(vision.hostileTags, hitColliders[i].transform.tag) >= 0) {
                enemyPosOnSurprised = hitColliders[i].transform.position;
                return hitColliders[i].transform.gameObject;
            }
        }

        return null;
    }

    // check if an enemy got too close and turn to attack if so
    void SurroundingsCheck()
    {
        if (!checkEnemyContact || enemyToAttack || state == State.hit) {
            return;
        }


        GameObject closeTarget = CheckSurroundingForTarget();

        // check if an enemy got too close
        if (closeTarget == null) {
            return;
        }
            
        
        if (state == State.distracted) {
            if (previousState == State.normal) {
                Surprised();
                return;
            }

            SetEnemy(closeTarget);
            return;
        }


        if (state != State.normal) {
            SetEnemy(closeTarget);
            return;
        }


        SetEnemy(closeTarget, false);
        Surprised();
    }

    #endregion

    #region ATTACK STATE
    
    // trigger the surprised state
    void Surprised()
    {
        if (state == State.hit) {
            return;
        }

        if (!useSurprisedState) {
            TurnToAttackState();
            return;
        }

        if (enemyToAttack){
            enemyPosOnSurprised = enemyToAttack.transform.position;
        }

        SetState(State.surprised);
    }

    // turn to attack state
    public void TurnToAttackState()
    {
        if (state == State.attack || state == State.goingToCover || state == State.surprised || state == State.hit) {
            return;
        }
        
        SetState(State.attack);
    }
    
    #endregion

    #region BEHAVIOURS & STATE MANAGEMENT
    
    // enable behaviour script of current state and disable others to maintain performance
    void EnableBehaviour(MonoBehaviour passedBehaviour)
    {
        if (passedBehaviour == null || passedBehaviour == lastEnabledBehaviour) return;


        // useful if behaviour script changed programmatically then disable that previous one
        if (lastEnabledBehaviour != null) lastEnabledBehaviour.enabled = false;

    
        MonoBehaviour[] behaviours = {normalStateBehaviour, 
        alertStateBehaviour, 
        attackStateBehaviour, 
        coverShooterBehaviour, 
        goingToCoverBehaviour, 
        distractedStateBehaviour, 
        surprisedStateBehaviour, 
        hitStateBehaviour};
        

        vision.DisableAllAlertBehaviours();


        int max = behaviours.Length;
        
        for (int i=0; i<max; i++) {
            if (behaviours[i] != null) {
                if (passedBehaviour == behaviours[i]) {
                    behaviours[i].enabled = true;
                    continue;
                } 
                
                behaviours[i].enabled = false;
            }
        }


        // enable saw alert tag behaviour 
        if (state == State.sawAlertTag) {
            passedBehaviour.enabled = true;
        }


        lastEnabledBehaviour = passedBehaviour;
    }

    // set the state of the AI to passed value
    public void SetState(State stateToTurnTo)
    {
        if (!System.Enum.IsDefined(typeof(State), stateToTurnTo)) {
            print("Trying to set state to a value that is not defined.");
            return;
        }

        state = stateToTurnTo;
    }

    // disable all behaviours
    public void DisableAllBehaviours()
    {
        MonoBehaviour[] behaviours = {normalStateBehaviour, alertStateBehaviour, attackStateBehaviour, coverShooterBehaviour, goingToCoverBehaviour, distractedStateBehaviour, surprisedStateBehaviour, hitStateBehaviour};
        int max = behaviours.Length;
        
        for (int i=0; i<max; i++) {
            if (behaviours[i] != null) {
                behaviours[i].enabled = false;
            }
        }

        lastEnabledBehaviour = null;
    }

    void RemoveMoveToLocation()
    {
        if (state != State.alert && state != State.normal) {
            IgnoreMoveToLocation();
        }
    }
    
    #endregion

    #region CHARACTER
    
    void ShowCenterPosition()
    {
        if (!showCenterPosition) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + centerPosition, 0.1f);
    }

    void ShowEnemyContactRadius()
    {
        if (!showEnemyContactRadius || !checkEnemyContact) {
            return;
        }

        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position + centerPosition, enemyContactRadius);
    }
    
    #endregion

    #region AUDIOS
    
    public bool IsAudioScriptableEmpty()
    {
        if (audioScriptable == null) {
            print("A behaviour checked for an audio scriptable to play an audio but the property was empty.");
            return true;
        }

        return false;
    }

    // play a passed audio
    public bool PlayAudio(AudioClip audio) 
    {
        // if passed audio is null -> return
        if (audio == null) {
            return false;
        }


        // same audio is playing -> return
        if (audio == agentAudio.clip && agentAudio.isPlaying) {
            return true;
        }


        agentAudio.Stop();
        agentAudio.clip = audio;
        agentAudio.Play();


        return true;
    }

    public void StopAudio()
    {
        agentAudio.Stop();
    }

    #endregion

    #region PUBLIC METHODS
    
    // force the AI to move to a specified location
    public void MoveToLocation(Vector3 location)
    {
        if (state != State.normal && state != State.alert) {
            print("MoveToLocation() only works when the AI is in normal and alert states.");
            return;
        }
        
        // these two properties will be read by the behaviours
        endDestination = ValidateYPoint(location);
        movedToLocation = true;
    }

    // ignore the forcing of movement to a certain location
    public void IgnoreMoveToLocation()
    {
        movedToLocation = false;
    }

    // force the AI to go idle
    public void StayIdle()
    {
        if (state != State.normal && state != State.alert && state != State.distracted) {
            print("StayIdle() only works when the AI is in normal, alert and distracted states.");
            return;
        }

        // this public property will be read by the behaviours
        stayIdle = true;
    }

    // check whether the AI is idle or not
    public bool IsIdle()
    {
        if (state != State.normal && state != State.alert && state != State.distracted) {
            print("IsIdle() only works when the AI is in normal, alert and distracted states. Will return false.");
            return false;
        }

        // set by the behaviours
        return isIdle;
    }

    // force to attack target
    public void Attack()
    {
        if (state == State.death || !enabled) {
            print("Attack() can't be called when the AI is in death state or Blaze AI disabled.");
            return;
        }   

        if (enemyToAttack == null) {
            print("Attack() can't be called when the AI doesn't have a target.");
            return;
        }


        isAttacking = true;
        

        if (state != State.hit) {
            SetState(State.attack);
        }
    }

    // cancel current attack
    public void StopAttack()
    {
        isAttacking = false;
    }
    
    // change between normal and alert states only
    public void ChangeState(string state)
    {
        enabled = true;

        // just in case if user is respawning the AI from death
        navmeshAgent.enabled = true;

        // read by alert and normal state behaviours -> to avoid setting new destination
        changedState = true;


        if (state == "normal") {
            SetState(State.normal);
        }

        if (state == "alert") {
            SetState(State.alert);
        }
    }

    // set an enemy and turn to attack state
    public void SetEnemy(GameObject enemy, bool turnStateToAttack=true) 
    {
        if (state == State.death || !enabled) {
            print("Can't call SetEnemy() when AI is in death state or Blaze AI is disabled.");
            return;
        }


        if (enemyToAttack && enemy) {
            if (!enemyToAttack.transform.IsChildOf(enemy.transform)) {
                print("Can't call SetEnemy() when there's already a target chosen by the AI.");
                return;
            }
        }
        

        if (enemy == null) {
            ChangeState("alert");
            return;
        }
        

        // check and set path of enemy
        if (IsPathReachable(enemy.transform.position)) {
            checkEnemyPosition = enemy.transform.position;
        }
        else {
            Vector3 point;
            if (ClosestNavMeshPoint(enemy.transform.position, Vector3.Distance(enemy.transform.position, transform.position), out point)) {
                checkEnemyPosition = point;
            }
            else {
                ChangeState("alert");
                return;
            }
        }


        enemyToAttack = enemy;
        enemyColPoint = enemy.transform.position;
        

        if (turnStateToAttack) {
            SetState(State.attack);
        }
    }

    // hit the AI
    public void Hit(GameObject enemy=null) 
    {
        if (state == State.death || !enabled) {
            print("Hit() can't be called when the AI is in death state or Blaze AI is disabled.");
            return;
        }

        // read by the hit state behaviour
        hitEnemy = enemy;
        hitRegistered = true;

        SetState(State.hit);
    }

    // kill the AI
    public void Death()
    {
        if (state == State.death || !enabled) {
            print("Death() can't be called when the AI is in death state or Blaze AI is disabled.");
            return;
        }


        SetState(State.death);


        lastEnabledBehaviour = null;
        enemyToAttack = null;
        navmeshAgent.enabled = false;
        
        
        DisableAllBehaviours();
        vision.DisableAllAlertBehaviours();


        animManager.Play(deathAnim, deathAnimT);

        
        if (!IsAudioScriptableEmpty() && playDeathAudio) {
            PlayAudio(audioScriptable.GetAudio(AudioScriptable.AudioType.Death));
        }

        
        if (disableBlazeOnDeath) {
            enabled = false;
        }
    }
    
    #endregion
}