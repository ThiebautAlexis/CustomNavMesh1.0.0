using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/*
[Script Header] CustomNavMeshAgentEditor Version 0.0.1
Created by: Thiebaut Alexis 
Date: 21/01/2019
Description: Editor of the Agent
             - Show visual debug

///
[UPDATES]
Update n°: 001
Updated by: Thiebaut Alexis
Date: 14/01/2019
Description: - Creating the method DrawWireCylinder that can draw the height and the radius of the agent
             - Call the method in OnSceneGUI() method 
*/

[CustomEditor(typeof(CustomNavMeshAgent))]
public class CustomNavMeshAgentEditor : Editor 
{
    /* CustomNavMeshAgentEditor :
	 *
	 *	#####################
	 *	###### PURPOSE ######
	 *	#####################
	 *
	 *	[PURPOSE]
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

    #region Fields / Properties
    private CustomNavMeshAgent p_target;
    #endregion

    #region Methods

    #region Original Methods
    /// <summary>
    /// Draw a wire cylinder with a height and a radius
    /// </summary>
    /// <param name="_pos">center position</param>
    /// <param name="_radius">radius of the cylinder</param>
    /// <param name="_height">height of the cylinder</param>
    /// <param name="_color">color of the cylinder</param>
    public void DrawWireCylinder(Vector3 _pos, float _radius, float _height, Color _color = default(Color))
    {
        if (_color != default(Color))
            Handles.color = _color;

        //draw sideways
        Handles.DrawLine(_pos + new Vector3(0, _height, -_radius), _pos + new Vector3(0, -_height, -_radius));
        Handles.DrawLine(_pos + new Vector3(0, _height, _radius), _pos + new Vector3(0, -_height, _radius));
        //draw frontways
        Handles.DrawLine(_pos + new Vector3(-_radius, _height, 0), _pos + new Vector3(-_radius, -_height, 0));
        Handles.DrawLine(_pos + new Vector3(_radius, _height, 0), _pos + new Vector3(_radius, -_height, 0));
        //draw center
        Handles.DrawWireDisc(_pos + Vector3.up * _height, Vector3.up, _radius);
        Handles.DrawWireDisc(_pos + Vector3.down * _height, Vector3.up, _radius);

    }
    #endregion

    #region Unity Methods
    /// <summary>
    /// Called when Object is selected
    /// Set the target as the CustomNavMeshAgent selected
    /// </summary>
    private void OnEnable()
    {
        p_target = (CustomNavMeshAgent)target; 
    }

    private void OnSceneGUI()
    {
        DrawWireCylinder(p_target.transform.position, p_target.Radius, p_target.Height, Color.green);
    }
    #endregion

    #endregion
}
