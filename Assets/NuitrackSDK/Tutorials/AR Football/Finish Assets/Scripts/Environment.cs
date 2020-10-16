#if ENABLE_AR_TUTORIAL
using UnityEngine;
using UnityEngine.Networking;

public class Environment : MonoBehaviour {

    public Transform aim;
    [SerializeField] Vector3 clientSize;

    void Start()
    {
        if(FindObjectOfType<NetworkIdentity>().isServer == false)
            transform.localScale = clientSize;
    }
}
#endif