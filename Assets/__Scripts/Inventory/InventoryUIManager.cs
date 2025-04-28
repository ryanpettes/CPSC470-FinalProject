using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public GameObject inventoryItemPrefab;
    public Transform inventoryItemContainer;
    
    public BattleManager battleManager;
    
    // Dictionary w/ key='HandType name' and value='UI element for given HandType'
    private Dictionary<PlayerInventory.HandType, InventoryItem> inventoryItems = new Dictionary<PlayerInventory.HandType, InventoryItem>();

    private void Start()
    {
        BuildInventoryUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            playerInventory.ChangeAmountBy(PlayerInventory.HandType.Rock, 1);
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            // Subscribe inventory rendering to inventory change event
            playerInventory.OnInventoryChanged += UpdateInventoryItemUI;
        }
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= UpdateInventoryItemUI;
        }
    }

    // Function to build full dynamic inventory UI in a scene
    private void BuildInventoryUI()
    {
        // Prevent duplication on scene loads by ensuring container is empty
        foreach (Transform child in inventoryItemContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Clear inventory dictionary
        inventoryItems.Clear();
        
        // For each hand, build an itemUI in Inventory container
        foreach (var handType in playerInventory.GetAvailableHands())
        {
            // Create empty itemUI prefab
            GameObject itemGO = Instantiate(inventoryItemPrefab, inventoryItemContainer);

            // Setup the UI element with corresponding sprite and quantity
            InventoryItem inventoryItem = itemGO.GetComponent<InventoryItem>();
            if (inventoryItem != null)
            {
                inventoryItem.Setup(playerInventory, handType, battleManager);
                inventoryItems.Add(handType, inventoryItem);
            }
            else
            {
                Debug.LogError("InventoryItem component missing on prefab.");
            }
        }
    }
    
    // Function to rebuild only one item in inventory UI
    private void UpdateInventoryItemUI(PlayerInventory.HandType handType, int newQuantity)
    {
        if (inventoryItems.TryGetValue(handType, out InventoryItem itemUI))
        {
            itemUI.SetQuantity(newQuantity);
        }
        else
        {
            Debug.LogWarning($"No UI element found for {handType}. Rebuilding UI.");
            BuildInventoryUI(); // Fallback
        }
    }
}
