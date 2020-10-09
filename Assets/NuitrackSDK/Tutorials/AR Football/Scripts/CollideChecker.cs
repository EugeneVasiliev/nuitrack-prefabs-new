using UnityEngine;

public class CollideChecker : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        GetComponentInParent<BallController>().OnCollide(collision); //Если мячик с чем-то столкнулся, то сообщаём об этом BallController, который находится на родительском объекте
    }
}
