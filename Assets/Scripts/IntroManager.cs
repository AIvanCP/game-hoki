using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private float waitTime = 10f;
    private float timer = 0f;
    private bool shouldTransition = true;

    [SerializeField]
    private AudioManager audioManager;

    [Header("Audio Fade Settings")]
    [SerializeField] private float fadeInDuration = 5f;
    [SerializeField] private float targetVolume = 1f;
    private Coroutine fadeCoroutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0f;

        // Find AudioManager if not assigned
        if (audioManager == null)
        {
            audioManager = FindFirstObjectByType<AudioManager>();
        }

        // Start with zero volume
        if (audioManager != null)
        {
            audioManager.SetBGMVolume(0f);
            // Start the fade-in coroutine
            fadeCoroutine = StartCoroutine(FadeInVolume());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldTransition)
        {
            timer += Time.deltaTime;

            if (timer >= waitTime)
            {
                LoadNextScene();
                shouldTransition = false;
            }
        }
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if the next scene exists in the build settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Next scene does not exist in build settings. Attempting to load first scene.");
            SceneManager.LoadScene(0);
        }
    }

    private IEnumerator FadeInVolume()
    {
        float currentTime = 0;

        while (currentTime < fadeInDuration)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(0f, targetVolume, currentTime / fadeInDuration);
            audioManager.SetBGMVolume(newVolume);
            yield return null;
        }

        // Ensure we end exactly at target volume
        audioManager.SetBGMVolume(targetVolume);
    }

    private void OnDestroy()
    {
        // Clean up the coroutine if destroyed while fading
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
    }
}
