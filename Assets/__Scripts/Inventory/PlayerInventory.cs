using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Scriptable Objects/PlayerInventory")]
public class PlayerInventory : ScriptableObject
{
    // Default startValue for all playing hands
    private int startValue = 10;
    // Enum of different hand types
    public enum HandType { Rock, Paper, Scissors }
    // Event to be called each time inventory must be rerendered
    public event Action<HandType, int> OnInventoryChanged;

    // Dictionary for player hand types
    private Dictionary<HandType, int> playerHands;

    // Initialize inventory when this ScriptableObject is first enabled
    private void OnEnable()
    {
        if (playerHands == null)
        {
            InitializeInventory();
        }
    }

    // Function to initialize inventory with starting amounts
    private void InitializeInventory()
    {
        playerHands = new Dictionary<HandType, int>
        {
            { HandType.Rock, startValue },
            { HandType.Paper, startValue },
            { HandType.Scissors, startValue }
        };
    }
    
    // Function to reset inventory after a player loss
    public void ResetInventory()
    {
        InitializeInventory(); // Re-initialize with start values
        foreach (var handType in playerHands.Keys)
        {
            OnInventoryChanged?.Invoke(handType, playerHands[handType]);
        }
    }

    // Function to increment/decrement hand count value by 'amount' 
    public void ChangeAmountBy(HandType itemType, int amount)
    {
        if (playerHands.ContainsKey(itemType))
        {
            playerHands[itemType] += amount;
            int newAmount = playerHands[itemType];
            OnInventoryChanged?.Invoke(itemType, newAmount);
        }
        else
        {
            Debug.LogError($"Cannot change item {itemType} because it does not exist in the player inventory.");
        }
    }

    // Function to get a value
    public int GetAmount(HandType itemType)
    {
        if (playerHands.ContainsKey(itemType))
        {
            return playerHands[itemType];
        }
        else
        {
            Debug.LogError($"Cannot find item {itemType} in player inventory.");
            return -1;
        }
    }
    
    // Function to check if a player has a given amount of a hand type
    public bool HasEnough(HandType handType, int amount)
    {
        if (!playerHands.ContainsKey(handType)) return false;
        else if (playerHands[handType] < amount) return false;
        else return true;
    }

    public bool InventoryEmpty()
    {
        return playerHands[HandType.Rock] == 0 && playerHands[HandType.Paper] == 0 && playerHands[HandType.Scissors] == 0;
    }

    // Expose dictionary keys for UI
    public IEnumerable<HandType> GetAvailableHands() => playerHands.Keys;
}



