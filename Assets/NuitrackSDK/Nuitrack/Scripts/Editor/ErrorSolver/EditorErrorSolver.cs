using UnityEditor;
using NuitrackSDK.ErrorSolver;


namespace NuitrackSDKEditor.ErrorSolver
{
    [InitializeOnLoad]
    public static class EditorErrorSolver
    {
        static EditorErrorSolver()
        {
            NuitrackErrorSolver.OnError += NuitrackErrorSolver_OnError;
        }

        static string NuitrackErrorSolver_OnError(string error)
        {
            UnityEngine.Debug.Log("MESSAGE EditorErrorSolver\n" + error);
            string errorMessage = error;

            if (error.Contains("TBB"))
            {
#if UNITY_EDITOR_WIN
                string nuitrackHomePath = System.Environment.GetEnvironmentVariable("NUITRACK_HOME");
                string nuitrackTbbPath = nuitrackHomePath + "\\bin\\tbb.dll";

                string unityTbbPath = EditorApplication.applicationPath.Replace("Unity.exe", "") + "tbb.dll";
                errorMessage = "<color=red><b>You need to replace the file " + unityTbbPath +
                    " with Nuitrack compatible file " + nuitrackTbbPath + " (Don't forget to close the editor first)</b></color>";

                TBBReplacer.ShowMessage();
#endif
            }

            return errorMessage;
        }
    }
}