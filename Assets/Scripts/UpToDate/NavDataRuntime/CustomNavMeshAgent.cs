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
    [SerializeField] CustomNavPath currentPath;
    public CustomNavPath CurrentPath { get { return currentPath; } }

    CalculatingState pathState = CalculatingState.Waiting;
    public CalculatingState PathState { get { return pathState; } }

    bool isMoving = false; 
    public bool IsMoving { get { return isMoving; } }

    [SerializeField] Transform target; 
    #endregion

    #region Methods
    /// <summary>
    /// Calculate a path until reaching a destination
    /// </summary>
    /// <param name="_position">destination to reach</param>
    public void SetDestination(Vector3 _position)
    {
        currentPath.ClearPath();
        pathState = CalculatingState.Calculating;
        Debug.Log(CustomNavMeshManager.Instance.CalculatePath(transform.position, _position, currentPath)); 
        return; 
        if (CustomNavMeshManager.Instance.CalculatePath(transform.position, _position, currentPath))
            pathState = CalculatingState.Ready; 
    }

    /// <summary>
    /// Make the agent follows the path
    /// </summary>
    /// <param name="_speed">speed</param>
    /// <returns></returns>
    public IEnumerator FollowPath(float _speed)
    {
        isMoving = true; 
        CustomNavPath _pathToFollow = CurrentPath;
        int _index = 1;
        while (Vector3.Distance(transform.position, _pathToFollow.NavigationPoints.Last().Position) > .5f)
        {
            if (Vector3.Distance(transform.position, _pathToFollow.NavigationPoints[_index].Position) <= .5f)
            {
                _index = _index + 1;
            }
            transform.position = Vector3.MoveTowards(transform.position, _pathToFollow.NavigationPoints[_index].Position, Time.deltaTime * _speed);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        pathState = CalculatingState.Waiting;
        isMoving = false;
        CurrentPath.ClearPath();
        OnDestinationReached?.Invoke(); 
        yield break;
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
        Gizmos.color = Color.red; 
        for (int i = 0; i < currentPath.NavigationPoints.Count; i++)
        {
            Gizmos.DrawSphere(currentPath.NavigationPoints[i].Position, .2f); 
        }
        for (int i = 0; i < currentPath.NavigationPoints.Count - 1 ; i++)
        {
            Gizmos.DrawLine(currentPath.NavigationPoints[i].Position, currentPath.NavigationPoints[i + 1].Position); 
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