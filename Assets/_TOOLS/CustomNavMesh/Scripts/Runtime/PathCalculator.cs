using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
[Script Header] PathCalculator Version 0.0.1
Created by: Thiebaut Alexis
Date: 14/01/2019
Description: 
Script that search and compute the path using a* algorithm
Simplify the a* path using funnel algorithm             

///
[UPDATES]
Update n°: 001
Updated by: Thiebaut Alexis
Date: 14/01/2019
Description: 
 - First creation of the a* algorithm
 - Create all necessary methods to calcuulate a* path

Update n°: 002
Updated by: Thiebaut Alexis
Date: 14/01/2019
Description: Implementation of the funnel algorithm

Update n°: 003
Updated by: Thiebaut Alexis
Date: 24/01/2019
Description: Completing the funnel algorithm. Now the path is completly smoothed

*/

public static class PathCalculator 
{
    #region Methods
    #region bool
    /// <summary>
    /// Check if two segement intersect
    /// </summary>
    /// <param name="L1_start">start of the first segment</param>
    /// <param name="L1_end">end of the first segment</param>
    /// <param name="L2_start">start of the second segment</param>
    /// <param name="L2_end">end of the second segment</param>
    /// <returns>return true if segements intersect</returns>
    static bool IsIntersecting(Vector3 L1_start, Vector3 L1_end, Vector3 L2_start, Vector3 L2_end)
    {
        bool isIntersecting = false;

        //3d -> 2d
        Vector2 p1 = new Vector2(L1_start.x, L1_start.z);
        Vector2 p2 = new Vector2(L1_end.x, L1_end.z);

        Vector2 p3 = new Vector2(L2_start.x, L2_start.z);
        Vector2 p4 = new Vector2(L2_end.x, L2_end.z);

        float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);

        //Make sure the denominator is > 0, if so the lines are parallel
        if (denominator != 0)
        {
            float u_a = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
            float u_b = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

            //Is intersecting if u_a and u_b are between 0 and 1
            if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;
    }

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
        RaycastHit _hit;
        Vector3 _groundedOrigin;
        Vector3 _groundedDestination; 
        if (Physics.Raycast(new Ray(_origin, Vector3.down), out _hit))
        {
            _groundedOrigin = _hit.point;

        }
        else _groundedOrigin = _origin;
        if (Physics.Raycast(new Ray(_destination, Vector3.down), out _hit))
        {
            _groundedDestination = _hit.point;
        }
        else _groundedDestination = _destination; 
        // GET TRIANGLES
        // Get the origin triangle and the destination triangle
        Triangle _originTriangle = GetTriangleContainingPosition(_groundedOrigin, _trianglesDatas);
        Triangle _targetedTriangle = GetTriangleContainingPosition(_groundedDestination, _trianglesDatas);

        //Open list that contains all heuristically calculated triangles 
        List<Triangle> _openList = new List<Triangle>();
        //returned path
        Dictionary<Triangle, Triangle> _cameFrom = new Dictionary<Triangle, Triangle>();

        Triangle _currentTriangle = null; 

