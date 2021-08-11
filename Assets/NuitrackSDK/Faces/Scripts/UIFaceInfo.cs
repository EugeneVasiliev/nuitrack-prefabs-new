using UnityEngine;
using UnityEngine.UI;

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

    RectTransform canvasTransform;

    RectTransform frameTransform;

    Instances[] faces;

    JsonInfo faceInfo;

    void Start()
    {
        frameTransform = GetComponent<RectTransform>();
        canvasTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        infoPanel.SetActive(showInfo);
    }

    void Update()
    {
        if (autoProcessing)
        {
            ProcessFace(CurrentUserTracker.CurrentSkeleton);
        }
    }

    public void ProcessFace(nuitrack.Skeleton skeleton)
    {
        try
        {
            string json = nuitrack.Nuitrack.GetInstancesJson();
            faceInfo = JsonUtility.FromJson<JsonInfo>(json.Replace("\"\"", "[]"));

            faces = faceInfo.Instances;
            for (int i = 0; i < faces.Length; i++)
            {
                if (faces != null && i < faces.Length && skeleton.ID == faces[i].id)
                {
                    Face currentFace = faces[i].face;

                    if (skeleton != null && currentFace.rectangle != null && canvasTransform)
                    {
                        frameTransform.GetComponent<Image>().enabled = true;
                        infoPanel.SetActive(showInfo);

                        Vector2 newPosition = new Vector2(
                            canvasTransform.rect.width * (Mathf.Clamp01(currentFace.rectangle.left) - 0.5f) + frameTransform.rect.width / 2,
                            canvasTransform.rect.height * (0.5f - Mathf.Clamp01(currentFace.rectangle.top)) - frameTransform.rect.height / 2);

                        frameTransform.sizeDelta = new Vector2(currentFace.rectangle.width * canvasTransform.rect.width, currentFace.rectangle.height * canvasTransform.rect.height);
                        frameTransform.anchoredPosition = newPosition;

                        ageText.text = currentFace.age.type;
                        yearsText.text = "Years: " + currentFace.age.years.ToString("F1");
                        genderText.text = currentFace.gender;

                        neutral.value = currentFace.emotions.neutral;
                        angry.value = currentFace.emotions.angry;
                        surprise.value = currentFace.emotions.surprise;
                        happy.value = currentFace.emotions.happy;
                    }
                    else
                    {
                        frameTransform.GetComponent<Image>().enabled = false;
                        infoPanel.SetActive(false);
                    }
                }
            }
        }
        catch (System.Exception)
        {
            Debug.Log("Face Error ");
        }
    }
}
