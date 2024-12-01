using System.Collections;
using CardGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUIController : MonoBehaviour
{
    [SerializeField] Sprite[] itemIcons; // Array containing all possible item icons
    [SerializeField] Button drawButton; // Button to trigger lucky draw
    [SerializeField] Button leaveButton; // Button to trigger lucky draw
    [SerializeField] GameObject itemDetailWindow; // To display item details after draw
    [SerializeField] GameObject machine; // To display item details after draw
    [SerializeField] GameObject descriptionWindow; // To display item details after draw
    [SerializeField] Image itemIcon; // Image showing the item drawn
    [SerializeField] TextMeshProUGUI descriptionText; // Text to show item description
    [SerializeField] Button confirmButton; // Button to stop rolling or close the detail window
    [SerializeField] string[] itemDescriptions; // Array containing descriptions for each item

    [SerializeField] float rollingDuration = 2.0f; // How long the rolling effect lasts
    [SerializeField] float rollingSpeed = 0.05f; // How fast the icons roll
    [SerializeField] float slowDownDuration = 1.0f; // Time for the rolling to slow down

    private bool isRolling = false; // To prevent multiple simultaneous draws
    private bool stopPressed = false; // Track if the stop button was pressed
    private Coroutine rollingCoroutine; // Reference to the rolling coroutine

    // Start is called before the first frame update
    void Start()
    {
        drawButton.onClick.AddListener(LuckyDraw);
        confirmButton.onClick.AddListener(HandleConfirmButton);
        leaveButton.onClick.AddListener(EndShopState);
        ResetUI();
    }

    private void EndShopState()
    {
        GameManager.Instance.CompleteShoppingPhase();
    }

    private void OnDisable()
    {
        ResetUI();
    }

    void LuckyDraw()
    {
        if (isRolling || GameManager.Instance.PlayerStats.ownedChips < 600)
        {
            // Prevent multiple rolls at the same time
            return;
        }
        GameManager.Instance.PlayerStats.RemoveChips(500);

        isRolling = true;
        stopPressed = false; // Reset stopPressed when starting a new roll
        drawButton.interactable = false; // Disable the button to prevent repeated clicks
        drawButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        itemDetailWindow.SetActive(true);
        descriptionWindow.gameObject.SetActive(false); // Hide text during rolling

        confirmButton.gameObject.SetActive(true);
        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop"; // Set button to "Stop"

        rollingCoroutine = StartCoroutine(RollItems());
    }

    void HandleConfirmButton()
    {
        if (isRolling && !stopPressed)
        {
            stopPressed = true; // Ensure the stop effect only triggers once
            confirmButton.interactable = false; // Prevent further presses until rolling stops
        }
        else if (!isRolling)
        {
            // Close the detail window if rolling is not active
            CloseItemDetailWindow();
            EndShopState();
        }
    }

    IEnumerator RollItems()
    {
        float elapsedTime = 0f;

        // Temporary index to simulate the rolling effect
        int index = 0;

        while (elapsedTime < rollingDuration || stopPressed)
        {
            // Change the displayed icon to create the rolling effect
            itemIcon.sprite = itemIcons[index];

            // Increment the index and loop it within the bounds
            index = (index + 1) % itemIcons.Length;

            // Speed adjustment: If stop button is pressed, slow down animation
            float currentSpeed = stopPressed ? Mathf.Lerp(rollingSpeed, 0.2f, elapsedTime / slowDownDuration) : rollingSpeed;

            // Wait for a short delay before swapping to the next icon
            yield return new WaitForSeconds(currentSpeed);

            elapsedTime += rollingSpeed;

            // If stop is pressed and we have slowed down sufficiently, exit
            if (stopPressed && elapsedTime >= slowDownDuration)
            {
                break;
            }
        }

        // Randomly determine the final item
        int finalIndex = Random.Range(0, itemIcons.Length);

        // Set the final item sprite and description
        itemIcon.sprite = itemIcons[finalIndex];
        descriptionText.text = itemDescriptions[finalIndex];

        // End of rolling: Show description and update UI
        descriptionWindow.gameObject.SetActive(true);
        confirmButton.interactable = true; // Allow button interaction again
        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = "Confirm"; // Set button to "Confirm"
        isRolling = false;
        drawButton.interactable = true; // Enable the draw button
        GameManager.Instance?.PlayerStats.AddItem(finalIndex);
    }

    void CloseItemDetailWindow()
    {
        // Hide the detail window
        itemDetailWindow.SetActive(false);
    }

    void ResetUI()
    {
        // Hide detail window and show draw button
        itemDetailWindow.SetActive(true);
        machine.SetActive(true);
        descriptionWindow.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        drawButton.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);

        isRolling = false;
        stopPressed = false;
        if (rollingCoroutine != null)
        {
            StopCoroutine(rollingCoroutine);
        }
    }
}