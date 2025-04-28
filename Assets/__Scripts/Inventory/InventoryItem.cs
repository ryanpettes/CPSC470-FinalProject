using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    // Variables for displaying item's UI
    public TMP_Text quantityText;
    public Button button;
    public Image itemImage;
     
    private PlayerInventory playerInventory;
    private PlayerInventory.HandType handType;
    private int quantity;
    
    public Sprite rockSprite;
    public Sprite paperSprite;
    public Sprite scissorSprite;

    // Function to dynamically setup UI for this InventoryItem
    public void Setup(PlayerInventory inventory, PlayerInventory.HandType type, BattleManager battleManager)
    {
        playerInventory = inventory;
        handType = type;
        
        SetQuantity(playerInventory.GetAmount(handType));

        // Assign correct sprite to this item
        switch (handType)
        {
            case PlayerInventory.HandType.Rock:
                itemImage.sprite = rockSprite;
                break;
            case PlayerInventory.HandType.Paper:
                itemImage.sprite = paperSprite;
                break;
            case PlayerInventory.HandType.Scissors:
                itemImage.sprite = scissorSprite;
                break;
        }

        // Disable buttons in world map
        bool isBattleScene = SceneManager.GetActiveScene().buildIndex != 1;
        button.interactable = isBattleScene;

        if (isBattleScene && battleManager != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => battleManager.OnHandSelected(type));
        }
    }
    
    private void OnEnable()
    {
        BattleManager.OnBattleStateChanged += SetInteractable;
        BattleManager.OnRestrictedHandChanged += UpdateInteractable;
    }

    private void OnDisable()
    {
        BattleManager.OnBattleStateChanged -= SetInteractable;
        BattleManager.OnRestrictedHandChanged -= UpdateInteractable;
    }

    // Disable interactable when round is being handled
    private void SetInteractable(bool isInteractable)
    {
        button.interactable = isInteractable;
    }
    
    // Disable interactable when facing restricted-hand boss
    private void UpdateInteractable(PlayerInventory.HandType? restrictedHand)
    {
        if (restrictedHand == null)
        {
            GetComponent<Button>().interactable = true;
            return;
        }

        // Suppose this button represents 'rock'
        if (handType == restrictedHand.Value)
        {
            GetComponent<Button>().interactable = false;
        }
        else
        {
            GetComponent<Button>().interactable = true;
        }
    }
    
    public void SetQuantity(int quantity)
    {
        if (quantity > 99) quantityText.text = "99+";
        else quantityText.text = quantity.ToString();
    }
}


