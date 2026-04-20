using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Required")]
    public GameObject startPrefab;
    public GameObject playerPrefab;

    [Header("Module Pools (no 1-door here)")]
    public GameObject[] hallwayPrefabs;
    public GameObject[] roomPrefabs;

    [Header("Deadends (1-door caps)")]
    public GameObject[] deadendPrefabs;

    [Header("Objective")]
    public GameObject reactorRoomPrefab;

    [Header("Generation")]
    public int maxModules = 4;

    private readonly List<Transform> openExits = new List<Transform>();

    public void  GenerateMap()
    {
        GameObject start = Instantiate(startPrefab, Vector3.zero, Quaternion.identity);

        Transform playerSpawn = FindChild(start.transform, "PlayerSpawn");
        if (playerSpawn && playerPrefab)
            Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation);

        Transform startExit = FindChild(start.transform, "Exit");
        if (!startExit)
            return;
        openExits.Add(startExit);

        int modulesPlaced = 0;
        bool reactorRoomPlaced = false;
        int reactorRoomSlot = Random.Range(0, maxModules); // válasszon egy véletlenszerű helyet a reaktor szobának

        while (modulesPlaced < maxModules && openExits.Count > 0)
        {
            int idx = Random.Range(0, openExits.Count);
            Transform attachExit = openExits[idx];
            openExits.RemoveAt(idx);

            GameObject placed;
            Transform entranceUsed;

            // kényszerítse a reaktor szoba elhelyezését a kiválasztott helyre
            if (!reactorRoomPlaced && modulesPlaced == reactorRoomSlot)
            {
                placed = TryPlaceModuleWithPrefab(attachExit, out entranceUsed, reactorRoomPrefab);
                reactorRoomPlaced = placed != null;
            }
            else
            {
                placed = TryPlaceModuleWithRandomPrefab(attachExit, out entranceUsed, roomPrefabs, hallwayPrefabs);
            }

            if (placed == null || entranceUsed == null)
                continue;

            foreach (Transform t in placed.GetComponentsInChildren<Transform>(true))
            {
                if (t == entranceUsed) continue;
                if (t.name.StartsWith("Exit"))
                    openExits.Add(t);
            }

            modulesPlaced++;
        }

        CapAllOpenExitsWithDeadends();

        List<Transform> allExits = new List<Transform>();
        foreach (GameObject module in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            foreach (Transform t in module.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("Exit"))
                    allExits.Add(t);
            }
        }
//        DoorSpawner spawner = Object.FindFirstObjectByType<DoorSpawner>();
//        if (spawner != null)
//            StartCoroutine(CallDoorSpawnerNextFrame(spawner));
    }

