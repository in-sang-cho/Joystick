using UnityEngine;
using System.Collections.Generic;

namespace BlazeAISpace
{
    [System.Serializable]
    public class Waypoints
    {
        [Tooltip("Locations of the waypoints in world space. Will appear as orange spheres at agent's location to tweak their locations visually but the [Randomize] property must be set to off. If Randomize is set to off the waypoints CAN NOT be 0 and will add current agent position as the first waypoint.")]
        public Vector3[] waypoints;
        [Tooltip("Set the idle rotation for each waypoint. Set the turning animations below. The rotation direction is shown in the scene view as red squares along the waypoints. If both the x and y are 0 then no rotation will occur and no red squares will appear. THIS GETS SET AUTOMATICALLY BASED ON NUMBER OF WAYPOINTS.")]
        public Vector2[] waypointsRotation;
        [Min(0), Tooltip("The amount of time in seconds to pass before turning to waypoint rotation.")]
        public float timeBeforeTurning = 0.2f;
        [Tooltip("Turning speed of waypoints rotations.")]
        public float turnSpeed = 2f;

        [Space(5)]
        [Tooltip("Setting this to true will loop the waypoints when patrolling, setting it to false will stop at the last waypoint.")]
        public bool loop = false;
        
        [Space(5), Tooltip("Enabling randomize will instead generate randomized waypoints within a radius from the start position in a continuous fashion and won't use the pre-set waypoints.")]
        public bool randomize = true;
        [Min(0), Tooltip("The radius from the start position to get a randomized position.")]
        public float randomizeRadius = 20f;
        [Tooltip("Shows the radius as a yellow sphere in the scene view.")]
        public bool showRandomizeRadius;

        [Space(5)]
        [Tooltip("The animation state name that will be called for turning right in normal state. If empty no animation will be played.")]
        public string rightTurnAnimNormal;
        [Tooltip("The animation state name that will be called for turning left in normal state. If empty no animation will be played.")]
        public string leftTurnAnimNormal;
        [Tooltip("The animation state name that will be called for turning right in alert state. If empty no animation will be played.")]
        public string rightTurnAnimAlert;
        [Tooltip("The animation state name that will be called for turning left in alert state. If empty no animation will be played.")]
        public string leftTurnAnimAlert;
        [Tooltip("Transition time from any state to the turning animation.")]
        public float turningAnimT = 0.25f;

        [Space(5), Tooltip("Movement turning will make the AI when in normal-alert states turn to the correct direction before moving and always turn to face the correct path. The turn speed is the property found above.")]
        public bool useMovementTurning = false;
        [Range(-1f, 1f), Tooltip("Movement turning will be used if the dot product between path corner and current position is equals to or less than this value. Best to keep it between 0.5 - 0.7.")]
        public float movementTurningSensitivity = 0.7f;

        // save inspector states
        bool inspectorLoop;
        bool inspectorRandomize;


        // GUI validation for the waypoint system
        public void WaypointsValidation(Vector3 position) 
        {
            if (randomize && loop) {
                randomize = !inspectorRandomize;
                loop = !inspectorLoop;
            }
            
            inspectorLoop = loop;
            inspectorRandomize = randomize;

            
            if (!randomize) {
                showRandomizeRadius = false;

                // if randomize is off and no waypoints -> set current position as waypoint
                if (waypoints.Length <= 0) {
                    waypoints = new Vector3[1];
                    waypoints[0] = position;

                    waypointsRotation[0] = Vector2.zero;

                    return;
                }
            }


            for (int i=0; i<waypoints.Length; i+=1) {
                if (waypoints[i] == Vector3.zero) {
                    waypoints[i] = position;
                }
            }


            if (waypointsRotation != null) {
                Vector2[] arrCopy;
                arrCopy = new Vector2[waypointsRotation.Length];

                waypointsRotation.CopyTo(arrCopy, 0);
                waypointsRotation = new Vector2[waypoints.Length];

                for (int i=0; i<waypointsRotation.Length; i+=1) {
                    if (i <= arrCopy.Length-1) {
                        waypointsRotation[i] = arrCopy[i];
                        if (waypointsRotation[i].x > 0.5f) waypointsRotation[i].x = 0.5f;
                        if (waypointsRotation[i].y > 0.5f) waypointsRotation[i].y = 0.5f;
                        if (waypointsRotation[i].x < -0.5f) waypointsRotation[i].x = -0.5f;
                        if (waypointsRotation[i].y < -0.5f) waypointsRotation[i].y = -0.5f;
                    }
                }
            }
        }

        // Mark the waypoints in editor-view
        public void Draw(Vector3 position)
        {
            if (randomize) {
                if (showRandomizeRadius) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(position, randomizeRadius);
                }
                return;
            }

            for (int i = 0; i < waypoints.Length; i++) {
                
                if (i == 0) {
                    Gizmos.color = new Color(1f, 0.3f, 0f);
                }else{
                    Gizmos.color = new Color(1f, 0.6f, 0.0047f);
                }
                
                Gizmos.DrawSphere(waypoints[i], 0.3f);
                
                // Draws the waypoint rotation cubes
                if (waypointsRotation[i].x != 0 || waypointsRotation[i].y != 0) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(new Vector3(waypoints[i].x + waypointsRotation[i].x, waypoints[i].y, waypoints[i].z + waypointsRotation[i].y), new Vector3(0.3f, 0.3f, 0.3f));
                }

                if (waypoints.Length > 1)
                {
                    Gizmos.color = Color.blue;
                    if (i == 0)
                    {
                        Gizmos.DrawLine(waypoints[0], waypoints[1]);

                    }
                    else if (i == waypoints.Length - 1)
                    {
                        Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                        Gizmos.color = Color.grey;
                        Gizmos.DrawLine(waypoints[waypoints.Length - 1], waypoints[0]);
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                    } 
                }
            }
        }
    }
}
