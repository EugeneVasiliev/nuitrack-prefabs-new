using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class NewBehaviourScript1 : MonoBehaviour
{
    private void Start()
    {
        LocalIPAddress();
    }

    public string LocalIPAddress()
    {
        IPHostEntry host;
        string localIP = "";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                print(localIP);
                break;
            }
        }
        return localIP;
    }
}
