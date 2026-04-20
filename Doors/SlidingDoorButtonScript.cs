using UnityEngine;
using UnityEngine.InputSystem;

public class DoorButton : MonoBehaviour
{
    public SlidingDoor door;
    public GameObject promptText;
    public float interactDistance = 2f;
    public float animationLength = 1f;

    private bool playerLooking = false;
    private bool isAnimating = false;

    void Update()
    {
        playerLooking = false;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                if (hit.collider == GetComponent<Collider>())
                {
                    playerLooking = true;
                }
            }
        }

        if (promptText != null)
            promptText.SetActive(playerLooking && !isAnimating);

        if (playerLooking && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && !isAnimating)
        {
            if (door.IsOpen)
                StartCoroutine(AnimateDoor(() => door.CloseDoor()));
            else
                StartCoroutine(AnimateDoor(() => door.OpenDoor()));
        }
    }

    private System.Collections.IEnumerator AnimateDoor(System.Action doorAction)
    {
        isAnimating = true;
        doorAction.Invoke();
        yield return new WaitForSeconds(animationLength);
        isAnimating = false;
    }
}