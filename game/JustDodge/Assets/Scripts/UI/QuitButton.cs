using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void RequestQuit()
    {
        if(Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Application.Quit();
        }
    }
}
