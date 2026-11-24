using System.Runtime.InteropServices;
using UnityEngine;

public class QuitButton : MonoBehaviour
{
    [SerializeField] private string _pageUrl = "https://choclified.itch.io/just-dodge";

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void OpenInSameTab(string url);
#endif

    public void RequestQuit()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenInSameTab(_pageUrl);
#else
        Application.Quit();
#endif
    }
}
