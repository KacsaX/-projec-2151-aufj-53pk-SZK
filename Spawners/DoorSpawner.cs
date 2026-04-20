using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DoorSpawner : MonoBehaviour
{
    public GameObject slidingDoorPrefab;
    public GameObject simpleDoorPrefab;
    public GameObject brokenDoorPrefab;
    public float positionTolerance = 0.05f;

    public void PlaceDoors()
    {
        // minden modulban keresse meg az "Exit" nevű transformokat
        List<Transform> allExits = new List<Transform>();
        foreach (GameObject module in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            foreach (Transform t in module.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("Exit"))
                    allExits.Add(t);
            }
        }

        // Távolítsa el a null értékeket és az ismétlődéseket
        allExits = allExits
            .Where(t => t != null)
            .Distinct()
            .ToList();

        HashSet<Transform> alreadyPaired = new HashSet<Transform>();
        HashSet<Vector3> doorPositions = new HashSet<Vector3>();

        for (int i = 0; i < allExits.Count; i++)
        {
            Transform exitA = allExits[i];
            if (alreadyPaired.Contains(exitA)) continue;

            BoxCollider colA = exitA.GetComponent<BoxCollider>();
            if (colA == null) continue;

            bool foundPair = false;
            for (int j = 0; j < allExits.Count; j++)
            {
                if (i == j) continue;
                Transform exitB = allExits[j];
                if (alreadyPaired.Contains(exitB)) continue;

                BoxCollider colB = exitB.GetComponent<BoxCollider>();
                if (colB == null) continue;

                // Ellenőrizze, hogy a doboz kolliderek átfedik-e egymást
                if (colA.bounds.Intersects(colB.bounds))
                {
                    // Helyezzen el egy ajtót csak egyszer páronként
                    Vector3 doorPos = exitA.position;
                    if (!doorPositions.Contains(doorPos))
                    {
                        Quaternion doorRot = Quaternion.LookRotation(exitA.forward, Vector3.up);
                        GameObject doorPrefab = Random.value < 0.5f ? slidingDoorPrefab : simpleDoorPrefab;
                        Instantiate(doorPrefab, doorPos, doorRot);
                        doorPositions.Add(doorPos);
                    }
                    alreadyPaired.Add(exitA);
                    alreadyPaired.Add(exitB);
                    foundPair = true;
                    break;
                }
            }
            if (!foundPair)
            {
                // Helyezzen el egy törött ajtót a végpontnál
                Vector3 brokenPos = exitA.position;
                Quaternion doorRot = Quaternion.LookRotation(exitA.forward, Vector3.up);
                Instantiate(brokenDoorPrefab, brokenPos, doorRot);
            }
        }
    }
}