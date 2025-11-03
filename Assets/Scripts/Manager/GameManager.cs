using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public float playerDamage = 25f;
    
    public float playerMaxHealth = 100f;

    [Header("Enemy Stats")]
    public float meleeDamage = 15f;
    public float rangedDamage = 10f;
    public float enemyMaxHealth = 100f; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
}