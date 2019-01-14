using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

[Serializable]
public class CustomNavPath
{
    [SerializeField] List<CustomNavPoint> navigationPoints = new List<CustomNavPoint>(); 
    public List<CustomNavPoint> NavigationPoints { get {return navigationPoints; }}

    /// <summary>
    /// Build a path using Astar resources
    /// Get the last point and get all its parent to build the path
    /// </summary>
    /// <param name="_pathToBuild">Astar resources</param>
    public void BuildPath(Dictionary<CustomNavPoint, CustomNavPoint> _pathToBuild)
    {
        navigationPoints.Clear();
        CustomNavPoint _currentPoint = _pathToBuild.Last().Key;
        while (_currentPoint != _pathToBuild.First().Key)
        {
            navigationPoints.Add(_currentPoint);
            _currentPoint = _pathToBuild[_currentPoint]; 
        }
        navigationPoints.Add(_currentPoint);
        navigationPoints.Reverse();
    }

    /// <summary>
    /// Clear the path datas
    /// </summary>
    public void ClearPath()
    {
        navigationPoints.Clear(); 
    }
   
}
