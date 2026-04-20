using UnityEngine;

public class VentSpawner : MonoBehaviour
{
    public GameObject ventPrefab;

    public void SpawnVents()
    {
        // minden gameoject amely neve "Vent" keresése a jelenetben
        GameObject[] vents = GameObject.FindGameObjectsWithTag("Vent");
        if (vents.Length == 0)
        {
            
            vents = GameObject.FindObjectsOfType<GameObject>();
            vents = System.Array.FindAll(vents, go => go.name == "Vent");
        }

        foreach (GameObject vent in vents)
        {
            // Véletlenszerűen döntse el, hogy létrehoz-e egy prefab-t ezen a venten
            if (Random.value > 0.5f) // 50% esély, szükség szerint állítható
            {
                Instantiate(ventPrefab, vent.transform.position, vent.transform.rotation, vent.transform);
            }
        }
    }
}