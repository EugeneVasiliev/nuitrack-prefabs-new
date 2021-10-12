using UnityEditor;
using UnityEngine;

using System.Collections.Generic;


namespace NuitrackSDKEditor.Readme
{
    [HelpURL("https://github.com/3DiVi/nuitrack-sdk/tree/master/doc")]
    public class NuitrackTutorials : ScriptableObject
    {
        [System.Serializable]
        public class TutorialItem
        {
            [SerializeField] string label;
            [SerializeField] Texture previewImage;

            [SerializeField] string textURL;
            [SerializeField] string videoURL;
            
            [SerializeField, TextArea(1, 10)] string description;

            [SerializeField] SceneAsset scene;
        }

        [SerializeField] List<TutorialItem> tutorialItems;
    }
}