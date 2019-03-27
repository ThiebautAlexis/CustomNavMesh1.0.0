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

------------------------------------------

Update n°: 002
Updated by: Thiebaut Alexis
Date: 19/03/2019
Description: Adding Editor for the avoidance and detection properties
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
	 *	Editor class of the CustomNavMeshAgent class
     *	    - Display settings of the agent and allow the user to edit them
	 *
	 *	#####################
	 *	### MODIFICATIONS ###
	 *	#####################
	 *
     * Date :			[19/03/2019]
	 *	Author :		[Thiebaut Alexis]
	 *
	 *	Changes :
	 *
	 *	[Adding Detection Settings]
	 *      - Adding DetectionRange, DetectionFieldOfView and DetectionAcuracy properties to the editor and implement them
     *      
	 *	-----------------------------------
     *	
	 *	Date :			[14/02/2019]
	 *	Author :		[Thiebaut Alexis]
	 *
	 *	Changes :
	 *
	 *	[Initialisation of the CustomNavMeshAgentEditor class]
	 *      - Initialize the agent settings: positionOffset, height, radius, baseOffset, speed, detectionRange, steerForce, avoidanceForce, agentPriority
     *      - Implement the OnEnable, OnInspectorGUI and OnSceneGUI Methods to display all settings
	 *	-----------------------------------
	*/

    #region Fields / Properties

    #region SerializedProperties
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.positionOffset"/></summary> of type <see cref="Vector3"/>
    SerializedProperty positionOffset = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.height"/></summary> of type <see cref="float"/>
    SerializedProperty height = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.radius"/></summary> of type <see cref="float"/>
    SerializedProperty radius = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.baseO"/></summary> of type <see cref="float"/>
    SerializedProperty baseOffset = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.height"/></summary> of type <see cref="float"/>
    SerializedProperty speed = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.steerForce"/></summary> of type <see cref="float"/>
    SerializedProperty steerForce = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.avoidanceForce"/></summary> of type <see cref="float"/>
    SerializedProperty avoidanceForce = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.agentPriority"/></summary> of type <see cref="float"/>
    SerializedProperty agentPriority = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.detectionAccuracy"/></summary> of type <see cref="int"/>
    SerializedProperty detectionAccuracy = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.detectionFieldOfView"/></summary> of type <see cref="int"/>
    SerializedProperty detectionFieldOfView = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.detectionRange"/></summary> of type <see cref="float"/>
    SerializedProperty detectionRange = null;
    /// <summary>Serialized Property for <see cref="CustomNavMeshAgent.avoidanceLayer"/></summary> of type <see cref="float"/>
    SerializedProperty avoidanceLayer = null;
    #endregion

    Vector3 centerPosition = Vector3.zero; 
    Vector3 localForward = Vector3.zero; 
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
    private void DrawWireCylinder(Vector3 _pos, float _radius, float _height, Color _color = default(Color))
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

    /// <summary>
    /// Draw the field of view as a solid arc from the origin, oriented with the local forward with a range and an angle
    /// </summary>
    /// <param name="_origin">Origin of the arc</param>
    /// <param name="_localForward">local forward of the arc</param>
    /// <param name="_range">Range of the arc</param>
    /// <param name="_angle">Angle of the arc</param>
    private void DrawFieldOfView(Vector3 _origin, Vector3 _localForward, float _range, int _angle)
    {
        float _totalAngle = Vector3.SignedAngle(Vector3.forward, _localForward, Vector3.up) - (_angle /2);
        Vector3 _start = new Vector3(Mathf.Sin(_totalAngle * Mathf.Deg2Rad), 0, Mathf.Cos(_totalAngle * Mathf.Deg2Rad)).normalized;
        Handles.DrawSolidArc(_origin, Vector3.up, _start, _angle, _range);
    }
    #endregion

    #region Unity Methods
    public override void OnInspectorGUI()
    {
        GUIStyle _headerStyle = new GUIStyle();
        _headerStyle.fontStyle = FontStyle.BoldAndItalic;
        _headerStyle.fontSize = 20;

        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("HelpBox");

        EditorGUILayout.LabelField(serializedObject.targetObject.name, _headerStyle);
        GUILayout.Space(10);
        _headerStyle.fontSize = 12;

        EditorGUILayout.LabelField("GENERAL SETTINGS", _headerStyle);
        EditorGUILayout.PropertyField(positionOffset); 
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(radius);
        EditorGUILayout.PropertyField(baseOffset);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("MOVEMENTS SETTINGS", _headerStyle);
        EditorGUILayout.PropertyField(speed);
        EditorGUILayout.PropertyField(steerForce);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("DETECTION SETTINGS", _headerStyle);
        EditorGUILayout.PropertyField(detectionAccuracy);
        EditorGUILayout.PropertyField(detectionFieldOfView);
        EditorGUILayout.PropertyField(detectionRange);

        EditorGUILayout.Separator();

        EditorGUILayout.LabelField("AVOIDANCE SETTINGS", _headerStyle);
        EditorGUILayout.PropertyField(avoidanceForce);
        EditorGUILayout.PropertyField(agentPriority);

        EditorGUILayout.PropertyField(avoidanceLayer); 

        serializedObject.ApplyModifiedProperties(); 
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Called when Object is selected
    /// Set the target as the CustomNavMeshAgent selected
    /// </summary>
    private void OnEnable()
    {
        //Get serialized Properties
        positionOffset = serializedObject.FindProperty("positionOffset"); 
        height = serializedObject.FindProperty("height");
        radius = serializedObject.FindProperty("radius");
        baseOffset = serializedObject.FindProperty("baseOffset");
        speed = serializedObject.FindProperty("speed");
        steerForce = serializedObject.FindProperty("steerForce");
        avoidanceForce = serializedObject.FindProperty("avoidanceForce");
        agentPriority = serializedObject.FindProperty("agentPriority");
        detectionAccuracy = serializedObject.FindProperty("detectionAccuracy");
        detectionFieldOfView = serializedObject.FindProperty("detectionFieldOfView");
        detectionRange = serializedObject.FindProperty("detectionRange");
        avoidanceLayer = serializedObject.FindProperty("avoidanceLayer"); 
    }

    private void OnSceneGUI()
    {
        centerPosition = (serializedObject.targetObject as CustomNavMeshAgent).CenterPosition;
        localForward=  (serializedObject.targetObject as CustomNavMeshAgent).Velocity;
        DrawWireCylinder(centerPosition, radius.floatValue/2, height.floatValue/2, Color.green);
        Handles.color = new Color(1, 0, 0, .3f);
        DrawFieldOfView(centerPosition, localForward, detectionRange.floatValue, detectionFieldOfView.intValue);
    }
    #endregion

    #endregion
}
