using UnityEditor;
using UnityEngine;

using System.Collections.Generic;


namespace NuitrackSDKEditor.Readme
{
    public class NuitrackReadme : ScriptableObject
    {
        [System.Serializable]
        public class TutorialItem
        {
            [SerializeField] string label;
            [SerializeField] Texture previewImage;
            [SerializeField] string description;

            [SerializeField] string textURL;
            [SerializeField] string videoURL;
            [SerializeField] SceneAsset scene;
        }

        [SerializeField] List<TutorialItem> tutorialItems;
    }
}