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
    [SerializeField, Range(-5, 5)] float offset = 0;
    [SerializeField, Range(.5f, 10)] float speed = 1;

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
    public Vector3 OffsetSize { get { return new Vector3(transform.localScale.x, height, transform.localScale.z);  } }
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
        int _index = 1;
        while (Vector3.Distance(transform.position, _pathToFollow.Last()) > .5f)
        {
            if (Vector3.Distance(transform.position, _pathToFollow[_index]) <= .5f)
            {
                _index = _index + 1;
            }
            transform.position = Vector3.MoveTowards(transform.position, _pathToFollow[_index] + OffsetPosition , Time.deltaTime * _speed);
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
        for (int i = 0; i < currentPath.PathPoints.Count; i++)
        {
            Gizmos.DrawSphere(currentPath.PathPoints[i], .2f); 
        }
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