using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{

    [SerializeField]
    GameObject ballPrefab;

    [Command] //Вызывается на сервере, нужен префикс Cmd
    void CmdKick(Vector3 startPos, Vector3 endPos)
    {
        GameObject ball = (GameObject)Instantiate(ballPrefab);
        ball.GetComponent<BallController>().Setup(startPos, endPos);
        NetworkServer.Spawn(ball);
    }

    public void Kick(Vector3 startPos, Vector3 endPos)
    {
        CmdKick(startPos, endPos);
    }
}
