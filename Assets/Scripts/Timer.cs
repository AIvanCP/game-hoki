using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 60;
    public bool timerIsRunning = false;
    public TextMeshProUGUI timeText;

    public GameObject gameOverScreen;
    public TextMeshProUGUI winnerText;

    public BallController ballController;

    private void Start()
    {
        gameOverScreen.SetActive(false);
        // timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                gameOverScreen.SetActive(true);
                Time.timeScale = 0;
                Debug.Log("Time has run out!");
                if (ballController.player1Score > ballController.player2Score)
                {
                    winnerText.text = "Player 1 wins!";
                }
                else if (ballController.player1Score < ballController.player2Score)
                {
                    winnerText.text = "Player 2 wins!";
                }
                else
                {
                    winnerText.text = "It's a draw!";
                }
                timeRemaining = 0;
                timerIsRunning = false;
            }
        }
        else
        {
            if (Keyboard.current.enterKey.wasReleasedThisFrame || Keyboard.current.spaceKey.wasReleasedThisFrame && gameOverScreen.activeSelf)
            {
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeText.text = timeToDisplay.ToString("00");
    }
}
