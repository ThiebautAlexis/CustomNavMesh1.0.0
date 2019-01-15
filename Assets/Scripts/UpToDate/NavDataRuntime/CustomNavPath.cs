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
    private List<Vector3> left = new List<Vector3>();
    public List<Vector3> Left { get { return left; } }
    private List<Vector3> right = new List<Vector3>();
    public List<Vector3> Right { get { return right; } }


    public void SetPath(List<Vector3> _path, Vector3[] l, Vector3[] r)
    {
        pathPoints = _path;
        left = l.ToList();
        right = r.ToList(); 
    }
    public void SetPath(List<Triangle> _triangles, Vector3[] l, Vector3[] r)
    {
        pathPoints = _triangles.Select(t => t.CenterPosition).ToList();
        left = l.ToList();
        right = r.ToList();
    }


}
