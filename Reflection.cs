using UnityEngine;
using UnityEngine.UI;
// nem használt reflekciók viszont fejlesztéshez használhatóak mivel tesztelés során működőképesnek bizonyult, viszont nem tökéletes.
public class ScratchesReflection : MonoBehaviour
{
    public Image scratchesImage;
    public Transform playerCamera;
    public Light testLight;

    public float maxAlpha = 0.20f; 
    public float minAlpha = 0f;

    public float fadeDistance = 5f; 

    public void ReflectionSetup()
    {
        if (scratchesImage == null)
        {
            GameObject imgObj = GameObject.Find("Scratches");
            if (imgObj != null)
            {
                scratchesImage = imgObj.GetComponent<Image>();
                Debug.Log("ScratchesReflection: Found scratchesImage.");
            }
            else
            {
                Debug.LogWarning("ScratchesReflection: Could not find scratchesImage!");
            }
        }
        if (playerCamera == null)
        {
            GameObject camObj = GameObject.Find("Main Camera");
            if (camObj != null)
            {
                playerCamera = camObj.transform;
                Debug.Log("ScratchesReflection: Found playerCamera.");
            }
            else
            {
                Debug.LogWarning("ScratchesReflection: Could not find playerCamera!");
            }
        }
        if (testLight == null)
        {
            GameObject lightObj = GameObject.Find("TestLight");
            if (lightObj != null)
            {
                testLight = lightObj.GetComponent<Light>();
                Debug.Log("ScratchesReflection: Found testLight.");
            }
            else
            {
                Debug.LogWarning("ScratchesReflection: Could not find testLight!");
            }
        }
    }

    void Update()
    {
        if (scratchesImage == null || playerCamera == null || testLight == null)
            return;

        Vector3 toLight = (testLight.transform.position - playerCamera.position);
        float distance = toLight.magnitude;
        Vector3 toLightDir = toLight.normalized;
        float dot = Vector3.Dot(playerCamera.forward, toLightDir);

        
        float boostedDot = Mathf.Clamp01((dot + 1f) / 2f); 
        float angleFactor = Mathf.Max(boostedDot, 0.1f);

        float distanceFactor = Mathf.Clamp01(2f - (distance / fadeDistance));
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, angleFactor * distanceFactor);

        Color c = scratchesImage.color;
        c.a = alpha;
        scratchesImage.color = c;
    }
}