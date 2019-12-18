using System; 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary; 
using UnityEngine;

/*
[Script Header] CustomNavDataSaver Version 0.0.1
Created by: Thiebaut Alexis 
Date: 14/01/2019
Description: Class to save and load files

///
[UPDATES]
Update n°: 001
Updated by: Thiebaut Alexis 
Date: 14/01/2019
Description: Creation of the SaveFile and LoadFiles methods
*/

public class CustomNavDataSaver
{
    /// <summary>
    /// Save a serializable object in a path with the name objectName.bin
    /// </summary>
    /// <param name="_path">Path where to save the object</param>
    /// <param name="_objectName">Name of the file to save</param>
    /// <param name="_object">Object to save</param>
    public void SaveFile(string _path, string _objectName, CustomNavData _object)
    {
        string _name = _object.GetType().ToString()+ "_" + _objectName;
        if (File.Exists(Path.Combine(_path, _name) + ".meta"))
        {
            File.Delete(Path.Combine(_path, _name) + ".meta");
            Debug.Log("Delete .meta");
        }
        File.WriteAllText(Path.Combine(_path, _name) + ".txt", JsonUtility.ToJson(_object));
        Debug.Log($"{_name} successfully created in {_path}");
    }

    /// <summary>
    /// Load datas for a scene at the path given
    /// </summary>
    /// <param name="_path">Path where the file is saved</param>
    /// <param name="_sceneName">Name of the scene</param>
    /// <returns>Datas for the scene</returns>
    public CustomNavData LoadFile(string _path, string _sceneName)
    {
        string _name = "CustomNavData_" + _sceneName + ".txt";
        CustomNavData _obj = JsonUtility.FromJson<CustomNavData>(File.ReadAllText(Path.Combine(_path, _name)));
        return _obj;
    }

    /// <summary>
    /// Deserialize a text asset to get a serilisable object T
    /// </summary>
    /// <param name="_textAsset">textAsset to deserialize</param>
    /// <returns>Serialisable object T</returns>
    public CustomNavData DeserializeFileFromTextAsset(TextAsset _textAsset)
    {
        IFormatter _format = new BinaryFormatter();
        MemoryStream _toDeserialize = new MemoryStream(_textAsset.bytes);
        CustomNavData _object = (CustomNavData)_format.Deserialize(_toDeserialize);
        _toDeserialize.Close();
        return _object; 

    }
}
