#if ENABLE_AR_TUTORIAL
using UnityEngine;

public class BallController : MonoBehaviour
{

    [SerializeField]
    GameObject ball;
    Vector3 startPosition;
    Vector3 endPosition;
    float ballSpeed = 3;
    Rigidbody rb;
    bool inGame = true;
    NetworkController networkController;

    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        Destroy(gameObject, 7.0f);
        transform.parent = FindObjectOfType<Environment>().transform;

        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        ball.transform.localPosition = startPosition;

        networkController = FindObjectOfType<NetworkController>();
    }

    void Update()
    {
        if (inGame && networkController.isClient == false)
        {
            //https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
            ball.transform.localPosition = Vector3.MoveTowards(ball.transform.localPosition, endPosition, ballSpeed * Time.deltaTime);
            ball.transform.Rotate(Vector3.one * ballSpeed);
        }
    }

    public void Setup(Vector3 startPos, Vector3 endPos)
    {
        endPosition = endPos;
        startPosition = startPos;
    }

    public void OnCollide(Collision collision)
    {
        if (inGame && networkController.isClient == false)
        {
            Debug.Log("Ball collide");
            if (collision.transform.tag == "Hand")
                FindObjectOfType<NetworkController>().score++;

            rb.useGravity = true;
            inGame = false;
        }
    }
}
#endif