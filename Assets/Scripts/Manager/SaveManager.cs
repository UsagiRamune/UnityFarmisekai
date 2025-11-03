using System.IO;
using Data;
using UnityEngine;

namespace Manager
{
    public static class SaveManager
    {
        private static string fileName = "userdata.json";

        public static void Save(UserData data)
        {
            string json = JsonUtility.ToJson(data, true);
            string path = GetFilePath();
            File.WriteAllText(path, json);
            Debug.Log($"[SaveManager] Saved tp {path}");
        }

        public static UserData Load()
        {
            string path = GetFilePath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<UserData>(json);
            }
            else
            {
                Debug.LogWarning("[SaveManager] No save file found. Creating new user.");
                return new UserData();
            }
        }

        private static string GetFilePath()
        {
            return Path.Combine(Application.persistentDataPath, fileName);
        }
    }
}