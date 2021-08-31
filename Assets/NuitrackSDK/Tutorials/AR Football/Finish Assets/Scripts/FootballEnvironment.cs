using UnityEngine;
#if ENABLE_AR_TUTORIAL
using UnityEngine.Networking;
#endif

public class FootballEnvironment : MonoBehaviour {

    public Transform aim;
    [SerializeField] Vector3 clientSize;

    void Start()
    {
#if ENABLE_AR_TUTORIAL
        if(FindObjectOfType<NetworkIdentity>().isServer == false)
            transform.localScale = clientSize;
#endif
    }
}