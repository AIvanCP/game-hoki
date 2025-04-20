using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;



public class OptionsMenuManager : MonoBehaviour
{
    public GameObject[] optionItems; // Assign: FreeMove, Back
    private int selectedIndex = 0;
    public TextMeshProUGUI freeMoveStatusText; // drag tulisan "OFF"
    private bool isFreeMoveEnabled = false;    // default OFF
    
    public MainMenuManager mainMenu; // Drag MainMenuManager di inspector

    void OnEnable()
    {
        selectedIndex = 0;
        HighlightOption();
    }

    void Update()
    {
        if (Keyboard.current.downArrowKey.wasReleasedThisFrame || Keyboard.current.sKey.wasReleasedThisFrame)
        {
            selectedIndex = (selectedIndex + 1) % optionItems.Length;
            HighlightOption();
        }

        if (Keyboard.current.upArrowKey.wasReleasedThisFrame || Keyboard.current.wKey.wasReleasedThisFrame)
        {
            selectedIndex = (selectedIndex - 1 + optionItems.Length) % optionItems.Length;
            HighlightOption();
        }

        if (Keyboard.current.enterKey.wasReleasedThisFrame || Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            HandleSelection();
        }

        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            mainMenu.CloseOptionsMenu();
        }
    }

    void HighlightOption()
    {
        for (int i = 0; i < optionItems.Length; i++)
        {
            var text = optionItems[i].GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                bool isSelected = (i == selectedIndex);
                text.fontStyle = isSelected ? FontStyles.Bold : FontStyles.Normal;
                text.color = isSelected ? Color.white : Color.gray;

                // Kalau FreeMove (index 0), highlight juga status-nya
                if (i == 0 && freeMoveStatusText != null)
                {
                    freeMoveStatusText.color = isSelected ? Color.white : Color.gray;
                }
            }
        }
    }

    void HandleSelection()
    {
        switch (selectedIndex)
        {
            case 0: // Toggle FreeMove
                isFreeMoveEnabled = !isFreeMoveEnabled;
                freeMoveStatusText.text = isFreeMoveEnabled ? "ON" : "OFF";
                break;
            case 1: // Back
                mainMenu.CloseOptionsMenu();
                break;
        }
    }
}