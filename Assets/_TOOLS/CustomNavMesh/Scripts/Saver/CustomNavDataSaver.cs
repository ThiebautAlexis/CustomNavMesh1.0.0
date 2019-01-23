using System; 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary; 
using UnityEngine;

public class CustomNavDataSaver<T>
{
    public void SaveFile(string _path, string _objectName, T _object)
    {
        string _name = _object.GetType().ToString()+ "_" + _objectName + ".bin";
        IFormatter _format = new BinaryFormatter();
        Stream _toSave = new FileStream(Path.Combine(_path, _name), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _format.Serialize(_toSave, _object);
        _toSave.Close();
        Debug.Log($"{_name} successfully created in {_path}");
    }

    public void SaveFile(string _path, string _objectName, T _object, string _extension)
    {
        string _name = _object.GetType().ToString() + "_" + _objectName + _extension;
        IFormatter _format = new BinaryFormatter();
        Stream _toSave = new FileStream(Path.Combine(_path, _name), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        _format.Serialize(_toSave, _object);
        _toSave.Close();
        Debug.Log($"{_name} successfully created in {_path}");
    }

    public T LoadFile(string _path, string _sceneName)
    {
        string _name = default(T).GetType().ToString() + "_" + _sceneName + ".bin";
        IFormatter _format = new BinaryFormatter();
        FileStream _toRead = new FileStream(Path.Combine(_path, _name), FileMode.Open, FileAccess.Read);
        T _object = (T)_format.Deserialize(_toRead);
        _toRead.Close();
        return _object;
    }

    public T LoadFile(string _path, string _sceneName, string _extension)
    {
        string _name = default(T).GetType().ToString() + "_" + _sceneName + _extension;
        IFormatter _format = new BinaryFormatter();
        FileStream _toRead = new FileStream(Path.Combine(_path, _name), FileMode.Open, FileAccess.Read);
        T _object = (T)_format.Deserialize(_toRead);
        _toRead.Close();
        return _object;
    }
}
