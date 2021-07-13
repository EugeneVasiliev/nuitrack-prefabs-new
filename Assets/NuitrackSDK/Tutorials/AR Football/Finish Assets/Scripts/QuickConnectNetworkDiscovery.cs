#if ENABLE_AR_TUTORIAL
using UnityEngine.Networking;

public class QuickConnectNetworkDiscovery : NetworkDiscovery
{
    // Overridden method of the NetworkDiscovery class. Initially, it simply receives messages from the found servers (does not connect). Now OnReceivedBroadcast automatically connects to it when the server is found
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);

        if (NetworkManager.singleton.IsClientConnected())
            return;

        NetworkManager.singleton.networkAddress = fromAddress; // Found IP is substituted into NetworkManager
        NetworkManager.singleton.StartClient(); // Connects
    }
}
#endif
