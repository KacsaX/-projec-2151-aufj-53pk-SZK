using UnityEngine;
using TMPro;

public class FrequencyKnobController : MonoBehaviour
{
    public FrequencyGenerator generator;
    public string controlledPanelName = "Reactor Shielding Level";
    [Range(0, 20)] public int knobHundreds = 5;
    [Range(0, 9)] public int knobTens = 0;
    [Range(0, 9)] public int knobUnits = 0;

    public TMP_Text frequencyDisplay;

    public int Frequency
    {
        get
        {
            int freq = knobHundreds * 100 + knobTens * 10 + knobUnits;
            return Mathf.Clamp(freq, 50, 2000);
        }
    }

    void Update()
    {
        if (frequencyDisplay != null)
            frequencyDisplay.text = $"{Frequency} Hz";
    }

    
    public void SetHundreds(int value)
    {
        knobHundreds = Mathf.Clamp(value, 0, 20);
    }
    public void SetTens(int value)
    {
        knobTens = Mathf.Clamp(value, 0, 9);
    }
    public void SetUnits(int value)
    {
        knobUnits = Mathf.Clamp(value, 0, 9);
    }

    public void Submit()
    {
        if (generator != null && generator.frequencyMap.ContainsKey(Frequency))
        {
            int value = generator.frequencyMap[Frequency];
            Debug.Log($"Submitted {Frequency} Hz: Setting {controlledPanelName} to {value}");

            ReactorControlLogic logic = FindObjectOfType<ReactorControlLogic>();
            if (logic != null)
                logic.SetPanelValue(controlledPanelName, value);
        }
        else
        {
            Debug.Log($"Submitted {Frequency} Hz: Invalid frequency!");
        }
    }
}