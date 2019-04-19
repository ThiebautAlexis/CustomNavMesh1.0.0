using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour 
{
    /* Test :
	 *
	 *	#####################
	 *	###### PURPOSE ######
	 *	#####################
	 *
	 *	[PURPOSE]
	 *
	 *	#####################
	 *	####### TO DO #######
	 *	#####################
	 *
	 *	[TO DO]
	 *
	 *	#####################
	 *	### MODIFICATIONS ###
	 *	#####################
	 *
	 *	Date :			[DATE]
	 *	Author :		[NAME]
	 *
	 *	Changes :
	 *
	 *	[CHANGES]
	 *
	 *	-----------------------------------
	*/

    #region Events

    #endregion

    #region Fields / Properties
    [SerializeField] Vector3[] _points;
    private Vector3 _intersectionPoint = Vector3.zero; 
	#endregion

	#region Methods

	#region Original Methods

	#endregion

	#region Unity Methods
	// Awake is called when the script instance is being loaded
    private void Awake()
    {

    }

	// Use this for initialization
    private void Start()
    {
		
    }
	
	// Update is called once per frame
	private void Update()
    {
        if (GeometryHelper.IsIntersecting(_points[0], _points[1], _points[2], _points[3], out _intersectionPoint))
        {
            Debug.Log("Intersect"); 
        }

    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < 4 ; i++)
        {
            if(i < 2)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.blue; 
            }
            Gizmos.DrawSphere(_points[i], 1f); 
        }
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_points[0], _points[1]);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_points[2], _points[3]);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_intersectionPoint,1); 
    }
    #endregion

    #endregion
}
