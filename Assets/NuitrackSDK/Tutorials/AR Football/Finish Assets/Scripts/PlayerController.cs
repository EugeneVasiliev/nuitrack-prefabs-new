#if ENABLE_AR_TUTORIAL
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{

    [SerializeField]
    GameObject ballPrefab;

    [Command] //Called on the server, requires the Cmd prefix
    void CmdKick(Vector3 startPos, Vector3 endPos)
    {
        print("bonk");
        GameObject ball = (GameObject)Instantiate(ballPrefab);
        ball.GetComponent<BallController>().Setup(startPos, endPos);
        NetworkServer.Spawn(ball);
    }

    public void Kick(Vector3 startPos, Vector3 endPos)
    {
        CmdKick(startPos, endPos);
    }
}
#endif