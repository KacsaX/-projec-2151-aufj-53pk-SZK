using UnityEngine;

public class ReactorSpawner : MonoBehaviour
{
    public GameObject reactorPrefab;
    public GameObject[] reactorPanelPrefabs;

    public void SpawnReactorAndPanels()
    {
        // Létrehozó pontok keresése a jelenetben
        GameObject reactorSpawn = GameObject.Find("ReactorSpawn");
        GameObject reactorPanelsParent = GameObject.Find("ReactorPanels");

        // reaktor modell létrehozása
        if (reactorSpawn != null && reactorPrefab != null)
        {
            Instantiate(reactorPrefab, reactorSpawn.transform.position, reactorSpawn.transform.rotation, reactorSpawn.transform);
        }

        // reaktor panelek létrehozása a ReactorPanels gyermekeként
        if (reactorPanelsParent != null && reactorPanelPrefabs != null)
        {
            foreach (var panelPrefab in reactorPanelPrefabs)
            {
                if (panelPrefab != null)
                {
                    Instantiate(panelPrefab, reactorPanelsParent.transform.position, reactorPanelsParent.transform.rotation, reactorPanelsParent.transform);
                }
            }
        }
    }
}