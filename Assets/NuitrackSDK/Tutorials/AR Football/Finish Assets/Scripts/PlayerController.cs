using UnityEngine;
#if ENABLE_AR_TUTORIAL
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour {
#else
public class PlayerController : MonoBehaviour {
#endif

    [SerializeField]
    GameObject ballPrefab;
#if ENABLE_AR_TUTORIAL

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
#endif
}