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
public class CustomNavMeshManager
{
    #region Fields and properties
    [SerializeField]private static List<Triangle> triangles = new List<Triangle>();
    public static List<Triangle> Triangles { get { return triangles; }  }

    private static string DirectoryPath { get { return Application.dataPath + "/CustomNavDatas"; } }
    #endregion

    #region Methods
    /// <summary>
    /// Get the datas from the dataPath folder to get the navpoints and the triangles
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void LoadDatas()
    {
        CustomNavDataSaver<CustomNavData> _loader = new CustomNavDataSaver<CustomNavData>();
        string _sceneName = SceneManager.GetActiveScene().name;
        CustomNavData _datas = _loader.LoadFile(DirectoryPath, _sceneName);
        triangles = _datas.TrianglesInfos;
    }
    #endregion

}

[Serializable]
public struct CustomNavData
{
    public List<Triangle> TrianglesInfos;
}