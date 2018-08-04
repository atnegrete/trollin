using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class LoadingManager : Singleton<LoadingManager>
{
    public static int LOGIN_SCENE = 0;
    public static int LOBBY_SCENE = 1;
    public static int GAME_SCENE = 2;

    public void LoadNextScene()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex + 1);
    }

    public void LoadPreviousScene()
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(activeSceneIndex - 1);
    }
}