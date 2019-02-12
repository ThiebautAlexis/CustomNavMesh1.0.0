using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

public static class GeometryHelper 
{
    /* GeometryHelper :
	 *
	 *	#####################
	 *	###### PURPOSE ######
	 *	#####################
	 *
	 *	Contains methods that use geomtery
	 *
	 *	#####################
	 *	### MODIFICATIONS ###
	 *	#####################
	 *
	 *	Date :			[06/02/2018]
	 *	Author :		[THIEBAUT Alexis]
	 *
	 *	Changes : 
	 *
	 *	Initialisation of the class
     *	    - Migrating all geometry methods from other scripts
	 *
	 *	-----------------------------------
	*/

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
    public static bool IsIntersecting(Vector3 L1_start, Vector3 L1_end, Vector3 L2_start, Vector3 L2_end)
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
    public static bool IsInTriangle(Vector3 _position, Triangle _triangle)
    {
        Barycentric _barycentric = new Barycentric(_triangle.Vertices[0].Position, _triangle.Vertices[1].Position, _triangle.Vertices[2].Position, _position);
        return _barycentric.IsInside;
    }

    /// <summary>
    /// Check if a point is between two endpoints of a segment
    /// </summary>
    /// <param name="_firstSegmentPoint">First endpoint of the segment</param>
    /// <param name="_secondSegmentPoint">Second endpoint of the segment</param>
    /// <param name="_comparedPoint">Compared point</param>
    /// <returns></returns>
    public static bool PointContainedInSegment(Vector3 _firstSegmentPoint, Vector3 _secondSegmentPoint, Vector3 _comparedPoint)
    {
        float _segmentLength = Vector3.Distance(_firstSegmentPoint, _secondSegmentPoint);
        float _a = Vector3.Distance(_firstSegmentPoint, _comparedPoint);
        float _b = Vector3.Distance(_secondSegmentPoint, _comparedPoint);
        return _segmentLength > _a && _segmentLength > _b;
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
    public static int AngleSign(Vector3 _start, Vector3 _end, Vector3 _point, bool debug = false)
    {
        Vector3 _ref = _end - _start;
        Vector3 _angle = _point - _start;
        if (debug) Debug.Log($"{_start} --> {_end} // {_point} = {Vector3.SignedAngle(_ref, _angle, Vector3.up)}");
        float _alpha = Vector3.SignedAngle(_ref, _angle, Vector3.up);
        if (_alpha == 0 || _alpha == 180 || _alpha == -180) return 0;
        if (_alpha > 0) return 1;
        return -1;
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
    public static Triangle GetTriangleContainingPosition(Vector3 _position, List<Triangle> triangles)
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
    #endregion

    #region Vector3
    /// <summary>
    /// Get the transposed point of the predicted position on a segement between the previous and the next position
    /// Check if the targeted point is on the segment between the previous and the next points
    /// If it doesn't the normal point become the _nextPosition
    /// </summary>
    /// <param name="_predictedPosition">Predicted Position</param>
    /// <param name="_previousPosition">Previous Position</param>
    /// <param name="_nextPosition">Next Position</param>
    /// <returns></returns>
    public static Vector3 GetNormalPoint(Vector3 _predictedPosition, Vector3 _previousPosition, Vector3 _nextPosition)
    {
        Vector3 _ap = _predictedPosition - _previousPosition;
        Vector3 _ab = (_nextPosition - _previousPosition).normalized;
        Vector3 _ah = _ab * (Vector3.Dot(_ap, _ab));
        Vector3 _normal = (_previousPosition + _ah);
        Vector3 _min = Vector3.Min(_previousPosition, _nextPosition);
        Vector3 _max = Vector3.Max(_previousPosition, _nextPosition);
        if (_normal.x < _min.x || _normal.y < _min.y || _normal.x > _max.x || _normal.y > _max.y)
        {
            return _nextPosition;
        }
        return _normal; 
    }
    #endregion

    #endregion
}
