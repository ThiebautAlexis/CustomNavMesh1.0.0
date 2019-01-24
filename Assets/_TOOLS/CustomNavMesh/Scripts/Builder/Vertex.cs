using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[Script Header] TDS_NavPoint Version 0.0.1
Created by: Alexis Thiébaut
Date: 21/11/2018
Description: Point of a polygone with an ID

///
[UPDATES]
Update n°: 001
Updated by: Thiebaut Alexis
Date: 21/11/2018
Description: - Creation of the object 
*/
[Serializable]
public class Vertex
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
    #endregion

    #region Constructor
    public Vertex(Vector3 _pos, int _id)
    {
        id = _id; 
        xPos = _pos.x;
        yPos = _pos.y;
        zPos = _pos.z; 
    }
    #endregion
}