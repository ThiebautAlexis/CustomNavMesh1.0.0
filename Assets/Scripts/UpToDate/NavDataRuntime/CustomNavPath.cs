using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

[Serializable]
public class CustomNavPath
{
    [SerializeField] List<Vector3> pathPoints = new List<Vector3>(); 
    public List<Vector3> PathPoints { get {return pathPoints; }}

   
    public void SetPath(List<Triangle> _pathTriangle)
    {
        pathPoints.Clear();
        foreach (Triangle t in _pathTriangle)
        {
            pathPoints.Add(t.CenterPosition); 
        }
    }
}
