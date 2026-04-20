using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.2f;
    public float shakeAngle = 2f; // Max tilt in degrees

    private Vector3 originalPos;
    private Quaternion originalRot;
    private float shakeTime = 0f;

    void Start()
    {
        originalPos = transform.localPosition;
        originalRot = transform.localRotation;
    }

    void LateUpdate()
    {
        if (shakeTime > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;
            transform.localRotation = originalRot * Quaternion.Euler(
                Random.Range(-shakeAngle, shakeAngle),
                Random.Range(-shakeAngle, shakeAngle),
                Random.Range(-shakeAngle, shakeAngle)
            );
            shakeTime -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPos;
            transform.localRotation = originalRot;
        }
    }

    public void Shake()
    {
        shakeTime = shakeDuration;
    }

    public bool IsShaking => shakeTime > 0f;
}