using UnityEngine;

public class GaugeController : MonoBehaviour
{
    public Transform needle;
    public float minAngle = -90f;
    public float maxAngle = 90f;
    public float smoothSpeed = 5f; // Higher = faster

    private float targetAngle;
    private float currentAngle;

    public void SetGauge(float percent)
    {
        Debug.Log($"SetGauge called with percent={percent}");
        targetAngle = Mathf.Lerp(minAngle, maxAngle, percent);
    }

    void Start()
    {
        currentAngle = minAngle;
        targetAngle = minAngle;
    }

void Update()
{
    currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * smoothSpeed);

    // Mozogás közben kis "twitch" effektet adunk, hogy élettelibb legyen
    if (Mathf.Abs(currentAngle - targetAngle) < 0.5f)
    {
        float twitch = Random.Range(-1f, 1f);
        needle.localRotation = Quaternion.Euler(0, 0, currentAngle + twitch);
    }
    else
    {
        needle.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
}
}