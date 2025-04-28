using UnityEngine;

[CreateAssetMenu(fileName = "BossData", menuName = "Scriptable Objects/BossData")]
public abstract class BossData : ScriptableObject
{
    public string bossName;
    public GameObject bossPrefab;

    // Abstract function to choose a hand
    public abstract PlayerInventory.HandType ChooseHand(PlayerInventory playerInventory);
    
    // Hooks for abilities
    public virtual int GetHandCost(PlayerInventory.HandType handType)
    {
        return 1; // Default is 1, override in bosses with special costs
    }

    public virtual bool IsHandRestricted(PlayerInventory.HandType handType)
    {
        return false; // No restricted hands by default
    }
}
