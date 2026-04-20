using UnityEngine;
using System.Collections.Generic;

public class FrequencyGenerator : MonoBehaviour
{
    public string panelName = "Reactor Shielding Level";
    public Dictionary<int, int> frequencyMap = new Dictionary<int, int>();

    void Start()
    {
        GenerateFrequencies();
    }

    public void GenerateFrequencies()
    {
        frequencyMap.Clear();
        List<int> possibleFrequencies = new List<int>();
        for (int f = 50; f <= 999; f += 10)
            possibleFrequencies.Add(f);

        // Shuffle and pick 11 unique frequencies
        for (int i = 0; i < 11; i++)
        {
            int idx = Random.Range(0, possibleFrequencies.Count);
            int freq = possibleFrequencies[idx];
            possibleFrequencies.RemoveAt(idx);

            int value = i * 10; // 0, 10, 20, ..., 100

            frequencyMap.Add(freq, value);
        }

        // Log debuggoláshoz
        Debug.Log($"Frequency map for panel: {panelName}");
        foreach (var kvp in frequencyMap)
            Debug.Log($"Panel [{panelName}] Frequency {kvp.Key} Hz = Value {kvp.Value}");
    }
}