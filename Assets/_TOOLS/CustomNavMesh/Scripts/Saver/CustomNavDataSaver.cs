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

public class CustomNavDataSaver<T>
{
    /// <summary>
    /// Save a serializable object in a path with the name objectName.bin
    /// </summary>
    /// <param name="_path">Path where to save the object</param>
    /// <param name="_objectName">Name of the file to save</param>
    /// <param name="_object">Object to save</param>
    public void SaveFile(string _path, string _objectName, T _object)
    {
        string _name = _object.GetType().ToString()+ "_" + _objectName + ".bin";
        IFormatter _format = new BinaryFormatter();
        Stream _toSave = new FileStream(Path.Combine(_path, _name), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _format.Serialize(_toSave, _object);
        _toSave.Close();
        Debug.Log($"{_name} successfully created in {_path}");
    }

    /// <summary>
    /// Save a serializable object in a path with the name objectName.extension
    /// </summary>
    /// <param name="_path">Path where to save the object</param>
    /// <param name="_objectName">Name of the file to save</param>
    /// <param name="_object">Object to save</param>
    /// <param name="_extension">extension of the file</param>
    public void SaveFile(string _path, string _objectName, T _object, string _extension)
    {
        string _name = _object.GetType().ToString() + "_" + _objectName + _extension;
        IFormatter _format = new BinaryFormatter();
        Stream _toSave = new FileStream(Path.Combine(_path, _name), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _format.Serialize(_toSave, _object);
        _toSave.Close();
        Debug.Log($"{_name} successfully created in {_path}");
    }

    /// <summary>
    /// Load datas for a scene at the path given
    /// </summary>
    /// <param name="_path">Path where the file is saved</param>
    /// <param name="_sceneName">Name of the scene</param>
    /// <returns>Datas for the scene</returns>
    public T LoadFile(string _path, string _sceneName)
    {
        string _name = default(T).GetType().ToString() + "_" + _sceneName + ".bin";
        IFormatter _format = new BinaryFormatter();
        FileStream _toRead = new FileStream(Path.Combine(_path, _name), FileMode.Open, FileAccess.Read);
        T _object = (T)_format.Deserialize(_toRead);
        _toRead.Close();
        return _object;
    }

    /// <summary>
    /// Load datas for a scene at the path given with the given extension
    /// </summary>
    /// <param name="_path">Path where the file is saved</param>
    /// <param name="_sceneName">Name of the scene</param>
    /// <param name="_extension">extension of the file</param>
    /// <returns>Datas for the scene</returns>
    public T LoadFile(string _path, string _sceneName, string _extension)
    {
        string _name = default(T).GetType().ToString() + "_" + _sceneName + _extension;
        IFormatter _format = new BinaryFormatter();
        FileStream _toRead = new FileStream(Path.Combine(_path, _name), FileMode.Open, FileAccess.Read);
        T _object = (T)_format.Deserialize(_toRead);
        _toRead.Close();
        return _object;
    }

    /// <summary>
    /// Deserialize a text asset to get a serilisable object T
    /// </summary>
    /// <param name="_textAsset">textAsset to deserialize</param>
    /// <returns>Serialisable object T</returns>
    public T DeserializeFileFromTextAsset(TextAsset _textAsset)
    {
        IFormatter _format = new BinaryFormatter();
        MemoryStream _toDeserialize = new MemoryStream(_textAsset.bytes);
        T _object = (T)_format.Deserialize(_toDeserialize);
        _toDeserialize.Close();
        return _object; 

    }
}
