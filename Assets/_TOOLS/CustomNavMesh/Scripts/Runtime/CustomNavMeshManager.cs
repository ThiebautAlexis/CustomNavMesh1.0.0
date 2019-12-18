using System; 
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/*
[Script Header] TDS_NavMeshManager Version 0.0.1
Created by: Alexis Thiébaut
Date: 21/11/2018
Description: Manager of the navdatas
             - Load datas for the current scene
             - Contains all datas of the scene stocked in a list of triangles 

///
[UPDATES]
Update n°: 001 
Updated by: Thiebaut Alexis
Date: 21/11/2018
Description: Initialisation of the manager
             - Create the list of triangles 
             - Create the loading datas method to get the informations

[UPDATES]
Update n°: 002 
Updated by: Thiebaut Alexis
Date: 12/02/2019
Description: Removing the Monobehaviour Behaviour and using RuntimeInitializeOnLoadMethod attribute 
*/
public static class CustomNavMeshManager
{
    #region Fields and properties
    [SerializeField] private static List<Triangle> triangles = new List<Triangle>();
    public static List<Triangle> Triangles { get { return triangles; }  }

    private static string ResourcesPath { get { return "CustomNavDatas"; } }
    #endregion

    #region Methods
    /// <summary>
    /// Get the datas from the resources folder to get the navpoints and the triangles
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitManager()
    {
        SceneManager.sceneLoaded += LoadDatas;
#if UNITY_EDITOR
        LoadDatas(SceneManager.GetActiveScene(), LoadSceneMode.Additive);
#endif
    }

    /// <summary>
    /// Load Nav Mesh Datas on the loading of the scene
    /// </summary>
    /// <param name="scene">Loaded Scene</param>
    /// <param name="_mode">Load Scene Mode</param>
    public static void LoadDatas(Scene scene, LoadSceneMode _mode)
    {
        CustomNavDataSaver _loader = new CustomNavDataSaver();
        //CustomNavData _datas = _loader.LoadFile(Path.Combine(Application.dataPath, "Resources", ResourcesPath), scene.name);
        TextAsset _text = (Resources.Load(Path.Combine(ResourcesPath, "CustomNavData_" + scene.name))) as TextAsset; 
        if(_text == null) return; 
        CustomNavData _datas = JsonUtility.FromJson<CustomNavData>(_text.text); 
        triangles = _datas.TrianglesInfos;
        foreach (Triangle triangle in triangles)
        {
            LinkTriangles(triangle);
        }
    }

    /// <summary>
    /// Get all triangles' neighbors and add them to the list of Neighbors
    /// </summary>
    public static void LinkTriangles(Triangle _triangle)
    {
        if (_triangle.LinkedTriangles == null) _triangle.LinkedTriangles = new List<Triangle>(); 
        //COMPARE CURRENT TRIANGLE WITH EVERY OTHER TRIANGLE
        for (int i = 0; i < triangles.Count; i++)
        {
            // IF TRIANGLES ARE THE SAME, DON'T COMPARE THEM
            if (triangles[i] != _triangle /*&& !triangles[i].HasBeenLinked*/)
            {
                // GET THE VERTICES IN COMMON
                Vertex[] _verticesInCommon = GeometryHelper.GetVerticesInCommon(_triangle, triangles[i]);
                // CHECK IF THERE IS THE RIGHT AMMOUNT OF VERTICES IN COMMON
                if (_verticesInCommon.Length == 2)
                {
                    _triangle.LinkedTriangles.Add(triangles[i]);
                }
            }
        }
        _triangle.HasBeenLinked = true;
    }
    #endregion

}

[Serializable]
public class CustomNavData
{
    public List<Triangle> TrianglesInfos;
}