#if ENABLE_AR_TUTORIAL
using UnityEngine;

public class CollideChecker : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        GetComponentInParent<BallController>().OnCollide(collision); // If the ball collides with something, then we report this to the BallController, which is located on the parent object
    }
}
#endif