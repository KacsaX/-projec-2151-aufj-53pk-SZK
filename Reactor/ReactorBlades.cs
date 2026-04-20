using UnityEngine;

public class ReactorBlades : MonoBehaviour
{
    public float speed = 180f; // degrees per second

    void Update()
    {
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);
    }
}