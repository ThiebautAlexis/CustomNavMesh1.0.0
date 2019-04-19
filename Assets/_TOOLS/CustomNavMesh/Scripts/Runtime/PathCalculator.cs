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
    /// Calculate path from an origin to a destination 
    /// Set the path when it can be calculated 
    /// </summary>
    /// <param name="_origin">The Origin of the path </param>
    /// <param name="_destination">The Destination of the path</param>
    /// <param name="_path">The path to set</param>
    /// <returns>Return if the path can be calculated</returns>
    public static bool CalculatePath(Vector3 _origin, Vector3 _destination, CustomNavPath _path, List<Triangle> _trianglesDatas)
    {
        Vector3 _groundedOrigin = GeometryHelper.GetClosestPosition(_origin, _trianglesDatas);
        Vector3 _groundedDestination = GeometryHelper.GetClosestPosition(_destination, _trianglesDatas);

        // GET TRIANGLES
        // Get the origin triangle and the destination triangle
        Triangle _originTriangle = GeometryHelper.GetTriangleContainingPosition(_groundedOrigin, _trianglesDatas);
        Triangle _targetedTriangle = GeometryHelper.GetTriangleContainingPosition(_groundedDestination, _trianglesDatas);

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
                        _linkedTriangle.HeuristicPriority = HeuristicCost(_linkedTriangle, _targetedTriangle) + _cost + _linkedTriangle.Weight;
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
    /// Get the triangle with the best heuristic cost from a list 
    /// Remove this point from the list and return it
    /// </summary>
    /// <param name="_triangles">list where the points are</param>
    /// <returns>point with the best heuristic cost</returns>
    static Triangle GetBestTriangle(List<Triangle> _triangles)
    {
        int _bestIndex = 0;
        for (int i = 0; i < _triangles.Count; i++)
        {
            if (_triangles[i].HeuristicPriority < _triangles[_bestIndex].HeuristicPriority)
            {
                _bestIndex = i;
            }
        }

        Triangle _bestNavTriangle = _triangles[_bestIndex];
        _triangles.RemoveAt(_bestIndex);
        return _bestNavTriangle;
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
                if (GeometryHelper.IsIntersecting(_startLinePoint, _endLinePoint, _vertex1, _vertex2))
                {
                    if (GeometryHelper.AngleSign(_startLinePoint, _endLinePoint, _vertex1) > 0)
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
            if (GeometryHelper.IsIntersecting(_startLinePoint, _endLinePoint, _vertex1, _vertex2))
            {
                if (GeometryHelper.AngleSign(_startLinePoint, _endLinePoint, _vertex1) > 0)
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

        // Initialise end portal vertices -> Close the funnel
        _leftVertices[_absoluteTrianglePath.Count] = _destination;
        _rightVertices[_absoluteTrianglePath.Count] = _destination;
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
                if (GeometryHelper.AngleSign(_apex, _currentLeftVertex, _nextLeftVertex) >= 0)  
                {
                    //if next side cross the other side, place new apex
                    if (GeometryHelper.AngleSign(_apex, _currentRightVertex, _nextLeftVertex) > 0) 
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
                if (GeometryHelper.AngleSign(_apex, _currentRightVertex, _nextRightVertex) <= 0)
                {
                    //if next side cross the other side, place new apex
                    if (GeometryHelper.AngleSign(_apex, _currentLeftVertex, _nextRightVertex) < 0)
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