//    private System.Collections.IEnumerator CallDoorSpawnerNextFrame(DoorSpawner spawner)
//    {
//        yield return null; // Wait one frame for all transforms/colliders to update
//        spawner.PlaceDoors();
//    }

    GameObject TryPlaceModuleWithReactor(Transform attachExit, out Transform entranceUsed, ref bool reactorRoomPlaced, int modulesPlaced)
    {
        entranceUsed = null;
        List<GameObject> candidates = new List<GameObject>();
        candidates.AddRange(roomPrefabs);
        candidates.AddRange(hallwayPrefabs);

        // reaktor szoba nem lehet az első szoba
        if (!reactorRoomPlaced && Random.value < 0.5f && modulesPlaced > 0)
        {
            candidates.Add(reactorRoomPrefab);
        }

        // Shuffle candidates randomizáláshoz
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = temp;
        }

        foreach (GameObject prefab in candidates)
        {
            GameObject module = Instantiate(prefab);
            Transform[] exits = module.GetComponentsInChildren<Transform>(true);
            Transform bestEntrance = null;
            float bestDot = -1f;

            foreach (Transform t in exits)
            {
                if (!t.name.StartsWith("Exit")) continue;
                float dot = Vector3.Dot(t.forward, -attachExit.forward);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestEntrance = t;
                }
            }

            if (bestEntrance == null)
            {
                Destroy(module);
                continue;
            }

            // Igazítás
            Quaternion rotDelta = Quaternion.FromToRotation(bestEntrance.forward, -attachExit.forward);
            module.transform.rotation = rotDelta * module.transform.rotation;

            // Bejárat újra lekérése forgatás után
            bestEntrance = FindChild(module.transform, bestEntrance.name);
            if (!bestEntrance)
            {
                Destroy(module);
                continue;
            }

            // Mozgassa úgy, hogy a bejárat illeszkedjen az attachExit-hez
            Vector3 offset = attachExit.position - bestEntrance.position;
            module.transform.position += offset;

            // Kényszerítse a kolliderek bounds frissítését
            foreach (var col in module.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
                col.enabled = true;
            }
            Physics.SyncTransforms();

            // Ellenőrizze az átfedést
            if (IsAreaClear(module, attachExit.root))
            {
                entranceUsed = bestEntrance;
                if (prefab == reactorRoomPrefab)
                    reactorRoomPlaced = true;
                return module;
            }
            else
            {
                Debug.LogWarning($"Clash detected for module {module.name}, destroying it.");
                Destroy(module);
                continue;
            }
        }

        entranceUsed = null;
        return null;
    }

    void CapAllOpenExitsWithDeadends()
    {
        if (deadendPrefabs == null || deadendPrefabs.Length == 0)
        {
            Debug.LogWarning("No deadend prefabs assigned; open doors will remain.");
            return;
        }

        var exitsToCap = new List<Transform>(openExits);
        openExits.Clear();

        foreach (Transform exit in exitsToCap)
        {
            GameObject cap = TryPlaceDeadend(exit);
            if (cap == null)
                Debug.LogWarning("Could not cap exit with a deadend (would clash).");
        }
    }

    GameObject TryPlaceDeadend(Transform exit)
    {
        foreach (GameObject capPrefab in deadendPrefabs)
        {
            GameObject cap = Instantiate(capPrefab);

            Transform entrance = FindChild(cap.transform, "Exit1");
            if (!entrance)
            {
                Destroy(cap);
                continue;
            }

            // csak Y axison forogjon
            Vector3 attachDir = new Vector3(-exit.forward.x, 0, -exit.forward.z).normalized;
            Vector3 entranceDir = new Vector3(entrance.forward.x, 0, entrance.forward.z).normalized;
            float angle = Vector3.SignedAngle(entranceDir, attachDir, Vector3.up);
            cap.transform.Rotate(Vector3.up, angle, Space.World);
            Vector3 offset = exit.position - entrance.position;
            cap.transform.position += offset;

            foreach (var col in cap.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
                col.enabled = true;
            }
            Physics.SyncTransforms();

            if (IsAreaClear(cap, exit.root))
                return cap;

            Debug.LogWarning($"Clash detected for deadend {cap.name}, destroying it.");
            Destroy(cap);
            continue;
        }
        return null;
    }

    // Finds a child by name (case-sensitive, works for nested children)
    Transform FindChild(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == childName) return t;
        return null;
    }

    bool IsAreaClear(GameObject module, Transform ignoreRoot)
    {
        var clashBoxes = module.GetComponentsInChildren<Collider>();
        bool clash = false;

        // minden "ClashBox" nevű kollidert gyűjtsön össze a jelenetben, kivéve az új modult, a szülőt és a játékost
        List<Collider> sceneClashBoxes = new List<Collider>();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name != "ClashBox") continue;
            if (c.transform.IsChildOf(module.transform)) continue;
            if (ignoreRoot != null && c.transform.IsChildOf(ignoreRoot)) continue;

            // Játékos colliderek ignorálása
            Transform check = c.transform;
            while (check != null)
            {
                if (check.CompareTag("Player"))
                    goto IgnoreSceneCollider;
                check = check.parent;
            }
            sceneClashBoxes.Add(c);
            IgnoreSceneCollider:;
        }

        foreach (var col in clashBoxes)
        {
            if (col.gameObject.name != "ClashBox") continue;

            foreach (var other in sceneClashBoxes)
            {
                if (col.bounds.Intersects(other.bounds))
                {
                    Debug.Log($"ClashBox detected: {col.gameObject.name} at {col.bounds.center} with {other.gameObject.name} at {other.bounds.center}");
                    clash = true;
                    break;
                }
            }
            if (clash) break;
        }

        return !clash;
    }
    GameObject TryPlaceModuleWithPrefab(Transform attachExit, out Transform entranceUsed, GameObject prefab)
{
    entranceUsed = null;
    GameObject module = Instantiate(prefab);
    Transform[] exits = module.GetComponentsInChildren<Transform>(true);
    Transform bestEntrance = null;
    float bestDot = -1f;

    foreach (Transform t in exits)
    {
        if (!t.name.StartsWith("Exit")) continue;
        float dot = Vector3.Dot(t.forward, -attachExit.forward);
        if (dot > bestDot)
        {
            bestDot = dot;
            bestEntrance = t;
        }
    }

    if (bestEntrance == null)
    {
        Destroy(module);
        return null;
    }

    // Igazítás
    Quaternion rotDelta = Quaternion.FromToRotation(bestEntrance.forward, -attachExit.forward);
    module.transform.rotation = rotDelta * module.transform.rotation;

    // Bejárat újra lekérése forgatás után
    bestEntrance = FindChild(module.transform, bestEntrance.name);
    if (!bestEntrance)
    {
        Destroy(module);
        return null;
    }

    // Mozgassa úgy, hogy a bejárat illeszkedjen az attachExit-hez
    Vector3 offset = attachExit.position - bestEntrance.position;
    module.transform.position += offset;

    // Kényszerítse a kolliderek bounds frissítését
    foreach (var col in module.GetComponentsInChildren<Collider>())
    {
        col.enabled = false;
        col.enabled = true;
    }
    Physics.SyncTransforms();

    // Ellenőrizze az átfedést
    if (IsAreaClear(module, attachExit.root))
    {
        entranceUsed = bestEntrance;
        return module;
    }
    else
    {
        Debug.LogWarning($"Clash detected for module {module.name}, destroying it.");
        Destroy(module);
        return null;
    }
}

