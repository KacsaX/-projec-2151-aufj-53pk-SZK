using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    
    public LoadingScreenController loadingController;

    // Call from Play button
    public void PlayGame()
    {
        Time.timeScale = 1f;                          // game idő normálisra állítása
        Cursor.visible = false;                       // cursor rejtés
        Cursor.lockState = CursorLockMode.Locked;     // lock cursor

        if (loadingController != null)
            loadingController.StartLoading("Game");   // async load, loading UI-al
        else
            SceneManager.LoadScene("Game");           // vagy egyből töltés
    }

    // Call from Quit button
    public void QuitGame()
    {
        Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}