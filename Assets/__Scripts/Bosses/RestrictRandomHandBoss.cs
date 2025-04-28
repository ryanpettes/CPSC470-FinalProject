using UnityEngine;

[CreateAssetMenu(fileName = "RandomRestrictBoss", menuName = "Boss/RandomRestrictBoss")]

public class RestrictRandomHandBoss : BossData
{
    // Hand type to restrict
    private PlayerInventory.HandType? restrictedHand = null;
    
    public override PlayerInventory.HandType ChooseHand(PlayerInventory playerInventory)
    {
        int randomIndex = Random.Range(0, 3);
        return (PlayerInventory.HandType)randomIndex;
    }

    public override bool IsHandRestricted(PlayerInventory.HandType handType)
    {
        if (restrictedHand == null)
            return false;

        return handType == restrictedHand.Value;
    }

    // Called at start of every round
    public void SetNewRestrictedHand()
    {
        restrictedHand = (PlayerInventory.HandType)Random.Range(0, 3);
    }
    
    public PlayerInventory.HandType? GetRestrictedHand()
    {
        return restrictedHand;
    }
}
