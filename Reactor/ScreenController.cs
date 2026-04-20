using UnityEngine;
using TMPro;

public class ScreenController : MonoBehaviour
{
    public TMP_Text valueText;

    public void SetValue(int value)
    {
        if (valueText != null)
            valueText.text = value.ToString();
    }

    public void SetValueText(string text)
    {
        if (valueText != null)
            valueText.text = text;
    }
}