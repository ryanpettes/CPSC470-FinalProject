using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public GameObject player;
    
    // Related to transition animation
    public Animator transition;
    public float transitionTime = 1.0f;
    
    // Related to other UI elements
    public Canvas inventoryCanvas;

    // Scene indices
    private int _worldMapIndex = 1;
    private int _bossSceneIndex = 2;

    void Awake()
    {
        if (transition == null) Debug.LogWarning("No transition found");
    }

    // Function to load world map scene
    public void Load2DScene()
    {
        StartCoroutine(LoadScene(_worldMapIndex));
    }
    
    // Function to load boss scene
    public void LoadBossScene()
    {
        GameSessionManager.Instance.SaveWorldPlayerPosition(player.transform.position);
        StartCoroutine(LoadScene(_bossSceneIndex));
    }

    // Coroutine to play scene transition
    IEnumerator LoadScene(int levelIndex)
    {
        // Hide UI
        if (inventoryCanvas != null) inventoryCanvas.gameObject.SetActive(false);
        
        // Play transition animation
        transition.SetTrigger("Start");    
        
        // Wait
        yield return new WaitForSeconds(transitionTime);
        
        // Load scene
        SceneManager.LoadScene(levelIndex);
        
        // Display UI
        if(inventoryCanvas != null) inventoryCanvas.gameObject.SetActive(true);
    }
}
