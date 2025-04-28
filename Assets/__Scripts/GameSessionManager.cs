using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance;

    private Vector3 playerStartPosition = new Vector3(-1, 6, 0);
    public Vector3 worldPlayerPosition;
    public BossData currentBossData;
    public int currentBossIndex;
    public List<int> defeatedBosses = new List<int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    public void SaveWorldPlayerPosition(Vector3 position)
    {
        worldPlayerPosition = position;
    }

    public void MarkBossAsDefeated(int bossIndex)
    {
        if (!defeatedBosses.Contains(bossIndex))
        {
            defeatedBosses.Add(bossIndex);
            currentBossIndex++;
        }
    }

    public bool IsBossDefeated(int bossIndex)
    {
        return defeatedBosses.Contains(bossIndex);
    }
    
    public void ResetGameSession()
    {
        defeatedBosses.Clear();
        worldPlayerPosition = playerStartPosition;
    }
}
