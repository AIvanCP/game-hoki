using UnityEngine;

public class Player2MovementController : MonoBehaviour
{
    public float batasAtas;
    public float batasBawah;

    [SerializeField]
    private float kecepatan;
    [SerializeField]
    private string axis;

    void Update()
    {
        float gerak = Input.GetAxisRaw(axis) * kecepatan * Time.deltaTime;
        float nextPos = transform.position.y + gerak;

        if (nextPos > batasAtas)
        {
            nextPos = batasAtas;
        }
        else if (nextPos < batasBawah)
        {
            nextPos = batasBawah;
        }

        transform.position = new Vector3(transform.position.x, nextPos, transform.position.z);
    }

}
