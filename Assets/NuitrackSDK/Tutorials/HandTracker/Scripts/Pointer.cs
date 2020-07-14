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

    bool active = false;

    private void Start()
    {
        NuitrackManager.onHandsTrackerUpdate += NuitrackManager_onHandsTrackerUpdate;
    }

    private void OnDestroy()
    {
        NuitrackManager.onHandsTrackerUpdate -= NuitrackManager_onHandsTrackerUpdate;
    }

    public bool Press
    {
        get; private set;
    }

    private void NuitrackManager_onHandsTrackerUpdate(nuitrack.HandTrackerData handTrackerData)
    {
        active = false;
        Press = false;

        if (handTrackerData != null)
        {
            nuitrack.UserHands userHands = handTrackerData.GetUserHandsByID(CurrentUserTracker.CurrentUser);

            if (userHands != null)
            {
                Vector2 pageSize = parentRectTransform.rect.size;

                if (currentHand == Hands.right && userHands.RightHand != null)
                {
                    baseRect.anchoredPosition = new Vector2(userHands.RightHand.Value.X * pageSize.x, -userHands.RightHand.Value.Y * pageSize.y);
                    active = true;
                    Press = userHands.RightHand.Value.Click;
                }
                else if (currentHand == Hands.left && userHands.LeftHand != null)
                {                    
                    baseRect.anchoredPosition = new Vector2(userHands.LeftHand.Value.X * pageSize.x, -userHands.LeftHand.Value.Y * pageSize.y);
                    active = true;
                    Press = userHands.LeftHand.Value.Click;
                }
            }
        }

        background.enabled = active;
        background.sprite = active && Press ? pressSprite : defaultSprite;
    }
}
