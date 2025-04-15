using UnityEngine;
using TMPro;

public class GoalCountdownC : MonoBehaviour
{
    public TextMeshProUGUI countdownText;
    public float countdownTime = 3f;

    void OnEnable()
    {
        StartCoroutine(CountdownCoroutine());
    }


    private System.Collections.IEnumerator CountdownCoroutine()
    {
        Debug.Log("Countdown started!");
        float timeRemaining = countdownTime;

        while (timeRemaining > 0)
        {
            // Update the countdown text
            countdownText.text = Mathf.Ceil(timeRemaining).ToString();
            timeRemaining -= Time.unscaledDeltaTime; // Use unscaled delta time to ensure consistent countdown
            yield return null; // Wait for the next frame
        }
    }
}
