#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;


namespace NuitrackSDKEditor.ErrorSolver
{
    [InitializeOnLoad]
    public class TBBReplacer
    {
        static readonly string batName = "TBBReplacer.bat";

        static string EditorPath
        {
            get
            {
                return Path.GetDirectoryName(EditorApplication.applicationPath);
            }
        }

        static string TbbBackupPath
        {
            get
            {
                return Path.Combine(EditorPath, "tbb_backup.dll");
            }
        }

        static string TbbPath
        {
            get
            {
                return Path.Combine(EditorPath, "tbb.dll");
            }
        }

        public static bool Ready
        {
            get
            {
                return File.Exists(TbbBackupPath);
            }
        }

        static TBBReplacer()
        {
            Start();
        }

        public static void Start()
        {
            if (!Ready)
            {
#if UNITY_EDITOR_WIN
                if (EditorUtility.DisplayDialog("TBB-file",
                        "You need to replace the tbb.dll file in Editor with Nuitrack compatible tbb.dll file. \n" +
                        "If you click [Yes] the editor will be restarted and the file will be replaced automatically \n" +
                        "(old tbb-file will be renamed to tbb_backup.dll)", "Yes", "No"))
                {
                    CreateBat();
                }
#endif
            }
        }

        static void CreateBat()
        {
#if UNITY_EDITOR_WIN
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            if (File.Exists(batName))
                File.Delete(batName);

            string nuitrackHomePath = System.Environment.GetEnvironmentVariable("NUITRACK_HOME");
            string nuitrackTbbPath = Path.Combine(nuitrackHomePath, "bin", "tbb.dll");

            using (StreamWriter sw = new StreamWriter(batName))
            {
                sw.WriteLine(string.Format("rename \"{0}\" {1}", TbbPath, "tbb_backup.dll"));
                sw.WriteLine(string.Format("copy \"{0}\" \"{1}\"", nuitrackTbbPath, TbbPath));
                sw.WriteLine(string.Format("start \"\" \"{0}\" -projectPath \"{1}\"", EditorApplication.applicationPath, Directory.GetCurrentDirectory()));
                sw.WriteLine(string.Format("del \"{0}\"", batName));
            }

            EditorApplication.quitting += Quit;
            EditorApplication.Exit(0);
#endif
        }

        static void Quit()
        {
            ProgramStarter.Run(batName, "");
        }
    }
}

#endif