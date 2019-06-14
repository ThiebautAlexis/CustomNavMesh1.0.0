using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

/*
[Script Header] CustomNavPath Version 0.0.1
Created by: Thiebaut Alexis
Date: 14/01/2019
Description: Serializable Object containing a list of points that define a path

///
[UPDATES]
Update n°: 001
Updated by: Thiebaut Alexis
Date: 14/01/2019
Description: - Initialize the class
             - Creating the list of Vector3 and the Methods to set the path
*/
[Serializable]
public class CustomNavPath
{
    [SerializeField] List<Vector3> pathPoints = new List<Vector3>(); 
    public List<Vector3> PathPoints { get { return pathPoints; } }


    public void SetPath(List<Vector3> _path)
    {
        pathPoints = _path;
    }
}
