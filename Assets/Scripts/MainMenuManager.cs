using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    public TextMeshProUGUI insertCoinText;
    public TextMeshProUGUI titleText;

    public GameObject[] menuOptions; // Single, Multi, Options, Exit
    public GameObject optionsManager; // ⬅️ drag object "OptionsManager" ke sini

    private int selectedIndex = 0;
    private bool isMenuActive = false;
    private bool isInOptionsMenu = false;

    public static bool isMultiplayerMode = false;
    public static bool isFreeMoveEnabled = false;

    void Start()
    {
        SetMenuHighlight();
        ToggleMenuOptions(false);
        isMultiplayerMode = false;

        if (optionsManager != null)
            optionsManager.SetActive(false); // ⬅️ hide OptionsManager saat awal
    }

    void Update()
    {
        insertCoinText.color = new Color(1, 1, 1, Mathf.PingPong(Time.time, 1));

        float noise = Mathf.PerlinNoise(Time.time * 0.7f, 0);
        float alpha = Mathf.Lerp(0.7f, 1f, noise);
        titleText.color = new Color(1, 1, 1, alpha);

        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            Application.Quit();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        if (!isMenuActive)
        {
            if (Keyboard.current.spaceKey.wasReleasedThisFrame || Keyboard.current.enterKey.wasReleasedThisFrame)
            {
                isMenuActive = true;
                ToggleMenuOptions(true);
                SetMenuHighlight();
            }
            return;
        }

        // Kalau lagi di OptionsManager, abaikan input navigasi menu utama
        if (isInOptionsMenu) return;

        if (Keyboard.current.downArrowKey.wasReleasedThisFrame || Keyboard.current.sKey.wasReleasedThisFrame)
        {
            selectedIndex = (selectedIndex + 1) % menuOptions.Length;
            SetMenuHighlight();
        }

        if (Keyboard.current.upArrowKey.wasReleasedThisFrame || Keyboard.current.wKey.wasReleasedThisFrame)
        {
            selectedIndex = (selectedIndex - 1 + menuOptions.Length) % menuOptions.Length;
            SetMenuHighlight();
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame || Keyboard.current.enterKey.wasReleasedThisFrame)
        {
            HandleMenuSelection();
        }
    }

    void SetMenuHighlight()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
            var text = menuOptions[i].GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.fontStyle = (i == selectedIndex) ? FontStyles.Bold : FontStyles.Normal;
                text.color = (i == selectedIndex) ? Color.white : Color.gray;
            }
        }
    }

    void ToggleMenuOptions(bool isActive)
    {
        foreach (GameObject option in menuOptions)
        {
            option.SetActive(isActive);
        }
    }

    void HandleMenuSelection()
    {
        switch (selectedIndex)
        {
            case 0: // Single
                isMultiplayerMode = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                break;
            case 1: // Multi
                isMultiplayerMode = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                break;
            case 2: // Options
                OpenOptionsMenu();
                break;
            case 3: // Exit
                Application.Quit();
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
                break;
        }
    }

    void OpenOptionsMenu()
    {
        isInOptionsMenu = true;
        ToggleMenuOptions(false);
        if (optionsManager != null)
            optionsManager.SetActive(true);
    }

    public void CloseOptionsMenu()
    {
        isInOptionsMenu = false;
        ToggleMenuOptions(true);
        if (optionsManager != null)
            optionsManager.SetActive(false);
    }

    public void ToggleFreeMove()
    {
        isFreeMoveEnabled = !isFreeMoveEnabled;
        Debug.Log("Free Move Toggled: " + isFreeMoveEnabled);
    }
}
