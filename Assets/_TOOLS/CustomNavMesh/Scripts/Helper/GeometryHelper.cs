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
	 *	Date :			[06/02/2019]
	 *	Author :		[THIEBAUT Alexis]
	 *
	 *	Changes : 
	 *
	 *	Initialisation of the class
     *	    - Migrating all geometry methods from other scripts
	 *
	 *	-----------------------------------
     *	
     * Date :			[29/04/2019]
	 *	Author :		[THIEBAUT Alexis]
	 *
	 *	Changes : 
	 *
	 *	Modification on the method Is Intersecting, now can return the intersection point
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
    public static bool IsIntersecting(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d)
    {

        Vector3 _ab = _b - _a; // I
        float _anglesign = AngleSign(_a, _b, _c);
        Vector3 _cd = _anglesign < 0 ? _d - _c : _c - _d; // J 

        Vector3 _pointLeft = _anglesign < 0 ? _c : _d;

        float _denominator = Vector3.Cross(_ab, _cd).magnitude;

        if (_denominator != 0)
        {
            //  m =    (     -Ix*A.y      +      Ix*Cy     +      Iy*Ax     -      Iy*Cx )
            float _m = ((-_ab.x * _a.z) + (_ab.x * _pointLeft.z) + (_ab.z * _a.x) - (_ab.z * _pointLeft.x)) / _denominator;

            //  k =    (     Jy*Ax     -      Jy*Cx     -      Jx*Ay     +      Jx*Cy )
            float _k = ((_cd.z * _a.x) - (_cd.z * _pointLeft.x) - (_cd.x * _a.z) + (_cd.x * _pointLeft.z)) / _denominator;


            if ((_m >= 0 && _m <= 1 && _k >= 0 && _k <= 1))
            {
                
                if (Vector3.Distance((_a + _k * _ab), (_pointLeft + _m * _cd)) > .1f)
                {
                    return false;
                }
                
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Chack if two segment intersect and return the intersection point
    /// </summary>
    /// <param name="_a">start of the first segment</param>
    /// <param name="_b">end of the first segment</param>
    /// <param name="_c">start of the second segment</param>
    /// <param name="_d">end of the second segment</param>
    /// <param name="_intersection">Intersection Point</param>
    /// <returns>Does the segments intersect?</returns>
    public static bool IsIntersecting(Vector3 _a, Vector3 _b, Vector3 _c, Vector3 _d, out Vector3 _intersection)
    {
        Vector3 _ab = _b - _a; // I
        float _anglesign = AngleSign(_a, _b, _c);
        Vector3 _cd = _anglesign < 0 ? _d - _c : _c - _d; // J

        Vector3 _pointLeft = _anglesign < 0 ? _c : _d; 

        // P -> Intersection point 
        // P = _a + k * _ab = _c + m * _cd
        // A.x + k*_ab.x = _c.x + m *_cd.x
        // A.y + k*_ab.y = _c.y + m *_cd.y
        // A.z + k*_ab.z = _c.z + m *_cd.z

        float _denominator = Vector3.Cross(_ab, _cd).magnitude;

        if (_denominator != 0)
        {
            //  m =    (     -Ix*A.y      +      Ix*Cy     +      Iy*Ax     -      Iy*Cx )
            float _m = ((-_ab.x * _a.z) + (_ab.x * _pointLeft.z) + (_ab.z * _a.x) - (_ab.z * _pointLeft.x)) / _denominator;

            //  k =    (     Jy*Ax     -      Jy*Cx     -      Jx*Ay     +      Jx*Cy )
            float _k = ((_cd.z * _a.x) - (_cd.z * _pointLeft.x) - (_cd.x * _a.z) + ( _cd.x * _pointLeft.z)) / _denominator;

            //Debug.Log(_m + " " + _k); 

            if ((_m >= 0 && _m <= 1 && _k >= 0 && _k <= 1))
            {

                if (Vector3.Distance((_a + _k * _ab), (_pointLeft + _m * _cd)) > .1f)
                {
                    _intersection = _a;
                    return false;
                }

                _intersection = _a + _k * _ab; 
                return true; 
            }
        }
        _intersection = _a; 
        return false; 
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
    /// Return true if the position is contained in any triangle
    /// </summary>
    /// <param name="_position">Position to check</param>
    /// <param name="_triangles">List of all triangles</param>
    /// <returns></returns>
    public static bool IsInAnyTriangle(Vector3 _position, List<Triangle> _triangles)
    {
        for (int i = 0; i < _triangles.Count; i++)
        {
            if(IsInTriangle(_position, _triangles[i]))
            {
                return true; 
            }
        }
        return false;
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
        foreach (Triangle triangle in triangles)
        {
            if (IsInTriangle(_position, triangle))
            {
                return triangle;
            }
        }
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

    #region Vector3
    /// <summary>
    /// Get the closest position within all triangles
    /// </summary>
    /// <param name="_position">Checked position</param>
    /// <param name="_triangles">All Triangles</param>
    /// <returns></returns>
    public static Vector3 GetClosestPosition(Vector3 _position, List<Triangle> _triangles)
    {
        RaycastHit _hit;
        Vector3 _groundedPosition = _position;
        LayerMask _mask = LayerMask.NameToLayer("Ground"); 
        if (Physics.Raycast(new Ray(_position, Vector3.down), out _hit, 1, _mask))
        {
            _groundedPosition = _hit.point;
        }
        if (IsInAnyTriangle(_groundedPosition, _triangles))
        {

            return _groundedPosition; 
        }
        //Get the closest triangle
        Triangle _triangle = GetTriangleContainingPosition(_position, _triangles);
        //Get the closest point from position into the triangle 
        int _j = 0; 
        for (int i = 0; i < 2; i++)
        {
            _j = i + 1 >= _triangle.Vertices.Length ? 0 : i + 1;
            if (IsIntersecting(_triangle.Vertices[i].Position, _triangle.Vertices[_j].Position, _triangle.CenterPosition, _position, out _groundedPosition))
            {
                // Debug.Log("Return intersection -> " + _groundedPosition);
                return _groundedPosition;
            }
        }

        return _triangle.CenterPosition;
    }

    /// <summary>
    /// Get the grounded position of a selected Position
    /// </summary>
    /// <param name="_position">Position to check</param>
    /// <returns></returns>
    public static Vector3 GetGroundedPosition(Vector3 _position)
    {
        RaycastHit _hit;
        Vector3 _groundedPosition = _position;
        LayerMask _mask = LayerMask.NameToLayer("Ground");
        if (Physics.Raycast(new Ray(_position, Vector3.down), out _hit, 1, _mask))
        {
            _groundedPosition = _hit.point;
            return _groundedPosition;
        }
        return _position; 
    }
    #endregion
    #endregion

    /// <summary>
    /// Compare triangles
    /// if the triangles have more than 1 vertices in common return true
    /// </summary>
    /// <param name="_triangle1">First triangle to compare</param>
    /// <param name="_triangle2">Second triangle to compare</param>
    /// <returns>If the triangles have more than 1 vertex.ices in common</returns>
    public static Vertex[] GetVerticesInCommon(Triangle _triangle1, Triangle _triangle2)
    {
        List<Vertex> _vertices = new List<Vertex>();
        for (int i = 0; i < _triangle1.Vertices.Length; i++)
        {
            for (int j = 0; j < _triangle2.Vertices.Length; j++)
            {
                if (_triangle1.Vertices[i].Position == _triangle2.Vertices[j].Position)
                    _vertices.Add(_triangle1.Vertices[i]);
            }
        }
        return _vertices.ToArray();
    }
    #endregion
}
