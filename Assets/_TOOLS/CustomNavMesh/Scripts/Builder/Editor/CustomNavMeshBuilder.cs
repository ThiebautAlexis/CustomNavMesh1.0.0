using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics; 
using UnityEngine;
using UnityEngine.AI;
using UnityEditor.SceneManagement;
using UnityEditor;
using UtilsLibrary.GUILibrary;
using UnityEngine.SceneManagement; 
using Debug = UnityEngine.Debug;

/*
[Script Header] CustomNavMeshBuilder Version 0.0.1
Created by: Alexis Thiébaut
Date: 20/12/2018
Description: Improved Version of the CustomNavDataBuilder
The Object doesn't have to be instancied in the scene to build the nav datas

///
[UPDATES]
Update n°: 00 
Updated by: Alexis Thiébaut
Date: 20/12/2018
Description: Set the base of the CustomNavMeshBuilder

Update n°: 01 
Updated by: Alexis Thiébaut
Date: 
Description: Create a version of the builder in editor to remove an object of the scene

Update n°: 02 
Updated by: Alexis Thiébaut
Date: 26/03/2019
Description: Adding visual Debug to draw the navmesh in the editor
*/

public class CustomNavMeshBuilder : EditorWindow 
{

    #region Fields/Properties
    private bool isBuilding = false; 

    private List<Triangle> triangles = new List<Triangle>();

    private List<Vertex> navPoints = new List<Vertex>();

    private string SavingDirectory { get { return Application.dataPath + "/Resources/CustomNavDatas"; } }

    private Material material;
    #endregion

    #region Methods
    [MenuItem("Tools/Custom Nav Mesh/Open")]
    public static void Init()
    {
        CustomNavMeshBuilder _instance = (CustomNavMeshBuilder)GetWindow(typeof(CustomNavMeshBuilder));
        _instance.Show();
    }

    #region List<TDS_NavPoints>
    /// <summary>
    /// Compare triangles
    /// if the triangles have more than 1 vertices in common return true
    /// </summary>
    /// <param name="_triangle1">First triangle to compare</param>
    /// <param name="_triangle2">Second triangle to compare</param>
    /// <returns>If the triangles have more than 1 vertex.ices in common</returns>
    Vertex[] GetVerticesInCommon(Triangle _triangle1, Triangle _triangle2)
    {
        List<Vertex> _vertices = new List<Vertex>();
        for (int i = 0; i < _triangle1.Vertices.Length; i++)
        {
            for (int j = 0; j < _triangle2.Vertices.Length; j++)
            {
                if (_triangle1.Vertices[i] == _triangle2.Vertices[j])
                    _vertices.Add(_triangle1.Vertices[i]);
            }
        }
        return _vertices.ToArray();
    }
    #endregion

    #region void
    public void ClearNavDatas()
    {
        navPoints.Clear();
        triangles.Clear();
    }

    /// <summary>
    /// Get all nav mesh surfaces
    /// Update all navmesh datas for each NavMeshSurfaces
    /// Calculate triangulation 
    /// Add each triangle in the triangulation to a list of triangles
    /// Link Triangles with its neighbors
    /// </summary>
    public void GetNavPointsFromNavSurfaces()
    {
        isBuilding = true; 
        ClearNavDatas(); 
        List<NavMeshSurface> _surfaces = NavMeshSurface.activeSurfaces;
        foreach (NavMeshSurface surface in _surfaces)
        {
            surface.BuildNavMesh();
        }
        NavMeshTriangulation _tr = NavMesh.CalculateTriangulation();
        Vector3[] _vertices = _tr.vertices;
        List<int> _modifiedIndices = _tr.indices.ToList();
        if (_tr.vertices.Length == 0)
        {
            Debug.LogWarning("No Vertices found"); 
            return;
        }
        //GET ALL NAV POINTS
        int _previousIndex = 0;
        for (int i = 0; i < _vertices.Length; i++)
        {
            ////CREATE A POINT AT POSITION
            Vector3 _pos = _vertices[i];
            // CHECK IF NAV POINT ALREADY EXISTS
            if (navPoints.Any(p => p.Position == _pos))
            {
                ////GET THE SAME POINT
                Vertex _existingPoint = navPoints.Where(p => p.Position == _pos).First();
                //// ORDER THE INDEX 
                for (int j = 0; j < _modifiedIndices.Count; j++)
                {
                    // REPLACE INDEX
                    if (_modifiedIndices[j] == _previousIndex)
                    {
                        _modifiedIndices[j] = _existingPoint.ID;
                    }
                    // IF INDEX IS GREATER THAN THE REPLACED INDEX REMOVE ONE FROM THEM
                    if (_modifiedIndices[j] > _previousIndex)
                    {
                        _modifiedIndices[j]--;
                    }
                }
            }
            // IF THE NAV POINT DOESN'T EXISTS ADD IT TO THE DICO
            else
            {
                navPoints.Add(new Vertex(_pos, navPoints.Count));
                _previousIndex++;
            }
        }
        //GET ALL TRIANGLES
        for (int i = 0; i < _modifiedIndices.Count; i += 3)
        {
            Vertex _first = navPoints.Where(p => p.ID == _modifiedIndices[i]).FirstOrDefault();
            Vertex _second = navPoints.Where(p => p.ID == _modifiedIndices[i+1]).FirstOrDefault();
            Vertex _third = navPoints.Where(p => p.ID == _modifiedIndices[i+2]).FirstOrDefault();

            Vertex[] _pointsIndex = new Vertex[3] {_first, _second, _third };
            Triangle _triangle = new Triangle(_pointsIndex);
            triangles.Add(_triangle);
        }
        foreach (Triangle t in triangles)
        {
            LinkTriangles(t);
        }
        isBuilding = false;
        SaveDatas(); 
    }

