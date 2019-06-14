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
public static class CustomNavMeshManager
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
    public static void InitManager()
    {
        SceneManager.sceneLoaded += LoadDatas;
#if UNITY_EDITOR
        LoadDatas(SceneManager.GetActiveScene(), LoadSceneMode.Additive);
#endif
    }

    public static void LoadDatas(Scene scene, LoadSceneMode _mode)
    {
        string _fileName = $"CustomNavData_{scene.name}";
        TextAsset _textDatas = Resources.Load(Path.Combine(ResourcesPath, _fileName), typeof(TextAsset)) as TextAsset;
        if (_textDatas == null)
        {
            Debug.LogError($"{_fileName} not found.");
            return;
        }
        CustomNavDataSaver<CustomNavData> _loader = new CustomNavDataSaver<CustomNavData>();
        CustomNavData _datas = _loader.DeserializeFileFromTextAsset(_textDatas);
        triangles = _datas.TrianglesInfos;
    }

    /*
    /// <summary>
    /// Update the weight of each triangle
    /// </summary>
    static void UpdateWeights()
    {
        triangles.ForEach(t => t.UpdateWeight()); 
    }
    */
    #endregion

}

[Serializable]
public struct CustomNavData
{
    public List<Triangle> TrianglesInfos;
}