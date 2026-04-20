using UnityEngine;

public class SmallFan : MonoBehaviour
{
    public float speed = 180f; // degrees per second

    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
    }
}