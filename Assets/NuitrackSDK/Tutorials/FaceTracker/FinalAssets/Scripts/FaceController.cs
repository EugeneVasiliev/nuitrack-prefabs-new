using UnityEngine;

public class FaceController : MonoBehaviour 
{
    public Face.GenderType genderType;
    public Emotions.Type emotionType;
    public Age.Type ageType;

    public void SetFace(Face newFace)
    {
        //Gender
        genderType = newFace.Gender;

        //Age
        ageType = newFace.AgeType;

        //Emotion
        emotionType = newFace.PrevailingEmotion;
    }
}
