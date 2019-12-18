using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

/*
[Script Header] CustomNavMeshAgent Version 0.0.1
Created by: Thiebaut Alexis 
Date: 14/01/2019
Description: - Agent of the customNavMesh, they can Follow a path stocked in their CustomNavPath
             - They can check if their path can be compute before following a path
             - They have an offset and a size that allow them to be on the navmesh

///
[UPDATES]
Update n°: 001
Updated by: Thiebaut Alexis 
Date: 14/01/2019
Description: Rebasing the agent on a previously created agent

Update n°: 002
Updated By Thiebaut Alexis
Date: 21/01/2019
Description: Try to add steering to the agent

Update n°: 003
Updated by: Thiebaut Alexis
Date: 25/01/2019 - 31/01/2019
Description: Adding Steering Behaviour to the agent
             - The path is now smoothed
             - The agent can't avoid each other. Still have to implement the agent avoidance

Update n°: 004
Updated by: Thiebaut Alexis
Date: 04/02/2019 
Description: Fixing Offseted Positions errors

Update n°: 005
Updated by: Thiebaut Alexis
Date: 05/02/2019 
Description: Completing Stop Agent Method

Update n°: 006
Updated by: Thiebaut Alexis
Date: 07/02/2019 
Description: Updating the CheckDestination and SetDestination Methods
                - When the agent is already moving, stop the coroutine and calculate a new path

Update n°: 007
Updated by: Thiebaut Alexis
Date: 13/02/2019 
Description: Updating the Avoid Method -> Now add the the avoidance direction to the velocity
             Set set speed Property -> The speed can now be set from another script
             Adding a event OnAgentStopped

Update n°: 008
Updated by: Thiebaut Alexis
Date: 19/03/2019 
Description: Implementing the detection and the avoidance behaviours
             > Detection Behaviour cast multiple rays in direction of the velocity and get the obstacles touched by the rays
             > Avoid Behaviour use the obstacle get from the detection and avoid them
             
Update n°: 009
Updated by: Thiebaut Alexis
Date: 13/02/2019 
Description: Implementing layer mask to exclude certain layers from being detected and avoided

Update n°: 010
Updated by: Thiebaut Alexis
Date: 10/04/2019
Description: Implementing new event OnAgentReady

Update n°: 011
Updated by: Thiebaut Alexis
Date: 27/05/2019
Description: 
+Implementing a new float: pathRadius: used to set the precision of the stifness to follow the path 
+Correcting the case when the agent goes in the wrong direction along the path using the scalar product 

Update n°: 012
Updated by: Thiebaut Alexis
Date: 27/06/2019
Description: 
+Implementing AddAvoidanceLayer and RemoveAvoidanceLayer methods

Update n°: 012
Updated by: Thiebaut Alexis
Date: 01/07/2019
Description: 
+Improving the Avoid Method
*/
public class CustomNavMeshAgent : MonoBehaviour
{
    #region Events
    public event Action OnAgentReady;
    public event Action OnMovementStarted;
    public event Action OnDestinationReached;
    public event Action OnAgentStopped;
    #endregion

    #region FieldsAndProperty

    #region Inspector

    #region Vector3
    [SerializeField] private Vector3 positionOffset = Vector3.zero;
    #endregion

    #region float
    [SerializeField, Range(1, 90)] protected float avoidanceForce = 2;
    public float AvoidanceForce
    {
        get
        {
            return (Mathf.Tan(Mathf.Deg2Rad * avoidanceForce) * speed); 
        }
    }

    [SerializeField, Range(-5, 5)] protected float baseOffset = 0;

    [SerializeField, Range(.1f, 10)] protected float detectionRange = 2;

    [SerializeField, Range(.1f, 5)] protected float height = 1;
    public float Height { get { return height / 2; } }

    [SerializeField, Range(.5f, 5)] protected float radius = 1;
    public float Radius
    {
        get
        {
            return radius;
        }
        set
        {
            if (value > 0) radius = value;
        }
    }

    [SerializeField, Range(.1f, 5)] protected float pathRadius = 1;
    public float PathRadius
    {
        get
        {
            return pathRadius;
        }
        set
        {
            if (value > 0)
                pathRadius = value;
        }
    }

    [SerializeField, Range(.1f, 10)] protected float speed = 1;
    public float Speed
    {
        get
        {
            return speed;
        }
        set
        {
            if (value >= 0)
                speed = value;
        }
    }

