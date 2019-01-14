using System; 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random; 

/*
[Script Header] TDS_NavMeshManager Version 0.0.1
Created by: Alexis Thiébaut
Date: 21/11/2018
Description: 

///
[UPDATES]
Update n°:
Updated by:
Date:
Description:
*/
public class CustomNavMeshManager : MonoBehaviour
{
    #region Fields and properties
    public static CustomNavMeshManager Instance;

    [SerializeField] List<Triangle> triangles = new List<Triangle>();
    public List<Triangle> Triangles { get { return triangles; }  }

    private bool isCalculating = false; 
    public bool IsCalculating { get { return IsCalculating; } } 
    #endregion

    #region Methods

    #region void
    /// <summary>
    /// Get the datas from the dataPath folder to get the navpoints and the triangles
    /// </summary>
    void LoadDatas()
    {
        CustomNavDataSaver<CustomNavData> _loader = new CustomNavDataSaver<CustomNavData>();
        string _sceneName = SceneManager.GetActiveScene().name;
        string _directoryName = CustomNavDataLoader.DirectoryPath;
        CustomNavData _datas = _loader.LoadFile(_directoryName, _sceneName);
        triangles = _datas.TrianglesInfos;
    }

    /// <summary>
    /// Get the datas from the dataPath folder to get the navpoints and the triangles for the scene
    /// </summary>
    /// <param name="_scene"> Scene</param>
    /// <param name="_mode">LoadMode</param>
    void LoadDatas(Scene _scene, LoadSceneMode _mode)
    {
        CustomNavDataSaver<CustomNavData> _loader = new CustomNavDataSaver<CustomNavData>();
        string _sceneName = SceneManager.GetActiveScene().name;
        string _directoryName = CustomNavDataLoader.DirectoryPath;
        CustomNavData _datas = _loader.LoadFile(_directoryName, _sceneName);
        triangles = _datas.TrianglesInfos;
    }
    #endregion

    #endregion

    #region Unity Methods

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return; 
        }
        SceneManager.sceneLoaded += LoadDatas;
    }

    void Start ()
    {
        LoadDatas(); 
    }

    private void OnDrawGizmos()
    {
        
        if (triangles.Count == 0) return;
        foreach (Triangle triangle in triangles)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < triangle.Vertices.Length; i++)
            {
                Gizmos.DrawSphere(triangle.Vertices[i].Position, .1f);
                if (i < 2)
                {
                    Gizmos.DrawLine(triangle.Vertices[i].Position, triangle.Vertices[i + 1].Position);
                }
                else
                {
                    Gizmos.DrawLine(triangle.Vertices[i].Position, triangle.Vertices[0].Position);
                }
            }
            Gizmos.color = Color.red;
            for (int i = 0; i < triangle.LinkedTriangles.Count; i++)
            {
                Gizmos.DrawLine(triangle.CenterPosition, triangle.LinkedTriangles[i].CenterPosition);
            }

        }        
    }
    #endregion

}

[Serializable]
public struct CustomNavData
{
    public List<Triangle> TrianglesInfos;
}