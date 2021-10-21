using UnityEngine;
using UnityEngine.UI;

//using NuitrackSDK;


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

        Face currentFace = userData.Face;

        if (currentFace != null && currentFace.rectangle != null && spawnTransform)
        {
            image.enabled = true;
            infoPanel.SetActive(showInfo);

            Vector2 newPosition = new Vector2(
                spawnTransform.rect.width * (Mathf.Clamp01(currentFace.rectangle.left) - 0.5f) + frameTransform.rect.width / 2,
                spawnTransform.rect.height * (0.5f - Mathf.Clamp01(currentFace.rectangle.top)) - frameTransform.rect.height / 2);

            frameTransform.sizeDelta = new Vector2(currentFace.rectangle.width * spawnTransform.rect.width, currentFace.rectangle.height * spawnTransform.rect.height);
            frameTransform.anchoredPosition = newPosition;

            ageText.text = currentFace.age.type;
            yearsText.text = string.Format("Years: {0:F1}", currentFace.age.years);
            genderText.text = currentFace.gender;

            neutral.value = currentFace.emotions.neutral;
            angry.value = currentFace.emotions.angry;
            surprise.value = currentFace.emotions.surprise;
            happy.value = currentFace.emotions.happy;
        }
        else
        {
            image.enabled = false;
            infoPanel.SetActive(false);
        }

    }
}