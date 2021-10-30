using UnityEngine;

public class FaceSwitcher : MonoBehaviour {

    [SerializeField] Face.GenderType gender;
    [SerializeField] Age.Type ageType;
    [SerializeField] Emotions.Type emotions;
    [SerializeField] GameObject enabledObject;
    [SerializeField] GameObject disabledObject;

    FaceController faceController;
    bool display = false;

    void Start ()
    {
        faceController = GetComponentInParent<FaceController>();
    }

    void Update()
    {
        display =   (gender == Face.GenderType.any || gender == faceController.genderType) &&
                    (ageType == Age.Type.any || ageType == faceController.ageType) &&
                    (emotions == Emotions.Type.any || emotions == faceController.emotionType);

        SwitchObjects();
    }

    void SwitchObjects()
    {
        if (enabledObject)
            enabledObject.SetActive(display);

        if (disabledObject)
            disabledObject.SetActive(!display);
    }
}
