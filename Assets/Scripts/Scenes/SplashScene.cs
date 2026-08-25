using UnityEngine;

public class SplashScene : MonoBehaviour
{   // a scene to prepare NetworkManager and other necessary objects before going to the MainScene

    private void Start()
    {
        UIManager.INSTANCE.LoadScene(Database.MAIN_SCENE);
    }
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}