        /* ASTAR: Algorithm*/
        // Add the origin point to the open and close List
        // Set its heuristic cost and its selection state
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
                BuildPath(_cameFrom, _path, _groundedOrigin, _groundedDestination);
                //Clear all points selection state
                foreach (Triangle t in _trianglesDatas)
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
        foreach (Triangle t in _trianglesDatas)
        {
            t.HasBeenSelected = false;
        }
        return false;
    }
    #endregion

    #region float 
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
    static Triangle GetTriangleContainingPosition(Vector3 _position, List<Triangle> triangles)
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
    /// Get the triangle with the best heuristic cost from a list 
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

    #region int
    /// <summary>
    /// Return the sign of the angle between [StartPoint EndPoint] and [StartPoint Point]
    /// </summary>
    /// <param name="_start">Start point</param>
    /// <param name="_end">End Point</param>
    /// <param name="_point">Point</param>
    /// <param name="debug"></param>
    /// <returns>Sign of the angle between the three points (if 0 or 180 or -180, return 0)</returns>
    static int AngleSign(Vector3 _start, Vector3 _end, Vector3 _point, bool debug = false)
    {
        Vector3 _ref = _end - _start;
        Vector3 _angle = _point - _start;
        if (debug) Debug.Log($"{_start} --> {_end} // {_point} = {Vector3.SignedAngle(_ref, _angle, Vector3.up)}");
        float _alpha = Vector3.SignedAngle(_ref, _angle, Vector3.up);
        if (_alpha == 0 || _alpha == 180 || _alpha == -180) return 0; 
        if ( _alpha > 0) return 1;
        return -1; 
    }
    #endregion 

    #region void 
    /// <summary>
    /// Build a path using Astar resources
    /// Get the last point and get all its parent to build the path
    /// </summary>
    /// <param name="_pathToBuild">Astar resources</param>
    static void BuildPath(Dictionary<Triangle, Triangle> _pathToBuild, CustomNavPath _path, Vector3 _origin, Vector3 _destination)
    {
        #region BuildingAbsolutePath
        // Building absolute path -> Link all triangle's CenterPosition together
        // Adding _origin and destination to the path
        Triangle _currentTriangle = _pathToBuild.Last().Key;
        List<Triangle> _absoluteTrianglePath = new List<Triangle>();
        while (_currentTriangle != _pathToBuild.First().Key)
        {
            _absoluteTrianglePath.Add(_currentTriangle);
            _currentTriangle = _pathToBuild[_currentTriangle];
        }
        _absoluteTrianglePath.Add(_currentTriangle);
        //Reverse the path to start at the origin 
        _absoluteTrianglePath.Reverse();
        #endregion

        //Create the simplifiedPath
        List<Vector3> _simplifiedPath = new List<Vector3>() { _origin };

        //If there is only the origin and the destination, the path doesn't have to be simplified
        if (_absoluteTrianglePath.Count <= 1)
        {
            _simplifiedPath.Add(_destination);
            _path.SetPath(_simplifiedPath);
            return;
        }
        //Simplify the path with Funnel Algorithm

        //Create both portals vertices arrays
        Vector3[] _leftVertices = new Vector3[_absoluteTrianglePath.Count + 1];
        Vector3[] _rightVertices = new Vector3[_absoluteTrianglePath.Count + 1];
        //Create the apex
        Vector3 _apex = _origin;
        //Set left and right indexes
        int _leftIndex = 1;
        int _rightIndex = 1;

        //Initialize portal vertices
        Vector3 _startLinePoint = Vector3.zero;
        Vector3 _endLinePoint = Vector3.zero;
        Vector3 _vertex1 = Vector3.zero;
        Vector3 _vertex2 = Vector3.zero;

        #region Initialise Portal Vertices
        //Initialize portal vertices between each triangles
        for (int i = 0; i < _absoluteTrianglePath.Count - 1; i++)
        {
            _startLinePoint = _absoluteTrianglePath[i].CenterPosition;
            _endLinePoint = _absoluteTrianglePath[i + 1].CenterPosition;
            for (int j = 0; j < _absoluteTrianglePath[i].Vertices.Length; j++)
            {
                int k = j + 1 >= _absoluteTrianglePath[i].Vertices.Length ? 0 : j + 1;
                _vertex1 = _absoluteTrianglePath[i].Vertices[j].Position;
                _vertex2 = _absoluteTrianglePath[i].Vertices[k].Position; ;
                if (IsIntersecting(_startLinePoint, _endLinePoint, _vertex1, _vertex2))
                {
                    if (AngleSign(_startLinePoint, _endLinePoint, _vertex1) > 0)
                    {
                        _leftVertices[i + 1] = _vertex2;
                        _rightVertices[i + 1] = _vertex1;
                    }
                    else
                    {
                        _leftVertices[i + 1] = _vertex1;
                        _rightVertices[i + 1] = _vertex2;
                    }
                    break;
                }
            }
        }

        //Initialize start portal vertices
        _startLinePoint = _origin;
        _endLinePoint = _absoluteTrianglePath[1].CenterPosition;
        for (int j = 0; j < _absoluteTrianglePath[0].Vertices.Length; j++)
        {
            int k = j + 1 >= _absoluteTrianglePath[0].Vertices.Length ? 0 : j + 1;
            _vertex1 = _absoluteTrianglePath[0].Vertices[j].Position;
            _vertex2 = _absoluteTrianglePath[0].Vertices[k].Position; ;
            if (IsIntersecting(_startLinePoint, _endLinePoint, _vertex1, _vertex2))
            {
                if (AngleSign(_startLinePoint, _endLinePoint, _vertex1) > 0)
                {
                    _leftVertices[0] = _vertex2;
                    _rightVertices[0] = _vertex1;
                }
                else
                {
                    _leftVertices[0] = _vertex1;
                    _rightVertices[0] = _vertex2;
                }
                break;
            }
        }


        // Initialise end portal vertices 
        for (int j = 0; j < _absoluteTrianglePath[_absoluteTrianglePath.Count -1].Vertices.Length; j++)
        {
            if (_absoluteTrianglePath[_absoluteTrianglePath.Count - 1].Vertices[j].Position != _leftVertices[_absoluteTrianglePath.Count - 1]
                && (_absoluteTrianglePath[_absoluteTrianglePath.Count - 1].Vertices[j].Position != _rightVertices[_absoluteTrianglePath.Count - 1]))
            {
                _leftVertices[_absoluteTrianglePath.Count] = _absoluteTrianglePath[_absoluteTrianglePath.Count - 1].Vertices[j].Position;
                _rightVertices[_absoluteTrianglePath.Count] = _absoluteTrianglePath[_absoluteTrianglePath.Count - 1].Vertices[j].Position;
            }
        }
        #endregion

        //Step through the channel
        Vector3 _currentLeftVertex;
        Vector3 _nextLeftVertex;
        Vector3 _currentRightVertex;
        Vector3 _nextRightVertex;

        for (int i = 2; i < _absoluteTrianglePath.Count + 1 ; i++) 
        {
            _currentLeftVertex = _leftVertices[_leftIndex];
            _nextLeftVertex = _leftVertices[i];

            _currentRightVertex = _rightVertices[_rightIndex];
            _nextRightVertex = _rightVertices[i];

            //If the new left vertex is different process
            if (_nextLeftVertex != _currentLeftVertex && i > _leftIndex)
            {
                //If the next point does not widden funnel, update 
                if (AngleSign(_apex, _currentLeftVertex, _nextLeftVertex) >= 0)  
                {
                    //if next side cross the other side, place new apex
                    if (AngleSign(_apex, _currentRightVertex, _nextLeftVertex) > 0) 
                    {
                        // Set the new Apex
                        _apex = _currentRightVertex;
                        _simplifiedPath.Add(_apex);
                        //Set i to the apex index to be at the good index on the next loop 
                        i = _rightIndex; 

                        // Find new right vertex.
                        for (int j = _rightIndex; j < _rightVertices.Length; j++)
                        {
                            if (_rightVertices[j] != _apex)
                            {
                                _rightIndex = j;
                                break;
                            }
                        }
                    }
                    // else skip to the next vertex
                    else
                    {
                        _leftIndex = i;
                    }
                }
                //else skip
            }
            //else skip


            // If the right vertex is different process
            if (_nextRightVertex != _currentRightVertex && i > _rightIndex)
            {
                //If the next point does not widden funnel, update 
                if (AngleSign(_apex, _currentRightVertex, _nextRightVertex) <= 0)
                {
                    //if next side cross the other side, place new apex
                    if (AngleSign(_apex, _currentLeftVertex, _nextRightVertex) < 0)
                    {
                        //Set the new Apex
                        _apex = _currentLeftVertex;
                        _simplifiedPath.Add(_apex);
                        //Set i to the apex index to be at the good index on the next loop 
                        i = _leftIndex;

                        // Find next Left Index.
                        for (int j = _leftIndex; j < _leftVertices.Length; j++)
                        {
                            if (_leftVertices[j] != _apex)
                            {
                                _leftIndex = j;
                                break;
                            }
                        }
                    }
                    //else skip to the next vertex
                    else
                    {
                        _rightIndex = i;
                    }
                }
                //else skip
            }
            //else skip
        }

        _simplifiedPath.Add(_destination);
        //Set the simplifiedPath
        _path.SetPath(_simplifiedPath);
    }
    #endregion

    #endregion
}
