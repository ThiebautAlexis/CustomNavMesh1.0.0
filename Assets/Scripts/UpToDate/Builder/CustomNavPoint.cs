using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[Script Header] TDS_NavPoint Version 0.0.1
Created by: Alexis Thiébaut
Date: 21/11/2018
Description: 

///
[UPDATES]
Update n°:
Updated by:
Date:
Description:
*/
[Serializable]
public class CustomNavPoint
{
    #region Fields and Properties
    private int id = 0; 
    public int ID { get { return id; } }

    public Vector3 Position
    {
        get
        {
            return new Vector3(xPos, yPos, zPos);
        }
    }

    [SerializeField] float xPos, yPos, zPos;

    /* LEGACY
    public bool HasBeenSelected { get; set; }

    private float heuristicPriority;
    public float HeuristicPriority
    {
        get
        {
            return heuristicPriority = HeuristicCostFromStart + HeuristicCostToDestination;
        }
        set
        {
            heuristicPriority = value;
            HeuristicCostToDestination = heuristicPriority - HeuristicCostFromStart;
        }
    }

    public float HeuristicCostFromStart { get; set; }
    public float HeuristicCostToDestination { get; set; }
    */

    #endregion

    #region Constructor
    public CustomNavPoint(Vector3 _pos, int _id)
    {
        id = _id; 
        xPos = _pos.x;
        yPos = _pos.y;
        zPos = _pos.z; 
    }
    #endregion

    #region Methods
    /*OLD
    public CustomNavPoint[] GetAllNeighborsIndexes()
    {
        List<CustomNavPoint> _indexes = new List<CustomNavPoint>();
        for (int i = 0; i < linkedTriangles.Count; i++)
        {
            Triangle _t = linkedTriangles[i];
            _indexes.AddRange(_t.Vertices);

            /*[OLD]
            _indexes.AddRange(_t.MiddleSegmentIndex); 
            //

        }
        return _indexes.ToArray(); 
    }
    */
    #endregion
}