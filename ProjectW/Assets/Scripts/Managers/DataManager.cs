using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class DataManager
{
    // Start is called before the first frame update
    public static DataManager s_instance = new DataManager();

    public void SaveJsonFile<T>(T data, string jsonName)
    {
        string path = Path.Combine(Application.dataPath, "JsonData/" + jsonName + ".json");
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, jsonData);
    }
    public T LoadJsonFile<T>(T data, string jsonName)
    {
        string directoryPath = Path.Combine(Application.dataPath, "JsonData");
        if (!Directory.Exists(directoryPath))
        {
            Debug.LogError("JsonData folder doesn't exist.");
            Directory.CreateDirectory(directoryPath);
        }
        string path = Path.Combine(Application.dataPath, "JsonData/" + jsonName + ".json");
        if (!File.Exists(path))
        {
            Debug.LogError($"{jsonName}.json file doesn't exist.");
            SaveJsonFile<T>(data, jsonName);
        }
        string jsonData = File.ReadAllText(path);
        T dummyData = JsonUtility.FromJson<T>(jsonData);
        return dummyData;
    }
}
