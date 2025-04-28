using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public LevelLoader levelLoader;
    
    public void StartGame()
    {
        levelLoader.Load2DScene();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
