using UnityEngine;
using System.Collections;

public class AIMovementController : MonoBehaviour
{
    public float batasAtas;
    public float batasBawah;

    [SerializeField]
    private float kecepatan = 5f;
    [SerializeField]
    private float reactionTime = 0.1f; // Time to react to ball changes
    [SerializeField]
    private float predictionAccuracy = 0.8f; // How accurate the AI predicts (0-1)
    [SerializeField]
    private float randomMovementChance = 0.2f; // Chance to move randomly
    [SerializeField]
    private float difficultyMultiplier = 1.0f; // Scales all difficulty parameters
    [SerializeField]
    private Transform ballTransform; // Reference to the ball

    private int arah = 1; // 1 = naik, -1 = turun
    private float targetY;
    private float currentSpeed;
    private float lastDecisionTime;
    private float randomOffset;
    private bool makingMistake = false;

    private void Start()
    {
        targetY = transform.position.y;
        currentSpeed = kecepatan;
        lastDecisionTime = Time.time;
        StartCoroutine(UpdateDecision());
    }

    void Update()
    {
        // Move towards the target position
        float gerak = 0;

        if (Mathf.Abs(transform.position.y - targetY) > 0.1f)
        {
            arah = (targetY > transform.position.y) ? 1 : -1;
            gerak = arah * currentSpeed * Time.deltaTime;
        }

        float nextPos = transform.position.y + gerak;

        // Ensure we stay within boundaries
        if (nextPos >= batasAtas)
        {
            nextPos = batasAtas;
        }
        else if (nextPos <= batasBawah)
        {
            nextPos = batasBawah;
        }

        transform.position = new Vector3(transform.position.x, nextPos, transform.position.z);
    }

    private IEnumerator UpdateDecision()
    {
        while (true)
        {
            // Wait for reaction time (slower = easier)
            yield return new WaitForSeconds(reactionTime / difficultyMultiplier);

            // Decide what to do
            DecideMovement();

            // Vary the AI speed randomly for more human-like movement
            currentSpeed = kecepatan * Random.Range(0.8f, 1.2f) * difficultyMultiplier;

            // Occasionally make a mistake
            if (Random.value < 0.1f && !makingMistake)
            {
                StartCoroutine(MakeMistake());
            }
        }
    }

    private void DecideMovement()
    {
        // If there's no ball, just patrol
        if (ballTransform == null)
        {
            PatrolMovement();
            return;
        }

        // Sometimes move randomly for unpredictability
        if (Random.value < randomMovementChance && !makingMistake)
        {
            RandomMovement();
            return;
        }

        // Otherwise, try to track the ball with some prediction
        PredictBallMovement();
    }

    private void PatrolMovement()
    {
        // Simple patrol between upper and lower bounds
        if (transform.position.y >= batasAtas - 0.5f)
        {
            arah = -1;
        }
        else if (transform.position.y <= batasBawah + 0.5f)
        {
            arah = 1;
        }

        targetY = transform.position.y + arah * Random.Range(1f, 3f);
    }

    private void RandomMovement()
    {
        // Move to a random position within bounds
        targetY = Random.Range(batasBawah, batasAtas);
    }

    private void PredictBallMovement()
    {
        // Calculate where the ball might be when it reaches the paddle
        if (ballTransform != null)
        {
            // Get ball's rigidbody if it has one
            Rigidbody2D ballRb = ballTransform.GetComponent<Rigidbody2D>();
            Vector2 ballVelocity = (ballRb != null) ? ballRb.linearVelocity : Vector2.zero;

            // Basic distance calculation
            float distanceToBall = Mathf.Abs(transform.position.x - ballTransform.position.x);

            // If ball is coming toward this paddle
            if ((transform.position.x < ballTransform.position.x && ballVelocity.x < 0) ||
                (transform.position.x > ballTransform.position.x && ballVelocity.x > 0))
            {
                // Time for ball to reach paddle
                float timeToReach = distanceToBall / Mathf.Abs(ballVelocity.x);

                // Predicted Y position
                float predictedY = ballTransform.position.y + (ballVelocity.y * timeToReach);

                // Apply accuracy based on difficulty
                float errorFactor = (1 - predictionAccuracy * difficultyMultiplier);
                float predictionError = Random.Range(-5f, 5f) * errorFactor;

                // Target with error
                targetY = Mathf.Clamp(predictedY + predictionError, batasBawah, batasAtas);
            }
            else
            {
                // Ball moving away, return to center with some randomness
                targetY = Mathf.Lerp(batasBawah, batasAtas, 0.5f) + Random.Range(-1f, 1f);
            }
        }
    }

    private IEnumerator MakeMistake()
    {
        makingMistake = true;

        // Move in the wrong direction briefly
        float wrongDirection = Random.Range(batasBawah, batasAtas);
        targetY = wrongDirection;

        // Hold the mistake for a short time
        yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));

        makingMistake = false;
    }

    // Called by external scripts to adjust difficulty
    public void SetDifficulty(float difficulty)
    {
        difficultyMultiplier = difficulty;
    }
}
