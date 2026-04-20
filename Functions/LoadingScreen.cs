using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    public GameObject loadingPanel;   // root panel (enable/disable)
    public Slider progressBar;        // UI Slider for progress [0..1]
    public Text progressText;         
    public float minShowTime = 0.5f;  

    void Awake()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    // scene töltése
    public void StartLoading(string sceneName)
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float startTime = Time.unscaledTime;

        while (op.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = Mathf.RoundToInt(progress * 100f) + "%";
            yield return null;
        }

        // visuals
        if (progressBar != null) progressBar.value = 1f;
        if (progressText != null) progressText.text = "100%";

        // hogy ne tűnjön el azonnal, ha gyorsan töltődik a jelenet, legalább minShowTime-ig mutatjuk
        float elapsed = Time.unscaledTime - startTime;
        if (elapsed < minShowTime) yield return new WaitForSecondsRealtime(minShowTime - elapsed);

        // scene töltése engedélyezése
        op.allowSceneActivation = true;
        yield return op;
    }
}