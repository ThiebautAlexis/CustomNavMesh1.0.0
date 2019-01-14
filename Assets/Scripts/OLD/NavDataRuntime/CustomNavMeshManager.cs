using System; 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using UnityEngine;
using UnityEngine.SceneManagement;

/*
[Script Header] TDS_NavMeshManager Version 0.0.1
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
public class CustomNavMeshManager : MonoBehaviour
{
    #region Fields and properties
    public static CustomNavMeshManager Instance;

    [SerializeField] List<Triangle> triangles = new List<Triangle>();
    public List<Triangle> Triangles { get { return triangles; }  }

    private bool isCalculating = false; 
    public bool IsCalculating { get { return IsCalculating; } } 
    #endregion

    #region Methods
    #region bool
    /// <summary>
    /// Return if the position is inside of the triangle
    /// </summary>
    /// <param name="_position"></param>
    /// <returns></returns>
    bool IsInTriangle(Vector3 _position, Triangle _triangle)
    {
        Barycentric _barycentric = new Barycentric(_triangle.Vertices[0].Position, _triangle.Vertices[1].Position, _triangle.Vertices[2].Position, _position);
        return _barycentric.IsInside;
    }

    /// <summary>
    /// Calculate path from an origin to a destination 
    /// Set the path when it can be calculated 
    /// </summary>
    /// <param name="_origin">The Origin of the path </param>
    /// <param name="_destination">The Destination of the path</param>
    /// <param name="_path">The path to set</param>
    /// <returns>Return if the path can be calculated</returns>
    public bool CalculatePath(Vector3 _origin, Vector3 _destination, CustomNavPath _path)
    {
        isCalculating = true; 
        // GET TRIANGLES
        // Get the origin triangle and the destination triangle
        Triangle _currentTriangle = GetTriangleContainingPosition(_origin);
        Triangle _targetedTriangle = GetTriangleContainingPosition(_destination);

        // CREATE POINTS
        CustomNavPoint _currentPoint = null;
        //Create a point on the origin position
        CustomNavPoint _originPoint = new CustomNavPoint(_origin, -1);

        /*OLD
        //Get the linked triangles for the origin point
        _originPoint.LinkedTriangles.Add(_currentTriangle);
        */ 

        //Create a point on the destination position
        CustomNavPoint _destinationPoint = new CustomNavPoint(_destination, -2);

        //ARRAY 
        CustomNavPoint[] _linkedPoints = null;

        //LISTS AND DICO
        List<CustomNavPoint> _openList = new List<CustomNavPoint>();
        Dictionary<CustomNavPoint, CustomNavPoint> _cameFrom = new Dictionary<CustomNavPoint, CustomNavPoint>();

        /* ASTAR: Algorithm*/
        // Add the origin point to the open and close List
        //Set its heuristic cost and its selection state
        _openList.Add(_originPoint);
        _originPoint.HeuristicCostFromStart = 0; 
        _originPoint.HasBeenSelected = true;
        _cameFrom.Add(_originPoint, _originPoint);
        float _cost = 0; 
        while (_openList.Count > 0)
        {
            //Get the point with the best heuristic cost
            _currentPoint = GetBestPoint(_openList);
            //If this point is in the targeted triangle, 
            if (GetTrianglesFromPoint(_currentPoint).Contains(_targetedTriangle))
            {
                _cost = _currentPoint.HeuristicCostFromStart + HeuristicCost(_currentPoint, _destinationPoint);
                _destinationPoint.HeuristicCostFromStart = _cost;
                //add the destination point to the close list and set the previous point to the current point or to the parent of the current point if it is in Line of sight 
                if (IsInLineOfSight(_cameFrom[_currentPoint], _destinationPoint))
                {
                    _cameFrom.Add(_destinationPoint, _cameFrom[_currentPoint]); 
                }
                else
                {
                    _cameFrom.Add(_destinationPoint, _currentPoint);
                }
                //Build the path
                _path.BuildPath(_cameFrom);
                //Clear all points selection state
                foreach (CustomNavPoint point in _openList)
                {
                    point.HasBeenSelected = false; 
                }
                return true;
            }
            //Get all linked points from the current point
            _linkedPoints = GetLinkedPoints(_currentPoint);
            for (int i = 0; i < _linkedPoints.Length; i++)
            {
                CustomNavPoint _linkedPoint = _linkedPoints[i];
                CustomNavPoint _parentPoint; 
                // If the linked points is not selected yet
                if (!_linkedPoint.HasBeenSelected)
                {
                    // Calculate the heuristic cost from start of the linked point
                    _cost = _currentPoint.HeuristicCostFromStart + HeuristicCost(_currentPoint, _linkedPoint);
                    _linkedPoint.HeuristicCostFromStart = _cost; 
                    if (!_openList.Contains(_linkedPoint) || _cost < _linkedPoint.HeuristicCostFromStart)
                    {
                        if(IsInLineOfSight(_cameFrom[_currentPoint], _linkedPoint))
                        {
                            _cost = HeuristicCost(_cameFrom[_currentPoint], _linkedPoint);
                            _parentPoint = _cameFrom[_currentPoint]; 
                        }
                        else
                        {
                            _parentPoint = _currentPoint; 
                        }
                        // Set the heuristic cost from start for the linked point
                        _linkedPoint.HeuristicCostFromStart = _cost;
                        //Its heuristic cost is equal to its cost from start plus the heuristic cost between the point and the destination
                        _linkedPoint.HeuristicPriority = HeuristicCost(_linkedPoint, _destinationPoint) + _cost;
                        //Set the point selected and add it to the open and closed list
                        _linkedPoint.HasBeenSelected = true;
                        _openList.Add(_linkedPoint);
                        _cameFrom.Add(_linkedPoint, _parentPoint);
                    }
                }
            }
        }
        
        return false;
    }

    /// <summary>
    /// Check the cost 
    /// Get if there is a triangle in the direction from the next point to the parent point
    /// </summary>
    /// <param name="_previousPoint">Parent Point</param>
    /// <param name="_nextPoint">Linked Point</param>
    /// <returns>if the link between the previous and the next point can be set</returns>
    bool IsInLineOfSight(CustomNavPoint _previousPoint, CustomNavPoint _nextPoint)
    {
        float _cost = _previousPoint.HeuristicCostFromStart + HeuristicCost(_previousPoint, _nextPoint);
        if (_cost > _nextPoint.HeuristicCostFromStart) return false;
        return true; 
    }
    #endregion



    #region float 
    /// <summary>
    /// Return the heuristic cost between 2 points
    /// Heuristic cost is the distance between 2 points
    /// => Can add a multiplier to change the cost of the movement depending on the point 
    /// </summary>
    /// <param name="_a">First Point</param>
    /// <param name="_b">Second Point</param>
    /// <returns>Heuristic Cost between 2 points</returns>
    float HeuristicCost(CustomNavPoint _a, CustomNavPoint _b)
    {
        return Vector3.Distance(_a.Position, _b.Position); 
    }
    #endregion 

    #region Triangle
    /// <summary>
    /// Get the triangle where the position is contained 
    /// Position is right under the selected Position
    /// If triangle can't be found, get the closest triangle
    /// </summary>
    /// <param name="_position">Position</param>
    /// <returns>Triangle where the position is contained</returns>
    public Triangle GetTriangleContainingPosition(Vector3 _position)
    {
        RaycastHit _hit;
        if (Physics.Raycast(_position, Vector3.down, out _hit, 5))
        {
            Vector3 _onGroundPosition = _hit.point;
            foreach (Triangle triangle in triangles)
            {
                if (IsInTriangle(_onGroundPosition, triangle))
                {
                    return triangle;
                }
            }
        }
        return triangles.OrderBy(t => Vector3.Distance(t.CenterPosition, _position)).FirstOrDefault(); 
    }

    /// <summary>
    /// Get all triangles that contains the point P
    /// </summary>
    /// <param name="_point">Point P</param>
    /// <returns>List of all linked triangles of the point P</returns>
    Triangle[] GetTrianglesFromPoint(CustomNavPoint _point)
    {
        List<Triangle> _containingTriangles = new List<Triangle>();
        foreach (Triangle triangle in triangles)
        {
            if (IsInTriangle(_point.Position, triangle))
            {
                _containingTriangles.Add(triangle);
            }
        }
        return _containingTriangles.ToArray();
    }
    #endregion

    #region NavPoints
    /// <summary>
    /// Get all linked points from a selected point
    /// </summary>
    /// <param name="_point">Point</param>
    /// <returns>Array containing the linked points of the selected point</returns>
    CustomNavPoint[] GetLinkedPoints(CustomNavPoint _point)
    {
        
        List<CustomNavPoint> _points = new List<CustomNavPoint>();
        List<Triangle> _triangles = GetTrianglesFromPoint(_point).ToList();
        //For each triangle Linked
        for (int i = 0; i < _triangles.Count; i++)
        {
            CustomNavPoint[] _vertices = _triangles[i].Vertices.Where(v => v.ID != _point.ID).ToArray(); 
            //Get vertices indexes
            _points.AddRange(_vertices);
        }
        return _points.ToArray(); 
        
    }

    /// <summary>
    /// Get the point with the best heuristic cost from a list 
    /// Remove this point from the list and return it
    /// </summary>
    /// <param name="_points">list where the points are</param>
    /// <returns>point with the best heuristic cost</returns>
    CustomNavPoint GetBestPoint(List<CustomNavPoint> _points)
    {
        int bestIndex = 0;
        for (int i = 0; i < _points.Count; i++)
        {
            if (_points[i].HeuristicPriority < _points[bestIndex].HeuristicPriority)
            {
                bestIndex = i;
            }
        }

        CustomNavPoint _bestNavPoint = _points[bestIndex];
        _points.RemoveAt(bestIndex);
        return _bestNavPoint;
    }
    #endregion

    #region void
    /// <summary>
    /// Get the datas from the dataPath folder to get the navpoints and the triangles
    /// </summary>
    void LoadDatas()
    {
        CustomNavDataSaver<CustomNavData> _loader = new CustomNavDataSaver<CustomNavData>();
        string _sceneName = SceneManager.GetActiveScene().name;
        string _directoryName = CustomNavDataLoader.DirectoryPath;
        CustomNavData _datas = _loader.LoadFile(_directoryName, _sceneName);
        triangles = _datas.TrianglesInfos;
    }
    #endregion

    #endregion

    #region Unity Methods
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this); 
        }
    }

    void Start ()
    {
        LoadDatas(); 
    }

    private void OnDrawGizmos()
    {
        
        if (triangles.Count == 0) return;
        Gizmos.color = Color.green;
        foreach (Triangle triangle in triangles)
        {
            for (int i = 0; i < triangle.Vertices.Length; i++)
            {
                Gizmos.DrawSphere(triangle.Vertices[i].Position, .5f);
                if (i < 2)
                {
                    Gizmos.DrawLine(triangle.Vertices[i].Position, triangle.Vertices[i + 1].Position);
                }
                else
                {
                    Gizmos.DrawLine(triangle.Vertices[i].Position, triangle.Vertices[0].Position);
                }
            }
        }        
    }
    #endregion

}

[Serializable]
public struct CustomNavData
{
    public List<Triangle> TrianglesInfos;
}