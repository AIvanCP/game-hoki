using UnityEngine;
using TMPro;
using System.Collections;

public class BallController : MonoBehaviour
{
    public int force;
    public float maxSpeed = 15f;
    public float minSpeed = 5f; // Tambahin batas bawah kecepatan bola
    public float bounceFactor = 1.1f;
    public bool useTrail = true;

    private TrailRenderer trail;
    private Rigidbody2D rigid;
    private PhysicsMaterial2D ballMaterial;

    // Anti-glitch parameters
    private float lastBounceTime = 0f;
    private const float BOUNCE_COOLDOWN = 0.02f; // Prevent multiple collisions in a single frame
    private Vector2 lastCollisionNormal;
    private Vector2 lastVelocity;

    public int player1Score;
    public int player2Score;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    public AudioSource sfxAudioSource;
    public AudioClip hitSound;

    [SerializeField]
    private GameObject goalScreen;



    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();

        // Pastikan Rigidbody2D sudah dikonfigurasi dengan baik
        rigid.gravityScale = 0;
        rigid.linearDamping = 0;
        rigid.angularDamping = 0;

        // Setup PhysicsMaterial2D
        ballMaterial = new PhysicsMaterial2D();
        ballMaterial.bounciness = 1f;
        ballMaterial.friction = 0f;
        GetComponent<Collider2D>().sharedMaterial = ballMaterial;

