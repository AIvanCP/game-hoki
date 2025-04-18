using UnityEngine;
using System.Collections;

public class AIMovementController : MonoBehaviour
{
    public float batasAtas;
    public float batasBawah;
    public float batasKiri = -8f;
    public float batasKanan = 8f;

    [SerializeField]
    private float kecepatan = 5f;
    [SerializeField]
    private float reactionTime = 0.1f;
    [SerializeField]
    private float predictionAccuracy = 0.8f;
    [SerializeField]
    private float randomMovementChance = 0.2f;
    [SerializeField]
    private float difficultyMultiplier = 1.0f;
    [SerializeField]
    private Transform ballTransform;

    [Header("Control Mode")]
    public bool isControlledByPlayer = false;
    public bool isFreeMove = false;

    private int arah = 1;
    private float targetY;
    private float currentSpeed;
    private float lastDecisionTime;
    private float randomOffset;
    private bool makingMistake = false;

    private void Start()
    {
        isControlledByPlayer = MainMenuManager.isMultiplayerMode;
        targetY = transform.position.y;
        currentSpeed = kecepatan;
        lastDecisionTime = Time.time;

        if (!isControlledByPlayer)
        {
            StartCoroutine(UpdateDecision());
        }
    }

    void Update()
    {
        if (isControlledByPlayer)
        {
            if (isFreeMove)
                HandlePlayerFreeMove();
            else
                HandlePlayerInput();
        }
        else
        {
            if (isFreeMove)
                HandleAIFreeMove();
            else
                HandleAIMovement();
        }
    }

    private void HandlePlayerInput()
    {
        float move = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            move = kecepatan * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            move = -kecepatan * Time.deltaTime;
        }

        float nextY = Mathf.Clamp(transform.position.y + move, batasBawah, batasAtas);
        transform.position = new Vector3(transform.position.x, nextY, transform.position.z);
    }

    private void HandlePlayerFreeMove()
    {
        float move = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move = -kecepatan * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            move = kecepatan * Time.deltaTime;
        }

        float nextX = Mathf.Clamp(transform.position.x + move, batasKiri, batasKanan);
        transform.position = new Vector3(nextX, transform.position.y, transform.position.z);
    }

    private void HandleAIMovement()
    {
        float gerak = 0;

        if (Mathf.Abs(transform.position.y - targetY) > 0.1f)
        {
            arah = (targetY > transform.position.y) ? 1 : -1;
            gerak = arah * currentSpeed * Time.deltaTime;
        }

        float nextPos = transform.position.y + gerak;
        nextPos = Mathf.Clamp(nextPos, batasBawah, batasAtas);
        transform.position = new Vector3(transform.position.x, nextPos, transform.position.z);
    }

    private void HandleAIFreeMove()
    {
        if (ballTransform == null) return;

        Vector3 pos = transform.position;
        float targetX = ballTransform.position.x;
        pos.x = Mathf.MoveTowards(pos.x, targetX, kecepatan * Time.deltaTime);
        pos.x = Mathf.Clamp(pos.x, batasKiri, batasKanan);
        transform.position = pos;
    }

    private IEnumerator UpdateDecision()
    {
        while (true)
        {
            yield return new WaitForSeconds(reactionTime / difficultyMultiplier);
            DecideMovement();
            currentSpeed = kecepatan * Random.Range(0.8f, 1.2f) * difficultyMultiplier;

            if (Random.value < 0.1f && !makingMistake)
            {
                StartCoroutine(MakeMistake());
            }
        }
    }

    private void DecideMovement()
    {
        if (ballTransform == null)
        {
            PatrolMovement();
            return;
        }

        if (Random.value < randomMovementChance && !makingMistake)
        {
            RandomMovement();
            return;
        }

        PredictBallMovement();
    }

    private void PatrolMovement()
    {
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
        targetY = Random.Range(batasBawah, batasAtas);
    }

    private void PredictBallMovement()
    {
        if (ballTransform != null)
        {
            Rigidbody2D ballRb = ballTransform.GetComponent<Rigidbody2D>();
            Vector2 ballVelocity = (ballRb != null) ? ballRb.linearVelocity : Vector2.zero;

            float distanceToBall = Mathf.Abs(transform.position.x - ballTransform.position.x);

            if ((transform.position.x < ballTransform.position.x && ballVelocity.x < 0) ||
                (transform.position.x > ballTransform.position.x && ballVelocity.x > 0))
            {
                float timeToReach = distanceToBall / Mathf.Abs(ballVelocity.x);
                float predictedY = ballTransform.position.y + (ballVelocity.y * timeToReach);

                float errorFactor = (1 - predictionAccuracy * difficultyMultiplier);
                float predictionError = Random.Range(-5f, 5f) * errorFactor;

                targetY = Mathf.Clamp(predictedY + predictionError, batasBawah, batasAtas);
            }
            else
            {
                targetY = Mathf.Lerp(batasBawah, batasAtas, 0.5f) + Random.Range(-1f, 1f);
            }
        }
    }

    private IEnumerator MakeMistake()
    {
        makingMistake = true;
        targetY = Random.Range(batasBawah, batasAtas);
        yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        makingMistake = false;
    }

    public void SetDifficulty(float difficulty)
    {
        difficultyMultiplier = difficulty;
    }
}
