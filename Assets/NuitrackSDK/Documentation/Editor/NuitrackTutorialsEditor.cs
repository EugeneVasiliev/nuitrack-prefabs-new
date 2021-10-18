using UnityEditor;

namespace NuitrackSDKEditor.Documentation
{
    [CustomEditor(typeof(NuitrackTutorials), true)]
    public class NuitrackTutorialsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            NuitrackTutorialsEditorWindow.DrawTutorials();
            // pass all draws
        }
    }
}
