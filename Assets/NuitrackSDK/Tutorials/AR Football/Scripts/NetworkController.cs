using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkController : MonoBehaviour
{
    [HideInInspector]
    public int score;
    public bool isClient; //в редакторе выбираем это клиент или сервер

    [Header("Server")]
    [SerializeField]
    Text scoreText; //Text куда будут выводится полученные очки
    [SerializeField]
    Text connectionsText; //Text куда будет выводится количество подключённых игроков
    [SerializeField]
    GameObject environmentPrefab;

    [Header("Client")]
    [SerializeField]
    Text connectText; //Текстовое поле куда будет выводиться сообщение при коннекте

    private void Start()
    {
        //Если это не клиент, то создаём сервер
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
            scoreText.text = "Scores: "+ score.ToString(); //Обновляем счётчик очков
            connectionsText.text = "connected: " + NetworkManager.singleton.numPlayers; //Количество подключённых игроков
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
