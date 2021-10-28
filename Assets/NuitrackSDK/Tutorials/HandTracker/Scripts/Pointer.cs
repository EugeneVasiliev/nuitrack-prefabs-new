using UnityEngine;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    public enum Hands { left = 0, right = 1 };

    [SerializeField]
    Hands currentHand;

    [Header ("Visualization")]
    [SerializeField]
    RectTransform parentRectTransform;

    [SerializeField]
    RectTransform baseRect;

    [SerializeField]
    Image background;

    [SerializeField]
    Sprite defaultSprite;

    [SerializeField]
    Sprite pressSprite;

    [SerializeField]
    [Range(0, 50)]
    float minVelocityInteractivePoint = 2f;

    bool active = false;

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public bool Press
    {
        get; private set;
    }

    void Update()
    {
        active = false;

        UserData user = NuitrackManager.Users.Current;

        if (user != null)
        {
            UserData.Hand handContent = currentHand == Hands.right ? user.RightHand : user.LeftHand;

            if (handContent != null)
            {
                Vector3 lastPosition = baseRect.position;
                Vector2 handPosition = handContent.ProjPosition * parentRectTransform.rect.size;
                handPosition.y = -(parentRectTransform.rect.height - handPosition.y);
                baseRect.anchoredPosition = handPosition;

                float velocity = (baseRect.position - lastPosition).magnitude / Time.deltaTime;

                if (velocity < minVelocityInteractivePoint)
                    Press = handContent.Click;

                active = true;
            }
        }

        Press = Press && active;

        background.enabled = active;
        background.sprite = active && Press ? pressSprite : defaultSprite;
    }
}
