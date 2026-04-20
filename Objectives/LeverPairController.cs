using UnityEngine;

public class LeverPairController : MonoBehaviour
{
    public LeverInteraction leftLever;
    public LeverInteraction rightLever;
    public float interval = 3f;
    public string controlledPanelName = "Fuel";

    private ReactorControlLogic logic;
    private int maxValue = 100;
    private int minValue = 0;
    private Coroutine leftCoroutine;
    private Coroutine rightCoroutine;

    void Start()
    {
        logic = FindObjectOfType<ReactorControlLogic>();

        leftLever.OnLeverToggled += OnLeftLeverToggled;
        rightLever.OnLeverToggled += OnRightLeverToggled;
    }

    private void OnLeftLeverToggled(bool isOn)
    {
        if (isOn)
        {
            if (rightLever.IsOn)
                rightLever.SetLeverState(false);

            if (leftCoroutine == null)
                leftCoroutine = StartCoroutine(AddValueRoutine());
        }
        else
        {
            if (leftCoroutine != null)
            {
                StopCoroutine(leftCoroutine);
                leftCoroutine = null;
            }
        }
    }

    private void OnRightLeverToggled(bool isOn)
    {
        if (isOn)
        {
            if (leftLever.IsOn)
                leftLever.SetLeverState(false);

            if (rightCoroutine == null)
                rightCoroutine = StartCoroutine(SubtractValueRoutine());
        }
        else
        {
            if (rightCoroutine != null)
            {
                StopCoroutine(rightCoroutine);
                rightCoroutine = null;
            }
        }
    }

    private System.Collections.IEnumerator AddValueRoutine()
    {
        while (leftLever.IsOn)
        {
            yield return new WaitForSeconds(interval);
            if (logic != null)
            {
                int value = logic.GetCurrentValue(controlledPanelName);
                value = Mathf.Min(value + 5, maxValue);
                logic.SetPanelValue(controlledPanelName, value);
                Debug.Log($"{controlledPanelName} increased to {value}");
            }
        }
    }

    private System.Collections.IEnumerator SubtractValueRoutine()
    {
        while (rightLever.IsOn)
        {
            yield return new WaitForSeconds(interval);
            if (logic != null)
            {
                int value = logic.GetCurrentValue(controlledPanelName);
                value = Mathf.Max(value - 5, minValue);
                logic.SetPanelValue(controlledPanelName, value);
                Debug.Log($"{controlledPanelName} decreased to {value}");
            }
        }
    }
}