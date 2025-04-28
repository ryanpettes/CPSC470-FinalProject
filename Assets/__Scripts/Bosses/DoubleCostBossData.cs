using UnityEngine;

[CreateAssetMenu(fileName = "DoubleCostBoss", menuName = "Boss/DoubleCostBoss")]
public class DoubleCostBossData : BossData
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
        return 2; // Double cost for all hands
    }
}
