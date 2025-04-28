using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldBoss : MonoBehaviour
{
    public Canvas promptCanvas;
    public LevelLoader levelLoader;
    
    public BossData bossData;
    public int bossIndex = 0;

    public GameObject blockage;
    
    private TMP_Text promptText;
    
    private bool _playerInRange = false;

    void Start()
    {
        // Initialize prompt to be disabled
        if (promptCanvas == null) Debug.LogWarning("No prompt canvas");
        promptCanvas.gameObject.SetActive(false);
        
        if (levelLoader == null) Debug.LogWarning("No level loader");
        
        // Set prompt text to dynamically change with boss's data
        promptText = promptCanvas.GetComponentInChildren<TMP_Text>();
        if (promptText == null)
        {
            Debug.LogWarning("No TMP_Text assigned in inspector");
        }
        else
        {
            // Set the prompt text dynamically
            if (bossData != null)
            {
                promptText.text = $"Press [E] to challenge {bossData.bossName}";
            }
        }
        
        // If boss has been defeated, set inactive
        if (GameSessionManager.Instance.IsBossDefeated(bossIndex))
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (_playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                GameSessionManager.Instance.currentBossData = bossData;
                levelLoader.LoadBossScene();
            }
        }
    }
    
    // Function to handle entering a boss battle from 2D world map
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        { 
            // Prompt player with instructions
            promptCanvas.gameObject.SetActive(true);
            _playerInRange = true;
        }
    }
    
    // Disable prompt when player leaves boss's radius
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide boss battle prompt
            promptCanvas.gameObject.SetActive(false);
            _playerInRange = false;
        }
    }
}
