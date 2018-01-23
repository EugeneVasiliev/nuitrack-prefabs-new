using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneMenu : MonoBehaviour 
{
    private void Start()
    {
        //UnityEngine.VR.VRSettings.enabled = false;
    }

    public void LoadScene(int levelBuildIndex)
    {
        //UnityEngine.VR.VRSettings.enabled = true;
        SceneManager.LoadScene(levelBuildIndex);
    }
}
