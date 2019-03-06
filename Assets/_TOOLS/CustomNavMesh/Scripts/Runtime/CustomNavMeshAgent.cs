using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


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
*/
public class CustomNavMeshAgent : MonoBehaviour
{
    #region Events
    public event Action OnMovementStarted; 
    public event Action OnDestinationReached;
    public event Action OnAgentStopped; 
    #endregion

    #region FieldsAndProperty
    #region Inspector

    #region Vector3
    [SerializeField] private Vector3 positionOffset;
    #endregion

    #region float
    [SerializeField, Range(.1f, 5)] protected float height = 1;
    public float Height { get { return height / 2; } }

    [SerializeField, Range(.5f, 2)] protected float radius = 1;
    public float Radius { get { return radius * .75f; } }

    [SerializeField, Range(-5, 5)] protected float baseOffset = 0;

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

    [SerializeField, Range(.1f, 10)] protected float avoidanceRange = 2;

    [SerializeField, Range(.1f, 10)] protected float steerForce = .1f;

    [SerializeField, Range(.1f, 10)] protected float avoidanceForce = 2;

    [SerializeField, Range(1, 10)] protected int agentPriority = 1; 
    public int AgentPriority { get { return agentPriority;  } }
    #endregion

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
            return  CenterPosition - _heightOffset ;
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
    public Vector3 Velocity { get { return velocity;} }
    #endregion
    #endregion
    #endregion

    #region Methods
    /// <summary>
    /// Check if the destination can be reached
    /// </summary>
    /// <param name="_position">destination to reach</param>
    /// <returns>if the destination can be reached</returns>
    public bool CheckDestination(Vector3 _position)
    {
        if(CustomNavMeshManager.Triangles == null || CustomNavMeshManager.Triangles.Count == 0)
        {
            Debug.LogError("Triangles Not found. Must build the navmesh for the scene");
            Destroy(this);
            return false; 
        }
        if(isMoving)
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


        // Magnitude of the normal from the dir b reaching the predicted location
        float _distance = 0;

        CustomNavMeshAgent[] _agents; 

        /* First the velocity is equal to the normalized direction from the agent position to the next position */
        if (velocity == Vector3.zero)
            velocity = transform.forward * speed;
        Seek(_nextPosition); 

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
            if (_distance > radius/2)
            {
                Seek(_targetPosition);
            }
            /* Check if there is any agent near of the agent*/
            _agents = Physics.OverlapSphere(CenterPosition, avoidanceRange).Where(c => c.GetComponent<CustomNavMeshAgent>() && c.gameObject != gameObject).Select(c => c.GetComponent<CustomNavMeshAgent>()).ToArray();
            if (_agents.Length > 0)
            {
                _dir = Vector3.zero;
                for (int i = 0; i < _agents.Length; i++)
                {
                    if (agentPriority <= _agents[i].AgentPriority)
                        _dir += (CenterPosition - _agents[i].transform.position);
                }
                Avoid(_dir);
            }
            yield return new WaitForEndOfFrame();
        }
        StopAgent(); 
        OnDestinationReached?.Invoke();
    }

    /// <summary>
    /// Apply the avoidance force to the velocity
    /// Avoidance force is equal to the direction from the center position of the obstacle to the hit point of the ray cast
    /// </summary>
    /// <param name="_direction">Direction from the center position of the obstacle to the hit point of the ray cast</param>
    void Avoid(Vector3 _direction)
    {
        _direction.Normalize();
        Vector3 _avoidance = _direction * avoidanceForce * Time.deltaTime;
        _avoidance.y = 0; 
        velocity += _avoidance;
        velocity = Vector3.ClampMagnitude(velocity, speed);
    }

    /// <summary>
    /// Calculate the needed velocity 
    /// Desired velocity - currentVelocity
    /// </summary>
    /// <param name="_target"></param>
    void Seek(Vector3 _target)
    {
        Vector3 _desiredVelocity = (_target - OffsetPosition).normalized * speed;
        Vector3 _steer = ((_desiredVelocity - velocity) * steerForce * Time.deltaTime );
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
            Debug.LogError("Triangles Not found. Must build the navmesh for the scene");
            Destroy(this);
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
        Gizmos.DrawLine(CenterPosition, CenterPosition + velocity );
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(OffsetPosition, .1f);
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
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit _hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
            {
                float _randomX = UnityEngine.Random.Range(-.5f, .5f);
                float _randomZ = UnityEngine.Random.Range(-.5f, .5f);
                Vector3 _destination = _hit.point + new Vector3(_randomX, 0, _randomZ); 
                CheckDestination(_destination);
            }
        }
    }

    #endregion
}
public enum CalculatingState
{
    Waiting,
    Calculating,
    Ready
}