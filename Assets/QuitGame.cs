using UnityEngine;

public class QuitGame : MonoBehaviour
    {
    public void Quit()
        {
        // Quit the application
        Application.Quit();

        // If we're running in the Unity editor, stop playing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
