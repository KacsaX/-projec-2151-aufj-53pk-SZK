using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenController : MonoBehaviour
{
    [Header("UI refs (assign in Game scene)")]
    public Image blackOverlay;     // full-screen kép (fekete), alpha starts at 0
    public TextMeshProUGUI youDiedText;

    [Header("Timings")]
    public float fadeDuration = 0.7f;
    public float textDelay = 0.3f;

    void Awake()
    {
        // Biztonságos alapértelmezett értékek, ha nincs hozzárendelve
        if (blackOverlay != null)
            blackOverlay.color = new Color(0f, 0f, 0f, 0f);
        if (youDiedText != null)
        {
            var c = youDiedText.color;
            youDiedText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    //ha a játékos meghal, ez a függvény hívódik meg
    public void ShowDeathScreen()
    {
        StopAllCoroutines();
        StartCoroutine(FadeInRoutine());
    }

    IEnumerator FadeInRoutine()
    {
        // fekete kép megjelenítése, időt nem befolyásolja (Annak érdekében hogy ha a játékot esetleg többjátékosra akarjuk fejleszteni)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            if (blackOverlay != null)
                blackOverlay.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }

        // "You Died" szöveg megjelenítése rövid késleltetés után
        yield return new WaitForSecondsRealtime(textDelay);

        if (youDiedText != null)
        {
            var c = youDiedText.color;
            youDiedText.color = new Color(c.r, c.g, c.b, 1f);
        }

    }

    // Nincsenek alkalmazva, viszont további fejlesztéshez útmutatóként hasznosak lehetnek
    
    // Ha a játékos a főmenübe szeretne visszatérni (nincs használva, de hasznos lehet fejlesztéshez)
    public void OnQuitToMenu()
    {
        // Biztonságos alapértelmezett értékek visszaállítása, ha szükséges
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    // Újraéledés helye
    public void OnRespawnReload()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Spectate opció hasznos lehet többjátékos módhoz, hogy a halál után a játékos nézhesse a többieket (fejlesztéshez hasznos lehet)
    public void BeginSpectate()
    {
        // SpectateController helye
    }
}