    [SerializeField, Range(1,89)] protected float steerForce = .1f;
    #endregion

    #region Int
    [SerializeField, Range(1, 10)] protected int agentPriority = 1;
    public int AgentPriority { get { return agentPriority; } }

    [SerializeField, Range(3, 10)] protected int detectionAccuracy = 3;

    [SerializeField, Range(20, 180)] protected int detectionFieldOfView = 90;
    #endregion

    [SerializeField] private LayerMask avoidanceLayer;
    #endregion

    #region Other Fields and properties
    #region bool
    bool isMoving = false;
    public bool IsMoving { get { return isMoving; } }
    #endregion

    #region Path
    private CustomNavPath currentPath = new CustomNavPath();
    public CustomNavPath CurrentPath { get { return currentPath; } }
    #endregion

    #region CalculatingState
    private CalculatingState pathState = CalculatingState.Waiting;
    public CalculatingState PathState { get { return pathState; } }
    #endregion

    #region Vector3
    public Vector3 CenterPosition { get { return transform.position + positionOffset; } }
    public Vector3 OffsetSize { get { return new Vector3(radius, height, radius); } }
    public Vector3 OffsetPosition
    {
        get
        {
            Vector3 _heightOffset = new Vector3(0, (height / 2) + baseOffset, 0);
            return CenterPosition - _heightOffset;
        }
    }

    public Vector3 LastPosition
    {
        get
        {
            if (currentPath.PathPoints.Count == 0) return OffsetPosition;
            return currentPath.PathPoints.Last();
        }
    }

