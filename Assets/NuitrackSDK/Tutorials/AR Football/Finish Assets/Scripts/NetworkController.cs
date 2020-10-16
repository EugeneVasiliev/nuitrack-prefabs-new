#if ENABLE_AR_TUTORIAL
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkController : MonoBehaviour
{
    [HideInInspector]
    public int score;
    public bool isClient; //in the editor, select this client or server

    [Header("Server")]
    [SerializeField]
    Text scoreText; //Text where the received points will be displayed
    [SerializeField]
    Text connectionsText; //Text where the number of connected players will be displayed
    [SerializeField]
    GameObject environmentPrefab;

    [Header("Client")]
    [SerializeField]
    Text connectText; //The text field where the message will be displayed upon connection

    private void Start()
    {
        //If this is not a client, then we create a server
        if (isClient == false)
        {
            StartServer();
        }
    }

    public void StartClient()
    {
        FindObjectOfType<NetworkDiscovery>().Initialize();
        FindObjectOfType<NetworkDiscovery>().StartAsClient();
    }

    void StartServer()
    {
        FindObjectOfType<NetworkDiscovery>().Initialize();
        FindObjectOfType<NetworkDiscovery>().StartAsServer();
        NetworkManager.singleton.StartHost();

        GameObject environment = (GameObject)Instantiate(environmentPrefab);
        NetworkServer.Spawn(environment);
    }

    private void Update()
    {
        if (isClient == false)
        {
            scoreText.text = "Scores: "+ score.ToString(); //Updating the score counter
            connectionsText.text = "connected: " + NetworkManager.singleton.numPlayers; //Number of connected players
        }
        else
        {
            if(NetworkManager.singleton.IsClientConnected())
                connectText.text = "Connected";
            else
                connectText.text = "Connect";
        }
    }
}
#endif