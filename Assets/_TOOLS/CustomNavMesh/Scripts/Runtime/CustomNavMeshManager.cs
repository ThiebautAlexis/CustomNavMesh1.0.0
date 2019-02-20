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

    private static string ResourcesPath { get { return "CustomNavDatas"; } }
    #endregion

    #region Methods
    /// <summary>
    /// Get the datas from the resources folder to get the navpoints and the triangles
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void LoadDatas()
    {
        string _fileName = $"CustomNavData_{SceneManager.GetActiveScene().name}";
        TextAsset _textDatas = Resources.Load(Path.Combine(ResourcesPath, _fileName), typeof(TextAsset)) as TextAsset;
        CustomNavDataSaver<CustomNavData> _loader = new CustomNavDataSaver<CustomNavData>();
        CustomNavData _datas = _loader.DeserializeFileFromTextAsset(_textDatas);
        triangles = _datas.TrianglesInfos;
    }
    #endregion

}

[Serializable]
public struct CustomNavData
{
    public List<Triangle> TrianglesInfos;
}