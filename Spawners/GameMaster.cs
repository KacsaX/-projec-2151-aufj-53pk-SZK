using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameMaster : MonoBehaviour
{
    public MapGenerator mapGenerator;
    public DoorSpawner doorSpawner;
    public ReactorSpawner reactorSpawner;
    public ObjectiveSpawner objectiveSpawner;
    public NavMeshSurface navMeshSurface;
    public Transform mapRoot;
    public MonsterSpawner monsterSpawner;
    public VentSpawner ventSpawner;


    // player tracking / auto-quit
    public float quitDelay = 5f;                      // másodpercekben, hogy a játékosok meghalása után mennyi idővel lépjen ki a játék
    private List<PlayerHealth> trackedPlayers = new List<PlayerHealth>();
    private bool quitSequenceStarted = false;

    void Start()
    {
        StartCoroutine(GameSetupSequence());

        // regisztrálja az összes már jelenlévő játékost a jelenetben (hasznos teszteléshez)
        var players = FindObjectsOfType<PlayerHealth>();
        foreach (var p in players)
            RegisterPlayer(p);
    }

    private System.Collections.IEnumerator GameSetupSequence()
    {
        // 1. Térkép generálása
        if (mapGenerator != null)
            mapGenerator.GenerateMap();

        // 2. Mozgassa a szobákat a gyökérhez (annak érdekében hogy a navmesh generálás megtörténhessen)
        GameObject[] modules = GameObject.FindGameObjectsWithTag("Room");
        foreach (var module in modules)
            module.transform.SetParent(mapRoot, true);

        // 3. Helyezze el az ajtókat
        if (doorSpawner != null)
        {
            yield return null; // Várjon egy keretet a transzformok/kolliderek frissítésére
            doorSpawner.PlaceDoors();
        }

        // 4. Helyezze el a reaktort és a célokat
        if (reactorSpawner != null)
        {
            yield return null; // Várjon egy keretet az ajtók elhelyezésére
            reactorSpawner.SpawnReactorAndPanels();

            ReactorControlLogic controlLogic = FindObjectOfType<ReactorControlLogic>();
            if (controlLogic != null)
            {
                controlLogic.ReactorPanelsSpawn();
                controlLogic.UpdatePanelDisplays();
                float stability = controlLogic.CalculateStability();
                controlLogic.UpdateBalanceDisplay(stability);
            }
            if (objectiveSpawner != null)
                objectiveSpawner.SpawnObjectives();
        }
        // Eredetileg reflekciók használata is tervezve volt, és működőképes is volt, viszont nem került használatra. Hasznos lehet fejleszéshez és immerzió növeléséhez későbbiekben.
        // 4.5 Setup Reflections
        //if (reflectionSetup != null)
        //    reflectionSetup.ReflectionSetup();

        // 5. Call VentSpawner
        if (ventSpawner != null)
            ventSpawner.SpawnVents();

        // 6. Bake NavMesh
        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();

        // 7. Spawn monster
        if (monsterSpawner != null)
            monsterSpawner.SpawnMonster();
    }

    // -----------------------
    // Játékos követési módszerek
    // -----------------------
    public void RegisterPlayer(PlayerHealth p)
    {
        if (p != null && !trackedPlayers.Contains(p))
            trackedPlayers.Add(p);
    }

    public void UnregisterPlayer(PlayerHealth p)
    {
        if (p != null)
            trackedPlayers.Remove(p);
    }

    // Meghívja a PlayerHealth, amikor a játékos meghal
    public void NotifyPlayerDied(PlayerHealth p)
    {
        CheckAllPlayersDead();
    }

    private void CheckAllPlayersDead()
    {
        // ha bármelyik követett játékos él, ne csináljon semmit
        foreach (var p in trackedPlayers)
        {
            if (p != null && !p.IsDead)
                return;
        }

        // minden játékos meghalt -> indítsa el a kilépési szekvenciát
        if (!quitSequenceStarted)
        {
            quitSequenceStarted = true;
            StartCoroutine(QuitAfterDelay());
        }
    }

    private IEnumerator QuitAfterDelay()
    {
        yield return new WaitForSecondsRealtime(quitDelay);

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}