    private Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } }
    #endregion

    #region Vector3[]
    private Vector3[] fieldOfView = null;
    #endregion
    #endregion

    #endregion

    #region Methods
    /// <summary>
    /// Add the layers to the avoidanceLayer using their names
    /// </summary>
    /// <param name="_layerNames">Names of the added layers</param>
    public void AddAvoidanceLayer(string[] _layerNames) => AddAvoidanceLayer(LayerMask.GetMask(_layerNames));

    /// <summary>
    /// Add the layers to the avoidanceLayer using their int value
    /// </summary>
    /// <param name="_maskValue">int value of the mask(s)</param>
    public void AddAvoidanceLayer(int _maskValue)
    {
        if (_maskValue == 0) return;
        avoidanceLayer |= _maskValue;
    }

    /// <summary>
    /// Apply the avoidance force to the velocity
    /// Avoidance force is equal to the direction from the center position of the obstacle to the hit point of the ray cast
    /// </summary>
    /// <param name="_direction">Direction from the center position of the obstacle to the hit point of the ray cast</param>
    private void Avoid(Vector3 _direction)
    {
        _direction.Normalize();
        // Avoidance = Tan(avoidanceForce) * velocity * time.deltatime
        Vector3 _avoidance = _direction * AvoidanceForce * Time.fixedDeltaTime;
        Debug.DrawLine(transform.position + velocity, transform.position + velocity + _avoidance, Color.red, 1); 
        velocity += _avoidance;
        velocity = Vector3.ClampMagnitude(velocity, speed);
    }

    /// <summary>
    /// Check if the destination can be reached
    /// </summary>
    /// <param name="_position">destination to reach</param>
    /// <returns>if the destination can be reached</returns>
    public bool CheckDestination(Vector3 _position)
    {
        if (CustomNavMeshManager.Triangles == null || CustomNavMeshManager.Triangles.Count == 0)
        {
            Debug.LogWarning("Triangles Not found. Must build the navmesh for the scene");
            return false;
        }
        if (isMoving)
        {
            StopAllCoroutines();
        }
        pathState = CalculatingState.Calculating;
        bool _canBeReached = PathCalculator.CalculatePath(OffsetPosition, _position, currentPath, CustomNavMeshManager.Triangles);
        if (_canBeReached)
        {
            pathState = CalculatingState.Ready;
            StopAllCoroutines();
            StartCoroutine(FollowPath());
        }
        else pathState = CalculatingState.Waiting;
        return _canBeReached;
    }

    /// <summary>
    /// Make the agent follows the path
    /// </summary>
    /// <returns></returns>
    IEnumerator FollowPath()
    {
        OnMovementStarted?.Invoke();
        isMoving = true;
        int _pathIndex = 1;
        List<Vector3> _followingPath = currentPath.PathPoints;  //List of points in the path

        /*STEERING*/
        //Predicted position is the position of the agent at the next frame
        Vector3 _predictedPosition;


        // Previous Position
        Vector3 _previousPosition = OffsetPosition;
        //Next Position
        Vector3 _nextPosition = _followingPath[1];


        Vector3 _dir;
        Vector3 _targetPosition;
        Vector3 _normalPoint;


        float _distance = 0;
        // Angle theta is the angle between forward and velocity direction
        float _theta;
        float _scalarProduct;

        RaycastHit _hitInfo;
        Ray _ray;
        // List of directions to apply on the avoidance 
        List<Vector3> _obstaclesPos = new List<Vector3>();

        
        /* First the velocity is equal to the normalized direction from the agent position to the next position */
        /* LEGACY 
        if (velocity == Vector3.zero)
            velocity = (_nextPosition - OffsetPosition) * speed;
        Seek(_nextPosition);
        */

        while (Vector3.Distance(OffsetPosition, LastPosition) > radius)
        {
            /* Apply the velocity to the transform position multiply by the speed and by Time.deltaTime to move*/
            velocity = velocity.normalized * speed;
            velocity = Vector3.ClampMagnitude(velocity, speed);
            transform.position += velocity * Time.deltaTime;

            /* If the agent is close to the next position
             * Update the previous and the next point
             * Also update the pathIndex
             * if the pathindex is greater than the pathcount break the loop
             * else continue in the loop
             */
            if (Vector3.Distance(OffsetPosition, _nextPosition) <= radius)
            {
                //set the new previous position
                _previousPosition = _followingPath[_pathIndex];
                //Increasing path index
                _pathIndex++;
                if (_pathIndex > _followingPath.Count - 1) break;
                //Set the new next Position
                _nextPosition = _followingPath[_pathIndex];
                Seek(_nextPosition);
                continue;
            }
            // Theta is equal to the angle between the velocity and the forward vector
            _theta = Vector3.SignedAngle(Vector3.forward, velocity, Vector3.up);

            //Cast each ray of the fieldOfView Array
            for (int i = 0; i < fieldOfView.Length; i++)
            {
                // Apply a offset rotation on the direction equal to the angle between the forward and the velocity (theta angle)
                _ray = new Ray(OffsetPosition,
                    new Vector3(Mathf.Sin(_theta * Mathf.Deg2Rad) * fieldOfView[i].z + Mathf.Cos(_theta * Mathf.Deg2Rad) * fieldOfView[i].x,
                    0,
                    Mathf.Cos(_theta * Mathf.Deg2Rad) * fieldOfView[i].z - Mathf.Sin(_theta * Mathf.Deg2Rad) * fieldOfView[i].x));
                //Cast the ray
                if (Physics.Raycast(_ray, out _hitInfo, detectionRange, avoidanceLayer.value))
                {
                    //If the hit object isn't a navsurface's child or the agent itself
                    if (!_hitInfo.transform.GetComponentInParent<NavMeshSurface>() && _hitInfo.collider.gameObject != this.gameObject)
                    {
                        // Add the direction to avoid to the list
                        _dir = velocity - (_hitInfo.point - transform.position);
                        _dir.y = 0;
                        _obstaclesPos.Add(_dir);
                    }
                }
            }
            //If the obstacle position contains directions to follow in order to avoid the obstacle
            if (_obstaclesPos.Count > 0)
            {
                // Get the average direction to follow and avoid in this direction
                Vector3 _v = Vector3.zero;
                _obstaclesPos.ForEach(p => _v += p);
                Avoid(_v);
                _obstaclesPos = new List<Vector3>();
                yield return null;
                continue;
            }
            /* Get the predicted Velocity and the Predicted position*/
            _predictedPosition = OffsetPosition + velocity;

            /*Get the transposed Position of the predicted position on the segment between the previous and the next point
            * The agent has to get closer while it's to far away from the path 
            */
            _normalPoint = GeometryHelper.GetNormalPoint(_predictedPosition, _previousPosition, _nextPosition);


            /* Direction of the segment between the previous and the next position normalized in order to go further on the path
             * Targeted position is the normal point + an offset defined by the direction of the segment to go a little further on the path
             * If the target is out of the segment between the previous and the next position, the target position is the next position
             */
            _dir = (_nextPosition - _previousPosition).normalized;
            _targetPosition = _normalPoint + _dir;

            /* Distance between the predicted position and the normal point on the segment 
            * If the distance is greater than the radius, it has to steer to get closer
            */
            _distance = Vector3.Distance(_predictedPosition, _normalPoint);
            _scalarProduct = Vector3.Dot(velocity.normalized, _dir.normalized);
            if (_distance > pathRadius / 2 || _scalarProduct == -1 || velocity == Vector3.zero)
            {
                Seek(_targetPosition);
            }
            yield return null; 
        }
        StopAgent();
        OnDestinationReached?.Invoke();
    }

    /// <summary>
    /// Generate the rays of the field of view
    /// Base on the angle of the field of view, its accuracy
    /// </summary>
    private void GenerateFieldOfView()
    {
        fieldOfView = new Vector3[detectionAccuracy];
        float _angle = -(detectionFieldOfView / 2);
        float _offset = detectionFieldOfView / (detectionAccuracy - 1);
        Vector3 _point;
        for (int i = 0; i < detectionAccuracy; i++)
        {
            _point = new Vector3(Mathf.Sin(_angle * Mathf.Deg2Rad), 0, Mathf.Cos(_angle * Mathf.Deg2Rad)).normalized;
            fieldOfView[i] = _point;
            _angle += _offset;
        }
    }

    /// <summary>
    /// Remove the layers from the avoidanceLayer using their names
    /// </summary>
    /// <param name="_layerNames">Names of the removed layers</param>
    public void RemoveAvoidanceLayer(string[] _layerNames) => RemoveAvoidanceLayer(LayerMask.GetMask(_layerNames));

    /// <summary>
    /// Remove the layers from the avoidanceLayer using their int value
    /// </summary>
    /// <param name="_maskValue">int value of the mask(s)</param>
    public void RemoveAvoidanceLayer(int _maskValue)
    {
        if (_maskValue == 0) return;
        LayerMask _inverted = ~avoidanceLayer;
        _inverted |= _maskValue;
        avoidanceLayer = ~_inverted;
    }

    /// <summary>
    /// Calculate the needed velocity 
    /// Desired velocity - currentVelocity
    /// </summary>
    /// <param name="_target"></param>
    private void Seek(Vector3 _target)
    {
        Vector3 _desiredVelocity = (_target - OffsetPosition).normalized * speed;

        Vector3 _steer = (_desiredVelocity - velocity) * Mathf.Tan(Mathf.Deg2Rad * steerForce) * Time.fixedDeltaTime;
        velocity += _steer;
        velocity = Vector3.ClampMagnitude(velocity, speed);
    }

    /// <summary>
    /// Calculate a path until reaching a destination
    /// </summary>
    /// <param name="_position">destination to reach</param>
    public void SetDestination(Vector3 _position)
    {
        if (CustomNavMeshManager.Triangles == null || CustomNavMeshManager.Triangles.Count == 0)
        {
            Debug.LogWarning("Triangles Not found. Must build the navmesh for the scene");
            //Destroy(this);
            return;
        }
        if (isMoving)
        {
            StopAllCoroutines();
        }
        pathState = CalculatingState.Calculating;
        if (PathCalculator.CalculatePath(OffsetPosition, _position, currentPath, CustomNavMeshManager.Triangles))
        {
            pathState = CalculatingState.Ready;
            StartCoroutine(FollowPath());
        }
    }

    /// <summary>
    /// Stop the agent
    /// Stop the coroutine and reset all settings
    /// </summary>
    public void StopAgent()
    {
        StopCoroutine(FollowPath());
        currentPath.PathPoints.Clear();
        isMoving = false;
        pathState = CalculatingState.Waiting;
        OnAgentStopped?.Invoke();
    }
    #endregion

    #region UnityMethods
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(CenterPosition, .1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(CenterPosition + velocity, .1f);
        Gizmos.DrawLine(CenterPosition, CenterPosition + velocity);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(OffsetPosition, .1f);
        Gizmos.color = Color.cyan;
        if (currentPath == null || currentPath.PathPoints == null || currentPath.PathPoints.Count == 0) return;
        for (int i = 0; i < currentPath.PathPoints.Count; i++)
        {
            Gizmos.DrawSphere(currentPath.PathPoints[i], .2f);
        }
        for (int i = 0; i < currentPath.PathPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(currentPath.PathPoints[i], currentPath.PathPoints[i + 1]);
        }
    }
    private void Awake()
    {
        GenerateFieldOfView();
        OnAgentReady?.Invoke();
    }
    #endregion
}
public enum CalculatingState
{
    Waiting,
    Calculating,
    Ready
}