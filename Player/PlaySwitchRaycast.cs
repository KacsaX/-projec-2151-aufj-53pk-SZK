using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwitchRaycast : MonoBehaviour
{
    public float interactDistance = 3f;

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                SwitchInteraction switchScript = hit.collider.GetComponent<SwitchInteraction>();
                if (switchScript != null && switchScript.powerBox != null)
                {
                    switchScript.powerBox.ToggleSwitch(switchScript.switchIndex);
                }
            }
        }
    }
}