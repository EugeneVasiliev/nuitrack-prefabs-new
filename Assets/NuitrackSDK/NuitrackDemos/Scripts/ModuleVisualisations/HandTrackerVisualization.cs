using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace NuitrackSDK.NuitrackDemos
{
    public class HandTrackerVisualization : MonoBehaviour
    {
        [SerializeField] Transform handsContainer;
        [SerializeField] GameObject handUIPrefab;
        [SerializeField] float sizeNormal = 0, sizeClick = 0;
        [SerializeField] Color leftColor = Color.white, rightColor = Color.red;
        Dictionary<int, Image[]> hands;

        void Start()
        {
            hands = new Dictionary<int, Image[]>();
        }

        void Update()
        {
            foreach (KeyValuePair<int, Image[]> kvp in hands)
            {
                if (NuitrackManager.Users.GetUser(kvp.Key) == null)
                {
                    hands[kvp.Key][0].enabled = false;
                    hands[kvp.Key][1].enabled = false;
                }
            }

            foreach (UserData userData in NuitrackManager.Users)
            {
                if (!hands.ContainsKey(userData.ID))
                    CreateNewHands(userData.ID);

                ControllHand(userData.ID, 0, userData.LeftHand);
                ControllHand(userData.ID, 1, userData.RightHand);
            }
        }

        void CreateNewHands(int userId)
        {
            hands.Add(userId, new Image[2]);
            GameObject leftHand = Instantiate(handUIPrefab);
            GameObject rightHand = Instantiate(handUIPrefab);

            leftHand.transform.SetParent(handsContainer, false);
            rightHand.transform.SetParent(handsContainer, false);

            hands[userId][0] = leftHand.GetComponent<Image>();
            hands[userId][1] = rightHand.GetComponent<Image>();

            hands[userId][0].enabled = false;
            hands[userId][1].enabled = false;
            hands[userId][0].color = leftColor;
            hands[userId][1].color = rightColor;
        }

        void ControllHand(int userID, int handImageID, nuitrack.HandContent? handContent)
        {
            if ((handContent == null) || (handContent.Value.X == -1f))
            {
                hands[userID][handImageID].enabled = false;
            }
            else
            {
                hands[userID][handImageID].enabled = true;
                Vector2 pos = new Vector2(handContent.Value.X, 1f - handContent.Value.Y);
                hands[userID][handImageID].rectTransform.anchorMin = pos;
                hands[userID][handImageID].rectTransform.anchorMax = pos;
                hands[userID][handImageID].rectTransform.sizeDelta = handContent.Value.Click ? new Vector2(sizeClick, sizeClick) : new Vector2(sizeNormal, sizeNormal);
            }
        }
    }
}