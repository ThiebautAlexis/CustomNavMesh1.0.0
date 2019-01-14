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

   
    public void SetPath(List<CustomNavPoint> _pathPoints)
    {
        navigationPoints = _pathPoints; 
    }
}
