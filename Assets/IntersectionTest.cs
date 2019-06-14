using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionTest : MonoBehaviour 
{
    /* IntersectionTest :
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

    [SerializeField] Transform start;
    [SerializeField] Transform end;
    [SerializeField] Transform p1;
    [SerializeField] Transform p2;
    private Vector3 intersection = Vector3.zero; 

    private void OnDrawGizmos()
    {
        if (!start || !end || !p1 || !p2) return; 
        Gizmos.DrawLine(start.position, end.position);
        Gizmos.DrawLine(p1.position, p2.position);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(start.position, .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(end.position, .1f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(p1.position, .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(p2.position, .1f);
        Gizmos.color = Color.magenta; 
        if (GeometryHelper.IsIntersecting(start.position, end.position, p1.position, p2.position, out intersection))
            Gizmos.DrawSphere(intersection, .1f); 
    }
}
