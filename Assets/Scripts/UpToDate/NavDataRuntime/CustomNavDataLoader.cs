using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


/*
[Script Header] TDS_NavDataLoader Version 0.0.1
Created by: Alexis Thiébaut
Date: 21/11 /2018
Description: On the start of the game, get all nav datas from the Resources folder and create a copy of them in the data path folder

///
[UPDATES]
Update n°:
Updated by:
Date:
Description:
*/
public static class CustomNavDataLoader
{
    #region Fields and Properties
    public static string DirectoryPath { get { return Path.Combine(Application.dataPath, "CustomNavDatas");  } }
    #endregion

    #region Methods
    /// <summary>
    /// IF THE NAV DATAS AREN'T IN THE DATA PATH, CREATE A COPY OF THE DATAS IN .bin IN THE DATA PATH 
    /// </summary>
    public static void MigrateNavDatas()
    {
        //If the directory doesn't exists, create it
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }
        //for each scene, get the navdata in the resources folder and write all bytes in the Directory Path
        byte[] _datas;
        string _fileName = string.Empty; 
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            //FILE NAME = CustomNavData_[SceneName].txt
            _fileName = "CustomNavData_" + SceneManager.GetSceneAt(i).name; 
            //GET THE .txt FILE WITH NAVDATAS
            TextAsset _text = Resources.Load<TextAsset>(Path.Combine("CustomNavDatas", _fileName));
            if (_text != null)
            {
                _datas = _text.bytes;
                string _filePath = Path.Combine(DirectoryPath, _fileName) + ".bin";

                /* Replaced
                //If the file already exists
                if (File.Exists(_filePath))
                {
                    byte[] _existingData = File.ReadAllBytes(_filePath); 
                    // and if the length is different from the other file, write it over the another one
                    if(_existingData.Length != _datas.Length)
                    {
                        File.WriteAllBytes(_filePath, _datas);
                    }
                }
                //If the file doesn't exists, create the file
                else
                {
                    File.WriteAllBytes(_filePath, _datas);
                }
                */
                // WRITE THE FILE ANYWAY
                File.WriteAllBytes(_filePath, _datas);
                Debug.Log($"{_fileName} successfully created in binary file"); 
            }
        }
    }
    #endregion
}
