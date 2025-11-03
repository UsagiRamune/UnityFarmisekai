using Data;
using UnityEngine;

namespace Manager
{
    public class LoginManager : MonoBehaviour
    {
        public static LoginManager Instance;
    
        public UserData userData;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        
            DontDestroyOnLoad(gameObject);
        }

        public void Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("[LoginManager] Username or password is empty.");
                return;
            }

            userData = new UserData
            {
                username = username,
                userID = System.Guid.NewGuid().ToString()
            };
        
            SaveManager.Save(userData);
            Debug.Log($"[LoginManager] Login success. Welcome, {userData.username}");
        }

        public void BypassLogin()
        {
            userData = new UserData
            {
                username = "DevTester",
                userID = "bypass-mode"
            };
            
            SaveManager.Save(userData);
            Debug.Log("[LoginManager] Bypassed login success.");
        }
    }
}