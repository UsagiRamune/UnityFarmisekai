using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Atikun.GameDev3.Chapter6
{
    public class GameAppFlowManager : MonoBehaviour
    {
        protected static bool IsSceneOptionLoaded;

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void UnloadScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        public void LoadSceneAdditive(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }

        public void LoadOptionsScene(string optionSceneName)
        {
            if (!IsSceneOptionLoaded)
            {
                SceneManager.LoadScene(optionSceneName, LoadSceneMode.Additive);
                IsSceneOptionLoaded = true;
            }
        }

        public void UnloadOptionsScene(string optionSceneName)
        {
            if (IsSceneOptionLoaded)
            {
                SceneManager.UnloadSceneAsync(optionSceneName);
                IsSceneOptionLoaded = false;
            }
        }

        public void ExitGame()
        {
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        #region Scene Load and Unload Events Handler
        private void OnEnable()
        {
            SceneManager.sceneUnloaded += SceneUnloadEventHandler;
            SceneManager.sceneLoaded += SceneLoadedEventHandler;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= SceneUnloadEventHandler;
            SceneManager.sceneLoaded -= SceneLoadedEventHandler;
        }

        private void SceneUnloadEventHandler(Scene scene)
        {
            
        }

        private void SceneLoadedEventHandler(Scene scene, LoadSceneMode mode)
        {
            if (scene.name.CompareTo("SceneOptions") != 0)
            {
                IsSceneOptionLoaded = false;
            }
        }
        #endregion
    }
}