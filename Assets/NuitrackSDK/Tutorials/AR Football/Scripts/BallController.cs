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
    NetworkController networkController; //После написания сетевой части

    void Start()
    {
        //получаем ссылку на компонент Rigidbody
        rb = GetComponentInChildren<Rigidbody>();
        //Задаём уничтожение через 7 секунд после старта
        Destroy(gameObject, 7.0f);
        transform.parent = FindObjectOfType<Environment>().transform;
        //FindObjectOfType<AvatarSync>().transform; //TODO: коряво

        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        ball.transform.localPosition = startPosition;

        networkController = FindObjectOfType<NetworkController>(); //После написания сетевой части
    }

    void Update()
    {
        if (inGame && networkController.isClient == false) //движение мяча пока он "в игре". Он просто летит к заданой точке // " && networkController.isClient == false" После написания сетевой части. Всё движение мячика по скрипту и физике происходит на сервере, на клиент только передаётся его позиция.
        {
            //https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
            ball.transform.localPosition = Vector3.MoveTowards(ball.transform.localPosition, endPosition, ballSpeed * Time.deltaTime); //аргументы: текущая позиция, конечная позиция, скорость
            ball.transform.Rotate(Vector3.one * ballSpeed); //В полёте поворачиваем (просто для красоты)
        }
    }

    public void Setup(Vector3 startPos, Vector3 endPos)
    {
        endPosition = endPos;
        startPosition = startPos;
    }

    public void OnCollide(Collision collision)
    {
        //Если мяч что - нибудь задел, переменная inGame становится false, чтобы столкновения больше не обрабатывались
        if (inGame && networkController.isClient == false) // " && networkController.isClient == false" После написания сетевой части
        {
            Debug.Log("Ball collide");
            //Если мяч коснулся руки значит его отбили, и прибавляем одно очко
            if (collision.transform.tag == "Hand")
                FindObjectOfType<NetworkController>().score++;

            //включаем ему гравитацию, чтобы мяч упал вниз
            rb.useGravity = true;
            inGame = false;
        }
    }
}
