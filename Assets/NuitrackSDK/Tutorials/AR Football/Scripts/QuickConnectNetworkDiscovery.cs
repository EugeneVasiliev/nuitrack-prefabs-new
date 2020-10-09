using UnityEngine.Networking;

public class QuickConnectNetworkDiscovery : NetworkDiscovery
{

    // Переопределенный метод класса NetworkDiscovery. Изначально он просто получает сообщения от найденных серверов (не соединяется). Сейчас OnReceivedBroadcast при нахождении сервера автоматически с ним соединяется
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);

        if (NetworkManager.singleton.IsClientConnected())
            return;

        NetworkManager.singleton.networkAddress = fromAddress; // В NetworkManager подставляется найденный IP
        NetworkManager.singleton.StartClient(); // Соединяемся
    }
}

