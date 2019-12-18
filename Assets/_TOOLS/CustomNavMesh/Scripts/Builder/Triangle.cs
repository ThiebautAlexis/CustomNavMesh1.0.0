using System; 
using System.Collections.Generic;
using UnityEngine;

/*
[Script Header] TDS_Triangle Version 0.0.1
Created by: Alexis Thiébaut
Date: 21/11/2018
Description: Triangle represented by 3 Vertices 
             Linked with another triangles 
             Use the struct Barycentric to check if a point in inside the triangle

///
[UPDATES]
Update n°: 001
Updated by: Thiebaut Alexis
Date: 21/11/2018
Description: Initialisation of the class
             - Initialisation of the vertices, the center position, and the constructor
             - Giving an heuristic priority

Update n°: 002
Updated by: Thiebaut Alexis
Date: 26/03/2019
Description: Try adding weight to the triangle
                -> Search obstacle in the triangle and add weight when an obstacle is found
*/
[Serializable]
public class Triangle
{
    #region Fields and properties
    [SerializeField] Vertex[] vertices = new Vertex[3];
    public Vertex[] Vertices { get { return vertices; } }

    /// Center is equal of the arithmetic center of the 3 points
    public Vector3 CenterPosition
    {
        get
        {
            return (vertices[0].Position + vertices[1].Position + vertices[2].Position)/3;
        }
    }

    public List<Triangle> LinkedTriangles{ get; set;}

    [NonSerialized] public bool HasBeenLinked = false;

    public bool HasBeenSelected { get; set; }

    private float heuristicPriority;
    public float HeuristicPriority
    {
        get
        {
            return heuristicPriority; 
        }
        set
        {
            heuristicPriority = value;
            HeuristicCostToDestination = heuristicPriority - HeuristicCostFromStart;
        }
    }

    public float HeuristicCostFromStart { get; set; }
    public float HeuristicCostToDestination { get; set; }

    private int weight = 0;
    public int Weight
    {
        get { return weight;  }
    }

    private float surface; 
    #endregion

    #region Constructor 
    public Triangle(Vertex[] _vertices)
    {
        for (int i = 0; i < 3; i++)
        {
            vertices[i] =_vertices[i];
        }
        float _a = Vector3.Distance(vertices[0].Position, vertices[1].Position);
        float _b = Vector3.Distance(vertices[1].Position, vertices[2].Position);
        float _c = Vector3.Distance(vertices[2].Position, vertices[0].Position); 
        float _p = ( _a + _b  + _c )/2;
        surface = Mathf.Sqrt(_p * (_p - _a) * (_p - _b) * (_p - _c));

    }
    #endregion

    #region Methods
    /*
    /// <summary>
    /// Update the weight of the triangle
    /// Cast lines between the edges and the center of the triangle in order to detect obstacles
    /// If so, add weight to the triangle
    /// </summary>
    public void UpdateWeight()
    {
        weight = 0;
        RaycastHit _hitInfo;
        float _surface; 
        for (int i = 0; i < 3; i++)
        {
            if(Physics.Linecast(vertices[i].Position, CenterPosition, out _hitInfo))
            {
                _surface = _hitInfo.collider.transform.lossyScale.x * _hitInfo.collider.transform.lossyScale.y;
                //Ajouter le rapport surface de l'obstacle sur surface du triangle au poids
                int _percentage = (int)Mathf.Clamp( ((100 * _surface) / surface)/3, 0, 100);
                Debug.Log(_percentage);
                if (_percentage == 100) return; 
                //weight += ;
            }
        }
    }
    */

    #endregion
}

public struct Barycentric
{
    public float u;
    public float v;
    public float w; 

    /// <summary>
    /// Return if u, v and w are greater or equal than 0 and less or equal than 1
    /// That means the point is inside of the triangle
    /// </summary>
    public bool IsInside
    {
        get
        {
            return (u >= 0.0f) && (u <= 1.0f) && (v >= 0.0f) && (v <= 1.0f) && (w >= 0.0f); //(w <= 1.0f)
        }
    }

    /// <summary>
    /// Calculate 3 value u, v, w as:
    /// aP = u*aV1 + v*aV2 + w*aV3
    /// if u, v and w are greater than 0, that means that the point is in the triangle aV1 aV2 aV3.
    /// </summary>
    /// <param name="aV1">First point of the triangle</param>
    /// <param name="aV2">Second point of the triangle</param>
    /// <param name="aV3">Third point of the triangle</param>
    /// <param name="aP">Point to get the Barycentric coordinates</param>
    public Barycentric(Vector3 aV1, Vector3 aV2, Vector3 aV3, Vector3 aP)
    {
        Vector3 a = aV2 - aV3;
        Vector3 b = aV1 - aV3;
        Vector3 c = aP - aV3;
        float aLen = a.x * a.x + a.y * a.y + a.z * a.z;
        float bLen = b.x * b.x + b.y * b.y + b.z * b.z;
        float ab = a.x * b.x + a.y * b.y + a.z * b.z;
        float ac = a.x * c.x + a.y * c.y + a.z * c.z;
        float bc = b.x * c.x + b.y * c.y + b.z * c.z;
        float d = aLen * bLen - ab * ab;
        u = (aLen * bc - ab * ac) / d;
        v = (bLen * ac - ab * bc) / d;
        w = 1.0f - u - v;
    }
}