        sfxAudioSource = GetComponent<AudioSource>();
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        if (trail == null && useTrail)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.startWidth = 0.2f;
            trail.endWidth = 0.05f;
            trail.time = 0.2f;
        }

        LaunchBall();
    }

    void Update()
    {
        // Biar bola nggak keluar batas max speed atau min speed
        if (rigid.linearVelocity.magnitude > maxSpeed)
        {
            rigid.linearVelocity = rigid.linearVelocity.normalized * maxSpeed;
        }
        else if (rigid.linearVelocity.magnitude < minSpeed)
        {
            rigid.linearVelocity = rigid.linearVelocity.normalized * minSpeed;
        }

        // Jika bola hampir horizontal banget, tambahkan sedikit vertikal
        if (Mathf.Abs(rigid.linearVelocity.y) < 0.5f && rigid.linearVelocity.magnitude > 1f)
        {
            Vector2 newVelocity = rigid.linearVelocity;
            newVelocity.y += Random.Range(0.5f, 1.5f) * Mathf.Sign(Random.Range(-1f, 1f));
            rigid.linearVelocity = newVelocity.normalized * rigid.linearVelocity.magnitude;
        }

        // Store the last velocity for better collision handling
        lastVelocity = rigid.linearVelocity;
    }

    void FixedUpdate()
    {
        // Apply a small amount of smoothing to prevent erratic movement
        if (rigid.linearVelocity != Vector2.zero)
        {
            // Check for very small velocities and fix them
            if (Mathf.Abs(rigid.linearVelocity.x) < 0.1f)
            {
                Vector2 fixedVelocity = rigid.linearVelocity;
                fixedVelocity.x = 0.5f * Mathf.Sign(fixedVelocity.x);
                rigid.linearVelocity = fixedVelocity.normalized * rigid.linearVelocity.magnitude;
            }
        }
    }

    void LaunchBall()
    {
        // Improved launch with more predictable angles
        float randomSide = Random.value > 0.5f ? 1f : -1f;
        float randomAngle = Random.Range(-30f, 30f); // Wider angle range for more variety
        Vector2 arah = Quaternion.Euler(0, 0, randomAngle) * new Vector2(randomSide, 0);
        rigid.linearVelocity = arah * force;

        // Reset bounce tracking
        lastBounceTime = Time.time;
    }

    void ResetBall()
    {
        transform.localPosition = Vector2.zero;
        rigid.linearVelocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        // sfxAudioSource.time = 0.1f;
        sfxAudioSource.PlayOneShot(hitSound);

        // Prevent multiple physics calculations in quick succession
        if (Time.time - lastBounceTime < BOUNCE_COOLDOWN)
        {
            return;
        }
        lastBounceTime = Time.time;

        // Store the collision normal for reflection calculations
        if (coll.contactCount > 0)
        {
            lastCollisionNormal = coll.contacts[0].normal;
        }

        if (coll.gameObject.CompareTag("Player"))
        {
            // Get the paddle's movement direction (if it's moving)
            Vector2 paddleVelocity = Vector2.zero;
            Rigidbody2D paddleRigidbody = coll.gameObject.GetComponent<Rigidbody2D>();
            if (paddleRigidbody != null)
            {
                paddleVelocity = paddleRigidbody.linearVelocity;
            }

            // Calculate hit position more precisely using contact point
            Vector2 hitPoint = coll.contacts[0].point;
            Vector2 paddleCenter = coll.transform.position;

            // Determine if this is a vertical or horizontal paddle
            bool isVerticalPaddle = coll.collider.bounds.size.y > coll.collider.bounds.size.x;

            float hitFactor;
            Vector2 newDir;

            if (isVerticalPaddle)
            {
                // For vertical paddles (side paddles)
                hitFactor = (hitPoint.y - paddleCenter.y) / (coll.collider.bounds.size.y / 2);

                // Determine the proper x direction based on which side the paddle is on
                float xDir = (hitPoint.x > paddleCenter.x) ? 1f : -1f;

                // Create new direction with angle influence from hit position
                newDir = new Vector2(xDir, hitFactor * 1.5f).normalized;
            }
            else
            {
                // For horizontal paddles (top/bottom paddles)
                hitFactor = (hitPoint.x - paddleCenter.x) / (coll.collider.bounds.size.x / 2);

                // Determine the proper y direction based on which side the paddle is on
                float yDir = (hitPoint.y > paddleCenter.y) ? 1f : -1f;

                // Create new direction with angle influence from hit position
                newDir = new Vector2(hitFactor * 1.5f, yDir).normalized;
            }

            // Add a tiny bit of randomness to prevent repetitive patterns
            newDir = Quaternion.Euler(0, 0, Random.Range(-3f, 3f)) * newDir;

            // Calculate new speed with influence from paddle movement
            float paddleSpeedInfluence = 0f;
            if (paddleVelocity.magnitude > 0.1f)
            {
                // Add a portion of the paddle's velocity for more realistic physics
                paddleSpeedInfluence = Vector2.Dot(paddleVelocity.normalized, newDir) *
                                      Mathf.Min(paddleVelocity.magnitude, 3f) * 0.3f;
            }

            float baseSpeed = lastVelocity.magnitude;
            float newSpeed = Mathf.Clamp(baseSpeed * bounceFactor + paddleSpeedInfluence, minSpeed, maxSpeed);

            // Apply the new velocity with a small delay for better physics stability
            StartCoroutine(ApplyVelocityNextFrame(newDir * newSpeed));
        }
        else if (coll.gameObject.CompareTag("Wall") ||
                coll.gameObject.name == "TopLine" ||
                coll.gameObject.name == "BottomLine")
        {
            // Better wall reflection using contact normal
            Vector2 reflectedVelocity = Vector2.Reflect(lastVelocity, lastCollisionNormal);

            // Add tiny randomness to wall bounces to avoid perfectly looping trajectories
            reflectedVelocity = Quaternion.Euler(0, 0, Random.Range(-2f, 2f)) * reflectedVelocity;

            // Apply a very small speed loss for realism
            float speedRetention = 0.995f;
            float newSpeed = Mathf.Clamp(reflectedVelocity.magnitude * speedRetention, minSpeed, maxSpeed);

            // Apply the new velocity
            rigid.linearVelocity = reflectedVelocity.normalized * newSpeed;
        }

        if (coll.gameObject.name == "RightLine")
        {
            player1Score++;
            player1ScoreText.text = player1Score.ToString();
            if (goalScreen != null)
            {
                StartCoroutine(CountdownAfterGoal());
            }
            else
            {
                ResetBall();
                LaunchBall();
            }
        }

        if (coll.gameObject.name == "LeftLine")
        {
            player2Score++;
            player2ScoreText.text = player2Score.ToString();
            if (goalScreen != null)
            {
                StartCoroutine(CountdownAfterGoal());
            }
            else
            {
                ResetBall();
                LaunchBall();
            }
        }
    }

    private IEnumerator CountdownAfterGoal()
    {
        Time.timeScale = 0f;
        ResetBall();
        goalScreen.SetActive(true);
        Debug.Log("Wait 3s for next round...");
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1f;
        goalScreen.SetActive(false);
        LaunchBall();
        Debug.Log("Goal!");
    }

    // Apply velocity on the next frame to avoid physics glitches
    IEnumerator ApplyVelocityNextFrame(Vector2 newVelocity)
    {
        yield return null; // Wait for the next frame
        if (rigid != null)
        {
            rigid.linearVelocity = newVelocity;
        }
    }

    // Prevent the ball from getting stuck inside colliders
    void OnCollisionStay2D(Collision2D coll)
    {
        // If we're somehow inside a collider, push the ball outward along the normal
        if (coll.contactCount > 0 && rigid.linearVelocity.magnitude < minSpeed * 0.5f)
        {
            Vector2 escapeDirection = coll.contacts[0].normal;
            rigid.linearVelocity = escapeDirection * minSpeed;
        }
    }
}
