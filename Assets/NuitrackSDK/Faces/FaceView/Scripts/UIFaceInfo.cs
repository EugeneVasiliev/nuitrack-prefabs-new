﻿using UnityEngine;
using UnityEngine.UI;


namespace NuitrackSDK.Face
{
    [AddComponentMenu("NuitrackSDK/Face/Face View/UI Face Info")]
    public class UIFaceInfo : MonoBehaviour
    {
        [Header("Info")]
        public bool autoProcessing;
        [SerializeField] bool showInfo = true;
        [SerializeField] GameObject infoPanel;
        [SerializeField] Text ageText;
        [SerializeField] Text yearsText;
        [SerializeField] Text genderText;
        [SerializeField] Slider neutral;
        [SerializeField] Slider angry;
        [SerializeField] Slider surprise;
        [SerializeField] Slider happy;

        RectTransform spawnTransform;
        RectTransform frameTransform;
        Image image;

        public void Initialize(RectTransform spawnTransform)
        {
            this.spawnTransform = spawnTransform;

            frameTransform = GetComponent<RectTransform>();
            image = frameTransform.GetComponent<Image>();
        }

        void Update()
        {
            if (autoProcessing)
            {
                ProcessFace(NuitrackManager.Users.Current);
            }
        }

        public void ProcessFace(UserData userData)
        {
            if (!NuitrackManager.Instance.UseFaceTracking)
                Debug.Log("Attention: Face tracking disabled! Enable it on the Nuitrack Manager component");

            if (userData == null)
                return;

            nuitrack.Face currentFace = userData.Face;

            if (currentFace != null && spawnTransform)
            {
                image.enabled = true;
                infoPanel.SetActive(showInfo);

                Rect faceRect = currentFace.Rect;

                Vector2 newPosition = new Vector2(
                    spawnTransform.rect.width * (faceRect.x - 0.5f) + frameTransform.rect.width / 2,
                    spawnTransform.rect.height * (0.5f - faceRect.y) - frameTransform.rect.height / 2);

                frameTransform.sizeDelta = new Vector2(faceRect.width * spawnTransform.rect.width, faceRect.height * spawnTransform.rect.height);
                frameTransform.anchoredPosition = newPosition;

                ageText.text = currentFace.AgeType.ToString();
                yearsText.text = string.Format("Years: {0:F1}", currentFace.age.years);
                genderText.text = currentFace.Gender.ToString();

                neutral.value = currentFace.GetEmotionValue(nuitrack.Emotions.Type.neutral);
                angry.value = currentFace.GetEmotionValue(nuitrack.Emotions.Type.angry);
                surprise.value = currentFace.GetEmotionValue(nuitrack.Emotions.Type.surprise);
                happy.value = currentFace.GetEmotionValue(nuitrack.Emotions.Type.happy);
            }
            else
            {
                image.enabled = false;
                infoPanel.SetActive(false);
            }
        }
    }
}