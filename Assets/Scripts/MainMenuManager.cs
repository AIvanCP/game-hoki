using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    public TextMeshProUGUI insertCoinText;
    public TextMeshProUGUI titleText;
    void Update()
    {
        insertCoinText.color = new Color(1, 1, 1, Mathf.PingPong(Time.time, 1));
        if (Keyboard.current.spaceKey.wasReleasedThisFrame || Keyboard.current.enterKey.wasReleasedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            Application.Quit();
        }

        float noise = Mathf.PerlinNoise(Time.time * 0.7f, 0);
        float alpha = Mathf.Lerp(0.7f, 1f, noise);
        titleText.color = new Color(1, 1, 1, alpha);
    }
}
