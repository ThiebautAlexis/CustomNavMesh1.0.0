using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
[Script Header] PathCalculator Version 0.0.1
Created by:
Date: 
Description:

///
[UPDATES]
Update n°:
Updated by:
Date:
Description:
*/

public static class PathCalculator 
{

    #region Fields/Properties
    #endregion

    #region Methods
    #region bool
    /// <summary>
    /// Return if the position is inside of the triangle
    /// </summary>
    /// <param name="_position"></param>
    /// <returns></returns>
    static bool IsInTriangle(Vector3 _position, Triangle _triangle)
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
    public static bool CalculatePath(Vector3 _origin, Vector3 _destination, CustomNavPath _path, List<Triangle> _trianglesDatas)
    {
        // GET TRIANGLES
        // Get the origin triangle and the destination triangle
        Triangle _originTriangle = GetTriangleContainingPosition(_origin, _trianglesDatas);
        Triangle _targetedTriangle = GetTriangleContainingPosition(_destination, _trianglesDatas);

        /* LEGACY

        // CREATE POINTS
        CustomNavPoint _currentPoint = null;
        //Create a point on the origin position
        CustomNavPoint _originPoint = new CustomNavPoint(_origin, -1);

        //Create a point on the destination position
        CustomNavPoint _destinationPoint = new CustomNavPoint(_destination, -2);

        //ARRAY 
        CustomNavPoint[] _linkedPoints = null;

        //LISTS AND DICO
        List<CustomNavPoint> _openList = new List<CustomNavPoint>();
        Dictionary<CustomNavPoint, CustomNavPoint> _cameFrom = new Dictionary<CustomNavPoint, CustomNavPoint>();

        */

        List<Triangle> _openList = new List<Triangle>();
        Dictionary<Triangle, Triangle> _cameFrom = new Dictionary<Triangle, Triangle>();

        Triangle _currentTriangle = null; 

        /* ASTAR: Algorithm*/
        // Add the origin point to the open and close List
        //Set its heuristic cost and its selection state
        _openList.Add(_originTriangle);
        _originTriangle.HeuristicCostFromStart = 0;
        _originTriangle.HasBeenSelected = true;
        _cameFrom.Add(_originTriangle, _originTriangle);
        float _cost = 0;
        while (_openList.Count > 0)
        {
            //Get the point with the best heuristic cost
            _currentTriangle = GetBestTriangle(_openList);
            //If this point is in the targeted triangle, 
            if (_currentTriangle == _targetedTriangle)
            {
                _cost = _currentTriangle.HeuristicCostFromStart + HeuristicCost(_currentTriangle, _targetedTriangle);
                _targetedTriangle.HeuristicCostFromStart = _cost;
                //add the destination point to the close list and set the previous point to the current point or to the parent of the current point if it is in Line of sight 

                //_cameFrom.Add(_targetedTriangle, _currentTriangle);

                //Build the path
                BuildPath(_cameFrom, _path);
                //Clear all points selection state
                foreach (Triangle t in _openList)
                {
                    t.HasBeenSelected = false;
                }
                return true;
            }
            //Get all linked points from the current point
            //_linkedPoints = GetLinkedPoints(_currentPoint, trianglesDatas);
            for (int i = 0; i < _currentTriangle.LinkedTriangles.Count; i++)
            {
                Triangle _linkedTriangle = _currentTriangle.LinkedTriangles[i];
                // If the linked points is not selected yet
                if (!_linkedTriangle.HasBeenSelected)
                {
                    // Calculate the heuristic cost from start of the linked point
                    _cost = _currentTriangle.HeuristicCostFromStart + HeuristicCost(_currentTriangle, _linkedTriangle);
                    _linkedTriangle.HeuristicCostFromStart = _cost;
                    if (!_openList.Contains(_linkedTriangle) || _cost < _linkedTriangle.HeuristicCostFromStart)
                    {
                        // Set the heuristic cost from start for the linked point
                        _linkedTriangle.HeuristicCostFromStart = _cost;
                        //Its heuristic cost is equal to its cost from start plus the heuristic cost between the point and the destination
                        _linkedTriangle.HeuristicPriority = HeuristicCost(_linkedTriangle, _targetedTriangle) + _cost;
                        //Set the point selected and add it to the open and closed list
                        _linkedTriangle.HasBeenSelected = true;
                        _openList.Add(_linkedTriangle);
                        _cameFrom.Add(_linkedTriangle, _currentTriangle);
                    }
                }
            }
        }
        Debug.Log("PAPOSSIBLE"); 
        return false;
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
    static float HeuristicCost(CustomNavPoint _a, CustomNavPoint _b)
    {
        return Vector3.Distance(_a.Position, _b.Position);
    }

    /// <summary>
    /// Return the heuristic cost between 2 triangles
    /// Heuristic cost is the distance between 2 points
    /// => Can add a multiplier to change the cost of the movement depending on the point 
    /// </summary>
    /// <param name="_a">First Triangle</param>
    /// <param name="_b">Second Triangle</param>
    /// <returns>Heuristic Cost between 2 points</returns>
    static float HeuristicCost(Triangle _a, Triangle _b)
    {
        return Vector3.Distance(_a.CenterPosition, _b.CenterPosition);
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
    static public Triangle GetTriangleContainingPosition(Vector3 _position, List<Triangle> triangles)
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
    static Triangle[] GetTrianglesFromPoint(CustomNavPoint _point, List<Triangle> triangles)
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
    static CustomNavPoint[] GetLinkedPoints(CustomNavPoint _point, List<Triangle> triangles)
    {

        List<CustomNavPoint> _points = new List<CustomNavPoint>();
        List<Triangle> _triangles = GetTrianglesFromPoint(_point, triangles).ToList();
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
    /// <param name="_triangles">list where the points are</param>
    /// <returns>point with the best heuristic cost</returns>
    static Triangle GetBestTriangle(List<Triangle> _triangles)
    {
        int bestIndex = 0;
        for (int i = 0; i < _triangles.Count; i++)
        {
            if (_triangles[i].HeuristicPriority < _triangles[bestIndex].HeuristicPriority)
            {
                bestIndex = i;
            }
        }

        Triangle _bestNavTriangle = _triangles[bestIndex];
        _triangles.RemoveAt(bestIndex);
        return _bestNavTriangle;
    }
    #endregion

    #region void 
    /* LEGACY
    /// <summary>
    /// Build a path using Astar resources
    /// Get the last point and get all its parent to build the path
    /// </summary>
    /// <param name="_pathToBuild">Astar resources</param>
    static void BuildPath(Dictionary<CustomNavPoint, CustomNavPoint> _pathToBuild, CustomNavPath _path)
    {
        CustomNavPoint _currentPoint = _pathToBuild.Last().Key;
        List<CustomNavPoint> _pathPoints = new List<CustomNavPoint>(); 
        while (_currentPoint != _pathToBuild.First().Key)
        {
            _pathPoints.Add(_currentPoint);
            _currentPoint = _pathToBuild[_currentPoint];
        }
        _pathPoints.Add(_currentPoint);
        _pathPoints.Reverse();
        _path.SetPath(_pathPoints); 
    }
    */
    /// <summary>
    /// Build a path using Astar resources
    /// Get the last point and get all its parent to build the path
    /// </summary>
    /// <param name="_pathToBuild">Astar resources</param>
    static void BuildPath(Dictionary<Triangle, Triangle> _pathToBuild, CustomNavPath _path)
    {
        Triangle _currentPoint = _pathToBuild.Last().Key;
        List<Triangle> _trianglePath = new List<Triangle>();
        while (_currentPoint != _pathToBuild.First().Key)
        {
            _trianglePath.Add(_currentPoint);
            _currentPoint = _pathToBuild[_currentPoint];
        }
        _trianglePath.Add(_currentPoint);
        _trianglePath.Reverse();
        _path.SetPath(_trianglePath);
    }
    #endregion

    #endregion

}
