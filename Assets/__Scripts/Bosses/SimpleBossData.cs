using UnityEngine;

[CreateAssetMenu(fileName = "SimpleBoss", menuName = "Boss/SimpleBoss")]
public class SimpleBossData : BossData
{
    public override PlayerInventory.HandType ChooseHand(PlayerInventory playerInventory)
    {
        int randomIndex = Random.Range(0, 3);
        return (PlayerInventory.HandType)randomIndex;
    }

    public override bool IsHandRestricted(PlayerInventory.HandType hand)
    {
        return false; // No restrictions
    }

    public override int GetHandCost(PlayerInventory.HandType hand)
    {
        return 1; // Flat cost for all hands
    }
}