GameObject TryPlaceModuleWithRandomPrefab(Transform attachExit, out Transform entranceUsed, GameObject[] roomPrefabs, GameObject[] hallwayPrefabs)
{
    entranceUsed = null;
    List<GameObject> candidates = new List<GameObject>();
    candidates.AddRange(roomPrefabs);
    candidates.AddRange(hallwayPrefabs);

    // Shuffle candidates randomizáláshoz
    for (int i = candidates.Count - 1; i > 0; i--)
    {
        int j = Random.Range(0, i + 1);
        var temp = candidates[i];
        candidates[i] = candidates[j];
        candidates[j] = temp;
    }

    foreach (GameObject prefab in candidates)
    {
        GameObject module = Instantiate(prefab);
        Transform[] exits = module.GetComponentsInChildren<Transform>(true);
        Transform bestEntrance = null;
        float bestDot = -1f;

        foreach (Transform t in exits)
        {
            if (!t.name.StartsWith("Exit")) continue;
            float dot = Vector3.Dot(t.forward, -attachExit.forward);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestEntrance = t;
            }
        }

        if (bestEntrance == null)
        {
            Destroy(module);
            continue;
        }

        // Igazítás
        Quaternion rotDelta = Quaternion.FromToRotation(bestEntrance.forward, -attachExit.forward);
        module.transform.rotation = rotDelta * module.transform.rotation;

        // Bejárat újra lekérése forgatás után
        bestEntrance = FindChild(module.transform, bestEntrance.name);
        if (!bestEntrance)
        {
            Destroy(module);
            continue;
        }

        // Mozgassa úgy, hogy a bejárat illeszkedjen az attachExit-hez
        Vector3 offset = attachExit.position - bestEntrance.position;
        module.transform.position += offset;

        // Kényszerítse a kolliderek bounds frissítését
        foreach (var col in module.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
            col.enabled = true;
        }
        Physics.SyncTransforms();

        // Ellenőrizze az átfedést
        if (IsAreaClear(module, attachExit.root))
        {
            entranceUsed = bestEntrance;
            return module;
        }
        else
        {
            //Debug.LogWarning($"Clash detected for module {module.name}, destroying it.");
            Destroy(module);
            continue;
        }
    }

    return null;
}
}