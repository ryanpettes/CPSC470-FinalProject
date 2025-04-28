using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public LevelLoader levelLoader;
    public static event Action<bool> OnBattleStateChanged;
    public static event Action<PlayerInventory.HandType?> OnRestrictedHandChanged;
    private BossData bossData;
    
    // Scoring variables
    private int playerScore = 0;
    private int bossScore = 0;
    private int _scoreToWin = 7;
    
    // Variables related to scoring gameObjects (gold bars, in this case)
    public GameObject playerGoldBars;
    public Transform[] playerGoldContainers;
    public GameObject bossGoldBars;
    public Transform[] bossGoldContainers;
    public GameObject goldBarPrefab;
    
    // Models for rocks, papers, scissors
    public GameObject[] handTypeModels;
    public Transform[] playerSideModelTransforms;
    public Transform[] bossSideModelTransforms;
    
    // Variables related to UI prompts and results for the player
    public GameObject playHandButton;
    public TMP_Text resultText;
    public GameObject endGamePanel;
    public TMP_Text endGameText;
    public Button retryButton;
    public Button returnToMapButton;
    private Coroutine _messageCoroutine;
    private Coroutine _typeCoroutine;
    private float _messageDuration = 2f;
    private float _messageTypeSpeed = 0.05f;
    
    // Cameras
    public CinemachineCamera vCamMain;
    public CinemachineCamera vCamTopDown;
    private float _cameraLerpDuration = 2f;

    // Track currently selected hand of player
    private PlayerInventory.HandType? selectedHand = null;

    // Audio/particles
    public AudioClip itemSpawnSound;
    public AudioClip itemDestroySound;
    public AudioClip winSound;
    public AudioClip loseSound;
    public GameObject dustEffectPrefab;

    private void Awake()
    {
        bossData = GameSessionManager.Instance.currentBossData;

        if (bossData == null)
        {
            Debug.LogError("No BossData loaded for this battle!");
            return;
        }

        SetupBattle();
    }
    
    private void Start()
    {
        SetupBattle();
    }
    
    private void SetupBattle()
    {
        // Make sure inventory buttons are interactable
        OnBattleStateChanged?.Invoke(true);
        
        returnToMapButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        
        playerScore = bossScore = 0;
        selectedHand = null;
        playHandButton.SetActive(false);
        endGamePanel.SetActive(false);
        
        if (bossData is RestrictRandomHandBoss randomRestrictBoss)
        {
            randomRestrictBoss.SetNewRestrictedHand();
            OnRestrictedHandChanged?.Invoke(randomRestrictBoss.GetRestrictedHand());
        }
        
        if (_typeCoroutine != null)
            StopCoroutine(_typeCoroutine);

        _typeCoroutine = StartCoroutine(TypeText("Select your hand...", _messageTypeSpeed));
    }

    // Function to track which hand player has selected
    public void OnHandSelected(PlayerInventory.HandType hand)
    {
        if (bossData.IsHandRestricted(hand))
        {
            return;
        }
        
        selectedHand = hand;
        // Prompt player to actually PLAY the hand
        playHandButton.SetActive(true);
    }
    
    // Coroutine wrapper for "Play Hand" button
    public void PlayHandButtonClicked()
    {
        StartCoroutine(OnPlayHandButtonClicked());
    }

    // Function to handle logic after player elects to PLAY a hand
    public IEnumerator OnPlayHandButtonClicked()
    {
        if (selectedHand == null) yield break;

        // Check if player has enough of hand type to play
        int costOfHand = bossData.GetHandCost(selectedHand.Value);
        if (!playerInventory.HasEnough(selectedHand.Value, costOfHand))
        {
            if (_messageCoroutine != null) 
                StopCoroutine(_messageCoroutine);
        
            _messageCoroutine = StartCoroutine(DisplayMessageForSeconds("Not enough resources...", _messageDuration));
            yield break;
        }
    
        playerInventory.ChangeAmountBy(selectedHand.Value, -costOfHand);

        // Determine boss hand
        PlayerInventory.HandType bossHand = bossData.ChooseHand(playerInventory);

        yield return StartCoroutine(HandleRound(selectedHand.Value, bossHand));
        
        if (bossData is RestrictRandomHandBoss randomRestrictBoss)
        {
            randomRestrictBoss.SetNewRestrictedHand();
            OnRestrictedHandChanged?.Invoke(randomRestrictBoss.GetRestrictedHand());
        }
        
        if (CheckForEndGame())
        {
            yield break; // Stop early if game over
        }

        // Reset for next round
        selectedHand = null;
        playHandButton.SetActive(false);
    }
    
    // Coroutine to handle round-based logic (transitions, spawning, etc.)
    private IEnumerator HandleRound(PlayerInventory.HandType playerHand, PlayerInventory.HandType bossHand)
    {
        // Disable clicks
        OnBattleStateChanged?.Invoke(false);
        // Disable play hand button
        playHandButton.SetActive(false);
        
        if (_typeCoroutine != null) 
            StopCoroutine(_typeCoroutine);
        _typeCoroutine = StartCoroutine(TypeText("You selected " + selectedHand.Value, _messageTypeSpeed));
        
        MoveCameraTopDown();
        yield return new WaitForSeconds(_cameraLerpDuration);
        
        GameObject playerHandPrefab = SpawnHand(playerHand, 0);
        yield return new WaitForSeconds(1f);
        
        GameObject bossHandPrefab = SpawnHand(bossHand, 1);
        yield return new WaitForSeconds(2f);
        
        string roundResult = DetermineRoundWinner(playerHand, bossHand);
        
        if (roundResult == "Draw")
        {
            StartCoroutine(DestroyHand(playerHandPrefab));
            StartCoroutine(DestroyHand(bossHandPrefab));
        } 
        else if (roundResult == "Player")
        {
            SfxManager.Instance.PlaySoundEffect(winSound);
            yield return StartCoroutine(DestroyHand(bossHandPrefab));
        }
        else
        {
            SfxManager.Instance.PlaySoundEffect(loseSound);
            yield return StartCoroutine(DestroyHand(playerHandPrefab));
        }
        yield return new WaitForSeconds(1.5f);
        
        MoveCameraBackToOriginal();
        UpdateScoreOf(roundResult);
        yield return new WaitForSeconds(_cameraLerpDuration);
        
        if (roundResult == "Player")
            yield return StartCoroutine(DestroyHand(playerHandPrefab));
        else if (roundResult == "Boss")
            yield return StartCoroutine(DestroyHand(bossHandPrefab));
        
        // Enable clicks
        OnBattleStateChanged?.Invoke(true);
    }
    
    // Function to spawn a hand on the table (0 == player's side, 1 == boss's side)
    private GameObject SpawnHand(PlayerInventory.HandType handType, int side)
    {
        SfxManager.Instance.PlaySoundEffect(itemSpawnSound);
        GameObject handInstance = null;
        
        if (side == 0)
        {
            handInstance = Instantiate(handTypeModels[(int)handType], 
                playerSideModelTransforms[(int)handType].position, 
                playerSideModelTransforms[(int)handType].rotation);
        }
        else
        {
            handInstance = Instantiate(handTypeModels[(int)handType], 
                bossSideModelTransforms[(int)handType].position, 
                bossSideModelTransforms[(int)handType].rotation);
        }

        return handInstance;
    }
    
    // Coroutine to fade out an object
    private IEnumerator DestroyHand(GameObject hand)
    {
        // Spawn dust
        GameObject dust = Instantiate(dustEffectPrefab, hand.transform.position, Quaternion.identity);
        // Play sound
        SfxManager.Instance.PlaySoundEffect(itemDestroySound);
        Destroy(hand);
        
        // Destroy dust gameObject
        ParticleSystem dustParticles = dust.GetComponent<ParticleSystem>();
        if (dustParticles != null)
        {
            Destroy(dust, dustParticles.main.duration + dustParticles.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(dust, 2f);
        }

        yield return null;
    }
    
    // Coroutine to "fly" gold bar over to the appropriate side of table
    private IEnumerator DissolveGoldBarToWinner(string winner)
    {
        Vector3 startPos = new Vector3(2.26f, 0.5f, -6.6f);
        Vector3 targetPos;
        Quaternion targetRot;

        targetPos = (winner == "Player") ? playerGoldContainers[playerScore].position : bossGoldContainers[bossScore].position;
        targetRot = (winner == "Player") ? playerGoldContainers[playerScore].rotation : bossGoldContainers[bossScore].rotation;
        
        // Instantiate gold bar
        GameObject bar = Instantiate(goldBarPrefab, startPos, targetRot);
        
        // Set parent
        if (winner == "Player") bar.transform.SetParent(playerGoldBars.transform);
        else bar.transform.SetParent(bossGoldBars.transform);

        float duration = 1.0f; // time it takes to reach the target
        float elapsed = 0f; // elapsed time for lerping

        Vector3 initialPos = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
        
            // Smooth interpolation
            t = Mathf.SmoothStep(0, 1, t);

            bar.transform.position = Vector3.Lerp(initialPos, targetPos, t);

            yield return null;
        }

        // Snap to final position and rotation
        bar.transform.position = targetPos;
        bar.transform.rotation = targetRot;
    }
    
    // Coroutine to display a message for a number of seconds
    private IEnumerator DisplayMessageForSeconds(string message, float seconds)
    {
        if (_typeCoroutine != null)
            StopCoroutine(_typeCoroutine);
        yield return _typeCoroutine = StartCoroutine(TypeText(message, _messageTypeSpeed));
        
        yield return new WaitForSeconds(seconds);
        
        if (!endGamePanel.activeSelf)
        {
            if (_typeCoroutine != null)
                StopCoroutine(_typeCoroutine);
            yield return _typeCoroutine = StartCoroutine(TypeText("Select your hand...", _messageTypeSpeed));
        }
    }
    
    // Coroutine for typewriter effect while updating text
    private IEnumerator TypeText(string message, float typeSpeed)
    {
        resultText.text = "";
        
        for (int i = 0; i < message.Length; i++)
        {
            resultText.text += message[i];
            yield return new WaitForSeconds(typeSpeed);
        }
    }
    
    // Function to instantiate gold bar on winner's side
    private void UpdateScoreOf(string winner)
    {
        if (_messageCoroutine != null)
            StopCoroutine(_messageCoroutine);

        if (winner == "Draw")
            _messageCoroutine = StartCoroutine(DisplayMessageForSeconds("Draw", _messageDuration));
        else
        {
            StartCoroutine(DissolveGoldBarToWinner(winner));
            if (winner == "Player")
            {
                _messageCoroutine = StartCoroutine(DisplayMessageForSeconds("You Win", _messageDuration));
                playerScore++;
            }
            else if (winner == "Boss")
            {
                _messageCoroutine = StartCoroutine(DisplayMessageForSeconds("Boss Wins", _messageDuration));
                bossScore++;
            }
        }
    }

    // Function to determine who wins a round
    private string DetermineRoundWinner(PlayerInventory.HandType playerHand, PlayerInventory.HandType bossHand)
    {
        if (playerHand == bossHand) return "Draw";

        if (playerHand == PlayerInventory.HandType.Rock && bossHand == PlayerInventory.HandType.Scissors ||
            playerHand == PlayerInventory.HandType.Scissors && bossHand == PlayerInventory.HandType.Paper ||
            playerHand == PlayerInventory.HandType.Paper && bossHand == PlayerInventory.HandType.Rock)
        {
            return "Player";
        }
        else
        {
            return "Boss";
        }
    }

    // Function to check if game is over
    private bool CheckForEndGame()
    {
        if (playerScore >= _scoreToWin)
        {
            EndGame(true);
            return true;
        }
        else if (bossScore >= _scoreToWin || playerInventory.InventoryEmpty())
        {
            EndGame(false);
            return true;
        }
        return false;
    }
    
    // Function to handle game over scenario
    //   'isWin' = true iff player wins game, else false
    private void EndGame(bool isWin)
    {
        // Disable inventory
        OnBattleStateChanged?.Invoke(false);
        endGamePanel.SetActive(true);
        endGameText.text = isWin ? "Victory!" : "Defeat!";

        if (isWin)
        {
            RewardPlayer();
            returnToMapButton.gameObject.SetActive(true);
        }
        else
        {
            retryButton.gameObject.SetActive(true);
        }
    }
    
    // Reward player with hands for winning
    private void RewardPlayer()
    {
        List<PlayerInventory.HandType> handTypes = new List<PlayerInventory.HandType>(playerInventory.GetAvailableHands());
    
        foreach (PlayerInventory.HandType handType in handTypes)
        {
            playerInventory.ChangeAmountBy(handType, 10);
        }
    }
    
    // Function to move camera to top-down view of table
    void MoveCameraTopDown()
    {
        vCamMain.Priority = 0;
        vCamTopDown.Priority = 1;
    }
    
    // Function to move camera back to main position
    void MoveCameraBackToOriginal()
    {
        vCamMain.Priority = 1;
        vCamTopDown.Priority = 0;
    }
    
    // Hooks for Retry or Return to Map buttons:
    public void OnRetryButton()
    {
        DestroyBars();
        // Send player back to world map, restarting game
        playerInventory.ResetInventory();
        GameSessionManager.Instance.ResetGameSession();
        levelLoader.Load2DScene();
    }

    public void OnReturnToMapButton()
    {
        // Return to map
        GameSessionManager.Instance.MarkBossAsDefeated(GameSessionManager.Instance.currentBossIndex);
        levelLoader.Load2DScene();
    }
    
    // Destroy all instantiated gameObjects
    private void DestroyBars()
    {
        for (int i = _scoreToWin; i < playerGoldBars.transform.childCount; i++)
        {
            Destroy(playerGoldBars.transform.GetChild(i).gameObject);
        }

        for (int i = _scoreToWin; i < bossGoldBars.transform.childCount; i++)
        {
            Destroy(bossGoldBars.transform.GetChild(i).gameObject);
        }
    }
}
