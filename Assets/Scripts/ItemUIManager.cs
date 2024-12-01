using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CardGame;

public class ItemUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private RectTransform itemContainer;

    [Header("Preview Window")]
    [SerializeField] private GameObject previewWindow;
    [SerializeField] private Image cardPreview1;
    [SerializeField] private Image cardPreview2;
    [SerializeField] private TextMeshProUGUI previewText;
    [SerializeField] private Button closePreviewButton;

    [Header("Layout Settings")]
    [SerializeField] private float buttonSpacing = 10f;
    [SerializeField] private Vector2 buttonSize = new Vector2(80f, 80f);
    [SerializeField] private float containerPadding = 10f;

    private Dictionary<ItemType, GameObject> itemButtons = new Dictionary<ItemType, GameObject>();
    private ItemManager itemManager;

    private void Start()
    {
        itemManager = ItemManager.Instance;
        SetupItemButtons();
        SetupPreviewWindow();

        itemManager.OnItemUsed += HandleItemUsed;
        itemManager.OnItemPurchased += HandleItemPurchased;

        previewWindow.SetActive(false);
    }


    private void SetupItemButtons()
    {
        // Clear existing buttons
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
        itemButtons.Clear();

        // Calculate positions
        float currentY = -containerPadding;

        // Create buttons for each item type
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            // Create button instance
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemContainer);
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

            // Configure button layout
            rectTransform.sizeDelta = buttonSize;

            // Setup button data
            ItemData itemData = itemManager.GetItemData(type);
            Image iconImage = buttonObj.transform.Find("ItemIcon").GetComponent<Image>();
            TextMeshProUGUI countText = buttonObj.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
            Button button = buttonObj.GetComponent<Button>();

            // Set button properties
            iconImage.sprite = itemData.icon;
            button.onClick.AddListener(() => OnItemButtonClicked(type));

            // Add tooltip (optional)
            AddTooltip(buttonObj, itemData);

            // Store reference
            itemButtons[type] = buttonObj;

            // Update position
            currentY -= (buttonSize.y + buttonSpacing);

            // Initial state update
            UpdateButtonState(type);
        }
    }

    private void AddTooltip(GameObject buttonObj, ItemData itemData)
    {
        // Add tooltip component if you have one
        // var tooltip = buttonObj.AddComponent<TooltipComponent>(); // Replace with your tooltip component
        // if (tooltip != null)
        // {
        //     tooltip.SetTooltipText($"{itemData.name}\n{itemData.description}");
        // }
    }

    private void SetupPreviewWindow()
    {
        closePreviewButton.onClick.AddListener(() => previewWindow.SetActive(false));
    }

    private void UpdateButtonState(ItemType type)
    {
        if (!itemButtons.TryGetValue(type, out GameObject buttonObj))
            return;

        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI countText = buttonObj.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

        int count = GameManager.Instance.PlayerStats.GetItemCount((int)type);
        countText.text = count.ToString();

        // Update button interactability
        button.interactable = count > 0 && itemManager.CanUseItem(type);

        // Optional: Update visual feedback
        var iconImage = buttonObj.transform.Find("ItemIcon").GetComponent<Image>();
        iconImage.color = button.interactable ? Color.white : new Color(1, 1, 1, 0.5f);
    }

    private void OnItemButtonClicked(ItemType type)
    {
        if (itemManager.UseItem(type))
        {
            AudioManager.Instance.PlayButtonClick();
            UpdateButtonState(type);
        }
    }

    private void HandleItemUsed(ItemType type)
    {
        UpdateButtonState(type);

        if (type == ItemType.PeekNextCards)
        {
            // ShowCardPreview();
        }
    }

    private void HandleItemPurchased(ItemType type)
    {
        UpdateButtonState(type);
    }

    // private void ShowCardPreview()
    // {
    //     Card[] previewCards = BlackjackController.Instance.deck.PeekNextCards(2);
    //     if (previewCards == null) return;

    //     cardPreview1.sprite = CardSpriteReference.Instance.GetCardSprite(
    //         previewCards[0].rank, previewCards[0].suit);
    //     cardPreview2.sprite = CardSpriteReference.Instance.GetCardSprite(
    //         previewCards[1].rank, previewCards[1].suit);

    //     previewText.text = "Your next two cards will be:";
    //     previewWindow.SetActive(true);
    // }

    private void OnDestroy()
    {
        if (itemManager != null)
        {
            itemManager.OnItemUsed -= HandleItemUsed;
            itemManager.OnItemPurchased -= HandleItemPurchased;
        }
    }
}