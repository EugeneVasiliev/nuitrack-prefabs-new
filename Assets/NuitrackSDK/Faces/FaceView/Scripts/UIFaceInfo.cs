﻿using UnityEngine;
using UnityEngine.UI;


namespace NuitrackSDK.Face
{
    [AddComponentMenu("NuitrackSDK/Face/Face View/UI Face Info")]
    public class UIFaceInfo : UserTracker
    {
        [Header("Info")]
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

        protected override void Process(UserData userData)
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

                Rect screenRect = currentFace.AnchoredRect(spawnTransform.rect, frameTransform);

                frameTransform.sizeDelta = screenRect.size;
                frameTransform.anchoredPosition = screenRect.position;

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