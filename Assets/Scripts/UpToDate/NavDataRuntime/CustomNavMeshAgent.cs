using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;


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
    [SerializeField, Range(.1f, 5)] float height = 1;
    [SerializeField, Range(.5f, 2)] float radius = 1;
    [SerializeField, Range(-5, 5)] float offset = 0;
    [SerializeField, Range(.5f, 10)] float speed = 1;
    [SerializeField, Range(1, 15)] float detectionRange = 5; 
    #endregion

    #region Path
    [SerializeField] CustomNavPath currentPath;
    public CustomNavPath CurrentPath { get { return currentPath; } }
    #endregion

    #region CalculatingState
    CalculatingState pathState = CalculatingState.Waiting;
    public CalculatingState PathState { get { return pathState; } }
    #endregion

    #region Vector3
    public Vector3 OffsetSize { get { return new Vector3(radius, height, radius);  } }
    public Vector3 OffsetPosition { get { return new Vector3(0, (height / 2) + offset, 0);  } }
    #endregion 

    [SerializeField] Transform target; 
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
            StartCoroutine(FollowPath(speed)); 
        }
    }

    /// <summary>
    /// Make the agent follows the path
    /// </summary>
    /// <param name="_speed">speed</param>
    /// <returns></returns>
    public IEnumerator FollowPath(float _speed)
    {
        isMoving = true;
        List<Vector3> _pathToFollow = CurrentPath.PathPoints;
        RaycastHit _hit;
        /* STEERING 
        Vector3 _desiredVelocity = Vector3.Normalize(_pathToFollow.First() - transform.position) * speed;
        Vector3 _currentVelocity = Vector3.Normalize(transform.forward) * speed;
        Vector3 _seek = Vector3.zero;
        Vector3 _avoidanceForce = Vector3.zero;
        Vector3 _steering = Vector3.zero; 
        */

        while (Vector3.Distance(transform.position - OffsetPosition, _pathToFollow.Last()) > .5f)
        {
            /* STEERING
            _hit = default(RaycastHit);
            _avoidanceForce = Vector3.zero;
            */

            if (Vector3.Distance(transform.position - OffsetPosition, _pathToFollow.First()) <= .5f)
            {
                _pathToFollow.RemoveAt(0); 
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
            transform.position = Vector3.MoveTowards(transform.position, _pathToFollow.First() + OffsetPosition, Time.deltaTime * speed); 
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
        if(Input.GetKeyDown(KeyCode.Mouse0))
            SetDestination(target.position);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, OffsetSize); 
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position - OffsetPosition, .1f);
        if (currentPath.PathPoints.Count == 0) return; 
        for (int i = 0; i < currentPath.PathPoints.Count; i++)
        {
            Gizmos.DrawSphere(currentPath.PathPoints[i], .2f); 
        }
        Gizmos.DrawLine(transform.position - OffsetPosition, currentPath.PathPoints.First());
        for (int i = 0; i < currentPath.PathPoints.Count - 1 ; i++)
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