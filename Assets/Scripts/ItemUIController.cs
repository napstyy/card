using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace CardGame
{
    public class ItemUIController : MonoBehaviour
    {
        #region UI References
        [Header("Item UI Elements")]
        [SerializeField] private GameObject itemButtonPrefab;
        [SerializeField] private Transform itemButtonContainer;
        [SerializeField] private GameObject itemPreviewWindow;

        [Header("Preview Window Elements")]
        [SerializeField] private Image previewCardImage1;
        [SerializeField] private Image previewCardImage2;
        [SerializeField] private Button closePreviewButton;
        [SerializeField] private TextMeshProUGUI previewDescription;

        [Header("Audio")]
        [SerializeField] private AudioClip itemUseSound;
        #endregion

        private Dictionary<ItemType, GameObject> itemButtons = new Dictionary<ItemType, GameObject>();
        private ItemManager itemManager;
        private GameManager gameManager;

        private void Start()
        {
            itemManager = ItemManager.Instance;
            gameManager = GameManager.Instance;

            // Subscribe to events
            itemManager.OnItemUsed += HandleItemUsed;
            itemManager.OnItemPurchased += HandleItemPurchased;
            gameManager.OnGameStateChanged += HandleGameStateChanged;

            // Initialize UI
            InitializeItemButtons();
            closePreviewButton.onClick.AddListener(ClosePreviewWindow);
            itemPreviewWindow.SetActive(false);

            // Update button states
            UpdateItemButtonStates();
        }

        private void OnDestroy()
        {
            if (itemManager != null)
            {
                itemManager.OnItemUsed -= HandleItemUsed;
                itemManager.OnItemPurchased -= HandleItemPurchased;
            }

            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void InitializeItemButtons()
        {
            // Clear existing buttons
            foreach (Transform child in itemButtonContainer)
            {
                Destroy(child.gameObject);
            }
            itemButtons.Clear();

            // Create buttons for each item type
            foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
            {
                ItemData itemData = itemManager.GetItemData(type);
                if (itemData == null) continue;

                GameObject buttonObj = Instantiate(itemButtonPrefab, itemButtonContainer);
                SetupItemButton(buttonObj, itemData);
                itemButtons[type] = buttonObj;
            }
        }

        private void SetupItemButton(GameObject buttonObj, ItemData itemData)
        {
            // Setup button visuals
            Image iconImage = buttonObj.GetComponent<Image>();
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI countText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            // Set icon
            if (iconImage != null && itemData.icon != null)
            {
                iconImage.sprite = itemData.icon;
            }

            // Setup tooltip
            TooltipTrigger tooltip = buttonObj.GetComponent<TooltipTrigger>();
            if (tooltip != null)
            {
                tooltip.header = itemData.name;
                tooltip.content = itemData.description;
            }

            // Setup click handler
            button.onClick.AddListener(() => UseItem(itemData.type));

            // Initial state
            UpdateItemButton(itemData.type);
        }

        private void UpdateItemButton(ItemType type)
        {
            if (!itemButtons.TryGetValue(type, out GameObject buttonObj))
                return;

            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI countText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            int count = gameManager.PlayerStats.GetItemCount((int)type);

            // Update count text
            countText.text = count.ToString();

            // Update button interactability
            button.interactable = itemManager.CanUseItem(type);
        }

        private void UpdateItemButtonStates()
        {
            foreach (ItemType type in itemButtons.Keys)
            {
                UpdateItemButton(type);
            }
        }

        private void UseItem(ItemType type)
        {
            if (itemManager.UseItem(type))
            {
                AudioManager.Instance.PlaySFX(itemUseSound);
            }
        }

        #region Event Handlers
        private void HandleItemUsed(ItemType type)
        {
            UpdateItemButton(type);

            if (type == ItemType.PeekNextCards)
            {
                ShowCardPreview();
            }
        }

        private void HandleItemPurchased(ItemType type)
        {
            UpdateItemButton(type);
        }

        private void HandleGameStateChanged(GameManager.GameState newState)
        {
            UpdateItemButtonStates();
        }
        #endregion

        #region Preview Window
        private void ShowCardPreview()
        {
            Card[] previewCards = BlackjackController.Instance.deck.PeekNextCards(2);
            if (previewCards == null) return;

            // Update preview window elements
            previewCardImage1.sprite = CardSpriteReference.Instance.GetCardSprite(
                previewCards[0].rank, previewCards[0].suit);
            previewCardImage2.sprite = CardSpriteReference.Instance.GetCardSprite(
                previewCards[1].rank, previewCards[1].suit);

            previewDescription.text = "These are your next two cards:";
            itemPreviewWindow.SetActive(true);
        }

        private void ClosePreviewWindow()
        {
            itemPreviewWindow.SetActive(false);
        }
        #endregion
    }
}