using Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class LoginUI : MonoBehaviour
    {
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button loginButton;
        public TextMeshProUGUI statusText;

        private void Start()
        {
            if (loginButton == null || usernameInput == null || passwordInput == null || statusText == null)
            {
                Debug.LogError("[LoginUI] ไม่ได้ assign field ใน Inspector นะเว้ย!");
                return;
            }
        
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }
    
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                LoginManager.Instance.BypassLogin();
                statusText.text = "Bypass Login success";
                
                HideLoginUI();
            }
        }

        private void OnLoginButtonClicked()
        {
            string username = usernameInput.text;
            string password = passwordInput.text;
        
            LoginManager.Instance.Login(username, password);
            statusText.text = $"Welcome {username}";
            
            HideLoginUI();
        }
        
        private void HideLoginUI()
        {
            usernameInput.gameObject.SetActive(false);
            passwordInput.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);
        }
    }
}