﻿using UnityEngine;
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
            nuitrack.HandContent? handContent = currentHand == Hands.right ? user.RightHand : user.LeftHand;

            if (handContent != null)
            {
                Vector2 pageSize = parentRectTransform.rect.size;
                Vector3 lastPosition = baseRect.position;
                baseRect.anchoredPosition = new Vector2(handContent.Value.X * pageSize.x, -handContent.Value.Y * pageSize.y);

                float velocity = (baseRect.position - lastPosition).magnitude / Time.deltaTime;

                if (velocity < minVelocityInteractivePoint)
                    Press = handContent.Value.Click;

                active = true;
            }
        }

        Press = Press && active;

        background.enabled = active;
        background.sprite = active && Press ? pressSprite : defaultSprite;
    }
}
