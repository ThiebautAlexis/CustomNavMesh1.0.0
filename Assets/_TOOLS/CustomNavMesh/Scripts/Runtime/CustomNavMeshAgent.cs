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
*/
public class CustomNavMeshAgent : MonoBehaviour
{
    #region Events
    public event Action OnDestinationReached;
    #endregion

    #region FieldsAndProperty

    #region bool
    bool isMoving = false;
    public bool IsMoving { get { return isMoving; } }
    #endregion

    #region float
    [Header("Agent Settings")]
    [SerializeField, Range(.1f, 5)] float height = 1;
    public float Height { get { return height / 2; } }

    [SerializeField, Range(.5f, 2)] float radius = 1;
    public float Radius { get { return radius * .75f; } }

    [SerializeField, Range(-5, 5)] float offset = 0;

    [SerializeField, Range(.5f, 10)] float speed = 1;

    [SerializeField, Range(1, 15)] float detectionRange = 5;
    #endregion

    #region Path
    [SerializeField] private CustomNavPath currentPath = new CustomNavPath();
    public CustomNavPath CurrentPath { get { return currentPath; } }
    #endregion

    #region CalculatingState
    private CalculatingState pathState = CalculatingState.Waiting;
    public CalculatingState PathState { get { return pathState; } }
    #endregion

    #region Vector3
    public Vector3 OffsetSize { get { return new Vector3(radius, height, radius); } }
    public Vector3 OffsetPosition { get { return new Vector3(0, (height / 2) + offset, 0); } }

    public Vector3 LastPosition { get { return currentPath.PathPoints.Last() + OffsetPosition; } }
    public Vector3 TargetedPosition { get { return currentPath.PathPoints.First() + OffsetPosition; } }
    #endregion 
    #endregion

    #region Methods
    /// <summary>
    /// Calculate a path until reaching a destination
    /// </summary>
    /// <param name="_position">destination to reach</param>
    public void SetDestination(Vector3 _position)
    {
        pathState = CalculatingState.Calculating;
        if (PathCalculator.CalculatePath(transform.position, _position, currentPath, CustomNavMeshManager.Instance.Triangles))
        {
            pathState = CalculatingState.Ready;
            StartCoroutine(FollowPath());
        }
    }

    /// <summary>
    /// Check if the destination can be reached
    /// </summary>
    /// <param name="_position">destination to reach</param>
    /// <returns>if the destination can be reached</returns>
    public bool CheckDestination(Vector3 _position)
    {
        pathState = CalculatingState.Calculating;
        bool _canBeReached = PathCalculator.CalculatePath(transform.position, _position, currentPath, CustomNavMeshManager.Instance.Triangles);
        if (_canBeReached)
        {
            pathState = CalculatingState.Ready;
            StartCoroutine(FollowPath());
        }
        else pathState = CalculatingState.Waiting;
        return _canBeReached;
    }

    /// <summary>
    /// Make the agent follows the path
    /// </summary>
    /// <param name="_speed">speed</param>
    /// <returns></returns>
    IEnumerator FollowPath()
    {
        isMoving = true;
        //RaycastHit _hit;
        /* STEERING 
        Vector3 _desiredVelocity = Vector3.Normalize(_pathToFollow.First() - transform.position) * speed;
        Vector3 _currentVelocity = Vector3.Normalize(transform.forward) * speed;
        Vector3 _seek = Vector3.zero;
        Vector3 _avoidanceForce = Vector3.zero;
        Vector3 _steering = Vector3.zero; 
        */

        while (Vector3.Distance(transform.position, LastPosition) > radius)
        {
            /* STEERING
            _hit = default(RaycastHit);
            _avoidanceForce = Vector3.zero;
            */

            if (Vector3.Distance(transform.position, TargetedPosition) <= Height)
            {
                currentPath.PathPoints.RemoveAt(0);
                //_desiredVelocity = Vector3.Normalize(_pathToFollow.First() - transform.position) * speed;
                continue;
            }

            /*STEERING
            _currentVelocity = Vector3.Normalize(transform.forward) * speed;
            _seek = _desiredVelocity - _currentVelocity; 
            //if(Physics.Raycast(new Ray(transform.position, transform.forward), out _hit, detectionRange) && _hit.collider.GetComponent<CustomNavMeshAgent>())
            //{
            //    _avoidanceForce = _hit.point - _hit.transform.position; 
            //}
            _steering = _seek + _avoidanceForce; 
            transform.position = Vector3.MoveTowards(transform.position, transform.position + _steering + OffsetPosition , Time.deltaTime * _speed);
            //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, _steering + OffsetPosition, Time.deltaTime * _speed);
            */
            transform.position = Vector3.MoveTowards(transform.position, TargetedPosition, Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        pathState = CalculatingState.Waiting;
        isMoving = false;
        OnDestinationReached?.Invoke();
    }
    #endregion

    #region UnityMethods
    private void Start()
    {
    }

    private void Update()
    {
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position - OffsetPosition, .1f);
        if (currentPath == null || currentPath.PathPoints == null || currentPath.PathPoints.Count == 0) return;
        for (int i = 0; i < currentPath.PathPoints.Count; i++)
        {
            Gizmos.DrawSphere(currentPath.PathPoints[i], .2f);
        }
        Gizmos.DrawLine(transform.position - OffsetPosition, currentPath.PathPoints.First());
        for (int i = 0; i < currentPath.PathPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(currentPath.PathPoints[i], currentPath.PathPoints[i + 1]);
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