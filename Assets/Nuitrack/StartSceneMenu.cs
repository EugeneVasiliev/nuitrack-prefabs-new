using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartSceneMenu : MonoBehaviour 
{
  public void LoadScene1()
  {
    SceneManager.LoadScene(1);
  }

  public void LoadScene2()
  {
    SceneManager.LoadScene(2);
  }
}