    /// <summary>
    /// Get all triangles' neighbors and add them to the list of Neighbors
    /// </summary>
    void LinkTriangles(Triangle _triangle)
    {
        //COMPARE CURRENT TRIANGLE WITH EVERY OTHER TRIANGLE
        for (int i = 0; i < triangles.Count; i++)
        {
            // IF TRIANGLES ARE THE SAME, DON'T COMPARE THEM
            if (triangles[i] != _triangle /*&& !triangles[i].HasBeenLinked*/)
            {
                // GET THE VERTICES IN COMMON
                Vertex[] _verticesInCommon = GetVerticesInCommon(_triangle, triangles[i]);
                // CHECK IF THERE IS THE RIGHT AMMOUNT OF VERTICES IN COMMON
                if (_verticesInCommon.Length == 2)
                {
                    _triangle.LinkedTriangles.Add(triangles[i]);
                }
            }
        }
        _triangle.HasBeenLinked = true;
    }

    /// <summary>
    /// Open the resources folder where the datas are saved
    /// </summary>
    public void OpenDirectory()
    {
        if (!Directory.Exists(SavingDirectory)) Directory.CreateDirectory(SavingDirectory);
        Process.Start(SavingDirectory);
    }

    /// <summary>
    /// Get the nav points and save them in binary in a selected directory
    /// 
    /// Sauvegarder les datas dans le dossier resources
    /// Au démarrage du jeu, si les datas ne sont pas dans le data path, on va les chercher dans le dosser resources pour les mettre dans le pdpath
    /// </summary>
    public void SaveDatas()
    {
        if (!Directory.Exists(SavingDirectory)) Directory.CreateDirectory(SavingDirectory);
        CustomNavDataSaver<CustomNavData> _navDataSaver = new CustomNavDataSaver<CustomNavData>();
        CustomNavData _dataSaved = new CustomNavData();
        _dataSaved.TrianglesInfos = triangles;
        _navDataSaver.SaveFile(SavingDirectory, EditorSceneManager.GetActiveScene().name, _dataSaved, ".txt");
    }

    /// <summary>
    /// Load the datas to get the triangles
    /// </summary>
    private void LoadDatas()
    {
        string _fileName = $"CustomNavData_{SceneManager.GetActiveScene().name}";
        TextAsset _textDatas = Resources.Load(Path.Combine("CustomNavDatas", _fileName), typeof(TextAsset)) as TextAsset;
        if (_textDatas != null)
        {
            CustomNavDataSaver<CustomNavData> _loader = new CustomNavDataSaver<CustomNavData>();
            CustomNavData _datas = _loader.DeserializeFileFromTextAsset(_textDatas);
            triangles = _datas.TrianglesInfos;
        }
        else
        {
            Debug.Log("Not found"); 
        }

    }
    #endregion

    #endregion

    #region UnityMethods
    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("CUSTOM NAV MESH BUILDER", MessageType.None, true);
        EditorGUILayout.Space();
        GUITools.ActionButton("Build Navigation Datas", GetNavPointsFromNavSurfaces, Color.white, Color.black);
        if(isBuilding)
        {
            GUITools.ActionButton("Clear Navigation Datas", () => Debug.Log("Building"), Color.grey, Color.white);
        }
        else
        {
            GUITools.ActionButton("Clear Navigation Datas", ClearNavDatas, Color.white, Color.black);
        }
        GUITools.ActionButton("Open Saving Folder", OpenDirectory, Color.white, Color.black); 
        
        if (NavMeshSurface.activeSurfaces.Count == 0) EditorGUILayout.HelpBox("You must add nav mesh surfaces to build the datas", MessageType.Error, true);
    }

    void OnEnable()
    {
        //Create the material necessary to draw the navMesh
        material = new Material(Shader.Find("Specular"));
        LoadDatas(); 
        //Implement the event to draw the navmesh on the scene
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!material) return; 
        foreach (Triangle t in triangles)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = t.Vertices.Select(v => v.Position).ToArray();
            mesh.uv = new Vector2[3]{Vector2.zero, Vector2.zero, Vector2.zero};
            mesh.triangles = new int[3] {0,1,2 };
            Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);

            Handles.DrawAAPolyLine(t.Vertices.Select(v => v.Position).ToArray()); 
        }

        Handles.BeginGUI();

        Handles.EndGUI();
    }
    #endregion
}
