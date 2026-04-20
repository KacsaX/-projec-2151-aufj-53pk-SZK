using UnityEngine;
using UnityEngine.AI;
// LURKER MONSTER AI
public class LurkerAI : MonoBehaviour
{
    public float sightRange = 15f;
    public float sightFOV = 90f;
    public float hearingRange = 10f;
    public float speedIdle = 1f;
    public float speedSearch = 2f;
    public float speedChase = 4f;
    public float aggressiveness = 1f; // 0 = slow to attack, 1 = instant
    public float persistenceTime = 10f;
    private float searchWaitTimer = 0f;
    private float hitCooldownTimer = 3f;

    private Transform player;

    private enum State { Idle, Searching, Investigating, Chasing, Lost, LostSight, HitCooldown }
    private State currentState = State.Idle;
    private Vector3 lastHeardPosition;
    private float lostTimer = 0f;
    private NavMeshAgent agent;
    private Vector3 currentSearchTarget;

    // Animation
    private Animator animator;
    private bool jawOpen = false;
    private bool roarTriggered = false;

    // Head movement
    public Transform headBone;
    public float headTurnSpeed = 5f;
    public float maxHeadYaw = 70f;   // degrees left/right from forward
    public float maxHeadPitch = 40f; // degrees up/down from forward

    // For slowing down when near door
    private Coroutine slowDownCoroutine;
    private float originalSpeed;

    // For using vents
    private VentPoint[] allVents;
    private float idleTimer = 0f;
    private float searchVentTimer = 0f;
    private bool isTravelingVent = false;


    void Start()
    {
        allVents = FindObjectsOfType<VentPoint>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speedIdle;
        agent.stoppingDistance = 0.1f;

        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        switch (currentState)       //AI állapotmenetek
        {
            case State.Idle:
                Debug.Log("Lurker: Idle");
                IdleBehavior();
                break;
            case State.Searching:
                Debug.Log("Lurker: Searching");
                SearchBehavior();
                break;
            case State.Investigating:
                Debug.Log("Lurker: Investigating");
                InvestigateBehavior();
                break;
            case State.Chasing:
                Debug.Log("Lurker: Chasing");
                ChaseBehavior();
                break;
            case State.Lost:
                Debug.Log("Lurker: Lost");
                LostBehavior();
                break;
            case State.LostSight:
                Debug.Log("Lurker: LostSight");
                LostSightBehavior();
                break;
            case State.HitCooldown:
                Debug.Log("Lurker: HitCooldown");
                HitCooldownBehavior();
                break;    
        }

        // Animation: blend tree sebesség paraméterek
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float moveX = Mathf.Clamp(localVelocity.x / speedSearch, -1f, 1f); // Sideways (strafe)
        float moveY = Mathf.Clamp(localVelocity.z / speedSearch, -1f, 2f); // Forward/back (run up to 2)

        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveY", moveY);

        // Animation: állkapocs nyitás a chase állapotban
        if (currentState == State.Chasing && !jawOpen)
        {
            animator.SetTrigger("JawOpen");
            jawOpen = true;
        }
        else if (currentState != State.Chasing && jawOpen)
        {
            animator.SetTrigger("JawClose");
            jawOpen = false;
        }

        // Player hit check (Player ütés ellenőrzése)
        if (currentState == State.Chasing)
        {
            var playerMovement = player.GetComponent<PlayerMovementNewInput>();
            if (playerMovement != null && playerMovement.isDead)
                return; // Ignore dead player, skip hit logic (Halott játékos figyelmen kívül hagyása, kihagyjuk a hit logikát)
            if (Vector3.Distance(transform.position, player.position) < 1.5f)
            {
                Debug.Log("Lurker hits the player!");
                PlayHitAnimation();
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    int prevHealth = playerHealth.currentHealth;
                    playerHealth.TakeDamage(50);
                    if (playerHealth.currentHealth < prevHealth)
                    {
                        hitCooldownTimer = 4f;
                        SetState(State.HitCooldown);
                    }
                }
            }
        }

        Vector3 velocity = agent.velocity;
        velocity.y = 0f;
        if (velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        if (headBone != null)
        {
            Vector3 targetDir;
            if (CanSeePlayer())
            {
                // Lock onto player (Játékos követése)
                targetDir = (player.position - headBone.position).normalized;
            }
            else
            {
                // Idle logic (Nyugalmi állapot logika)
                targetDir = transform.forward;
            }

            // Calculate desired rotation (Kívánt forgatás számítása)
            Quaternion lookRotation = Quaternion.LookRotation(targetDir, Vector3.up);

            // Convert to local rotation relative to body (Fej helyi forgatása a testhez képest)
            Quaternion localLookRotation = Quaternion.Inverse(transform.rotation) * lookRotation;

            // Clamp head yaw and pitch (Fej forgatásának korlátozása)
            Vector3 euler = localLookRotation.eulerAngles;
            if (euler.x > 180) euler.x -= 360;
            if (euler.y > 180) euler.y -= 360;
            euler.x = Mathf.Clamp(euler.x, -maxHeadPitch, maxHeadPitch);
            euler.y = Mathf.Clamp(euler.y, -maxHeadYaw, maxHeadYaw);

            Quaternion clampedLocal = Quaternion.Euler(euler);

            // Smoothly rotate head (Fej sima forgatása annak érdekében, hogy ne legyen túl hirtelen)
            headBone.localRotation = Quaternion.Slerp(headBone.localRotation, clampedLocal, Time.deltaTime * headTurnSpeed);
        }
    }

    // State transition helper (állapotváltás segítő)
    private void SetState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Idle:
                agent.speed = speedIdle;
                animator.SetFloat("Speed", 0f);
                break;
            case State.Searching:
            case State.Investigating:
            case State.Lost:
                agent.speed = speedSearch;
                animator.SetFloat("Speed", speedSearch);
                break;
            case State.Chasing:
            case State.LostSight:
                agent.speed = speedChase;
                animator.SetFloat("Speed", speedChase);
                break;
        }
    }

// DOOR SLOWDOWN // (AI Lassítása ajtó közelében)

    public void SlowDownForDoor(float slowSpeed, float duration)
    {
        Debug.Log($"LurkerAI: SlowDownForDoor called! Speed: {slowSpeed}, Duration: {duration}, Time: {Time.time}, Lurker: {name}");
        if (slowDownCoroutine != null)
            StopCoroutine(slowDownCoroutine);
        slowDownCoroutine = StartCoroutine(SlowDownRoutine(slowSpeed, duration));
    }
    private System.Collections.IEnumerator SlowDownRoutine(float slowSpeed, float duration)
    {
        if (agent == null) yield break;
        originalSpeed = agent.speed;
        agent.speed = slowSpeed;
        yield return new WaitForSeconds(duration);
        agent.speed = originalSpeed;
    }

// HIT // (Ütés animációk és logika)

    public void PlayHitAnimation()
    {
        animator.SetTrigger("Hit");
    }

    void HitCooldownBehavior()
    {
        agent.isStopped = true; // Stop moving during cooldown (Mozgás leállítása a cooldown alatt a player menekülési esélyének növelése érdekében)
        hitCooldownTimer -= Time.deltaTime;

        if (!roarTriggered)
        {
            Debug.Log("Roar trigger activated! (HitCooldownBehavior)");
            animator.SetTrigger("Roar");
            roarTriggered = true;
        }

        // If can see player, keep facing and updating last seen position (Ha látja a játékost, tartsa a tekintetet és frissítse az utolsó látott pozíciót)
        if (CanSeePlayer())
        {
            lastHeardPosition = player.position;
            agent.SetDestination(lastHeardPosition);
        }
        // If can't see player, listen for player (Ha nem látja a játékost, hallgassa a játékost)
        else if (CanHearPlayer())
        {
            lastHeardPosition = player.position;
            agent.SetDestination(lastHeardPosition);
        }
        // If can't see or hear, move to last seen location (Ha nem látja és nem hallja, menjen az utolsó látott helyre)
        else
        {
            agent.SetDestination(lastHeardPosition);
        }

        if (hitCooldownTimer <= 0f)
        {
            agent.speed = speedChase; // Resume chase speed (Chase sebesség visszaállítása)
            agent.isStopped = false;
            SetState(State.Chasing);
            roarTriggered = false; // Reset for next time (Visszaállítás a következő alkalomra)
        }
    }



///////////////////////////////////////
// BEHAVIORS // (Állapotok viselkedése)

    void IdleBehavior()
    {
        // Idle időzítő átmenet keresésre
        idleTimer += Time.deltaTime;
        if (idleTimer > 10f)
        {
            SetState(State.Searching);
            idleTimer = 0f;
        }

        // Vándorlás véletlenszerű pontokra tétlenség közben
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        if (CanHearPlayer())
        {
            lastHeardPosition = player.position;
            SetState(State.Investigating);
        }
        else if (CanSeePlayer())
        {
            SetState(State.Chasing);
        }
    }

    void SearchBehavior()
    {
        // Ha az időzítő lejár, keresse a szellőzőket a játékos keresése érdekében
        if (!IsPlayerNearby(60f))
        {
            searchVentTimer += Time.deltaTime;
        }

        // Ha az időzítő lejár és még nem utazik, kezdje el a szellőző utazást
        if (searchVentTimer > 30f && !isTravelingVent)
        {
            VentPoint nearestVent = FindNearestVent();
            if (nearestVent != null)
            {
                agent.SetDestination(nearestVent.transform.position);
                isTravelingVent = true;
            }
            searchVentTimer = 0f;
        }

        // --- Ha közel van a játékos szakítsa meg a folyamatot ---
        if (isTravelingVent && IsPlayerNearby(60f))
        {
            isTravelingVent = false;
            agent.ResetPath();
            searchVentTimer = 0f;
            return;
        }

        // A legtávolabbi szellőzőhöz utazás
        if (isTravelingVent && agent.remainingDistance < 1f)
        {
            VentPoint furthestVent = FindFurthestVent(agent.destination);
            if (furthestVent != null)
            {
                transform.position = furthestVent.transform.position;
                agent.Warp(furthestVent.transform.position);
            }
            isTravelingVent = false;
            searchVentTimer = 0f;
        }

        // Ha nincs keresési célpont, válasszon egy újat
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            currentSearchTarget = GetNextSearchPoint();
            agent.SetDestination(currentSearchTarget);
            searchWaitTimer = 2f; // Wait 2 seconds at each search point
        }

        // ha a keresési célpontnál van, "nézelődjön" egy kicsit
        if (Vector3.Distance(transform.position, currentSearchTarget) < 1f)
        {
            searchWaitTimer -= Time.deltaTime;
            if (searchWaitTimer <= 0f)
            {
                currentSearchTarget = GetNextSearchPoint();
                agent.SetDestination(currentSearchTarget);
                searchWaitTimer = 2f;
            }
        }

        if (CanHearPlayer())
        {
            lastHeardPosition = player.position;
            SetState(State.Investigating);
        }
        else if (CanSeePlayer())
        {
            SetState(State.Chasing);
        }
    }

    // Helper: random pont keresése a NavMesh-en a jelenlegi pozíció körül
    Vector3 GetNextSearchPoint()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * 10f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position;
    }

    void InvestigateBehavior()
    {
        agent.SetDestination(lastHeardPosition);

        if (CanSeePlayer())
        {
            SetState(State.Chasing);
        }
        else if (Vector3.Distance(transform.position, lastHeardPosition) < 1f)
        {
            SetState(State.Searching);
        }
    }

    void ChaseBehavior()
    {
        agent.isStopped = false;
        agent.speed = speedChase;

        // Játékos sebességének lekérése (saját számításból vagy Rigidbody-ból
        var playerMovement = player.GetComponent<PlayerMovementNewInput>();
        if (playerMovement != null && playerMovement.isDead)
        {
            SetState(State.Idle);
            return; // Halott játékos ignorálása
        }    
        Vector3 playerVelocity = playerMovement != null ? playerMovement.Velocity : Vector3.zero;

        // Intercepciós pont számítása
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        float speed = agent.speed;

        // becslés az elfogási időre
        float interceptTime = distance / (speed + playerVelocity.magnitude + 0.01f); // kicsi érték hozzáadása a nullával osztás elkerülése érdekében

        // Intercepciós pont előrejelzése
        Vector3 interceptPoint = player.position + playerVelocity * interceptTime;

        // Úti cél beállítása az intercepciós pontra
        if (Vector3.Distance(agent.destination, interceptPoint) > 0.5f)
            agent.SetDestination(interceptPoint);

        // Ha nem látja a játékost, váltson LostSight (elveszett látás) állapotba
        if (!CanSeePlayer())
        {
            SetState(State.LostSight);
            lostTimer = persistenceTime;
        }
    }

    void LostSightBehavior()
    {
        // Mozgás az utolsó ismert játékos pozícióhoz
        agent.SetDestination(lastHeardPosition);

        // Ha közel van az utolsó ismert pozícióhoz, hallgassa a játékost
        if (Vector3.Distance(transform.position, lastHeardPosition) < 1f)
        {
            lostTimer -= Time.deltaTime;

            // Ha hallja a játékost, újra üldözés
            if (CanHearPlayer())
            {
                lastHeardPosition = player.position;
                agent.SetDestination(lastHeardPosition);
                lostTimer = persistenceTime; // Időzítő visszaállítása
            }
            // Ha látja a játékost, újra üldözés
            else if (CanSeePlayer())
            {
                SetState(State.Chasing);
            }
            // Ha az időzítő lejár, váltson Searching (keresés) állapotba
            else if (lostTimer <= 0)
            {
                SetState(State.Searching);
            }
        }
        else if (CanSeePlayer())
        {
            SetState(State.Chasing);
        }
        else if (CanHearPlayer())
        {
            lastHeardPosition = player.position;
            agent.SetDestination(lastHeardPosition);
            lostTimer = persistenceTime; // Időzítő visszaállítása
        }
    }

    void LostBehavior()
    {
        // Mozgás az utolsó ismert játékos pozícióhoz
        agent.SetDestination(lastHeardPosition);

        // Ha közel van az utolsó ismert pozícióhoz, keressen egy ideig
        if (Vector3.Distance(transform.position, lastHeardPosition) < 1f)
        {
            lostTimer -= Time.deltaTime;
            if (lostTimer <= 0)
            {
                SetState(State.Searching);
            }
        }
        else if (CanSeePlayer())
        {
            SetState(State.Chasing);
            lastHeardPosition = player.position;
        }
        else if (CanHearPlayer())
        {
            lastHeardPosition = player.position;
            lostTimer = persistenceTime; // keresési idő visszaállítása
            agent.SetDestination(lastHeardPosition);
        }
    }

// END OF BEHAVIORS // 

// HELPERS //

    VentPoint FindNearestVent()
    {
        VentPoint nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var vent in allVents)
        {
            float dist = Vector3.Distance(transform.position, vent.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = vent;
            }
        }
        return nearest;
    }

    VentPoint FindFurthestVent(Vector3 fromPosition)
    {
        VentPoint furthest = null;
        float maxDist = 0f;
        foreach (var vent in allVents)
        {
            float dist = Vector3.Distance(fromPosition, vent.transform.position);
            if (dist > maxDist)
            {
                maxDist = dist;
                furthest = vent;
            }
        }
        return furthest;
    }

    bool IsPlayerNearby(float radius)
    {
        if (player == null) return false;
        var pm = player.GetComponent<PlayerMovementNewInput>();
        if (pm != null && pm.isDead) return false; // Halott játékos figyelmen kívül hagyása

        float dist = Vector3.Distance(transform.position, player.position);
        return dist < radius;
    }

    bool CanSeePlayer()
    {
        // Halott játékos figyelmen kívül hagyása
        var pm = player != null ? player.GetComponent<PlayerMovementNewInput>() : null;
        if (pm != null && pm.isDead) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle < sightFOV * 0.5f && Vector3.Distance(transform.position, player.position) < sightRange)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out hit, sightRange))
            {
                if (hit.transform == player)
                    return true;
            }
        }
        return false;
    }

    bool CanHearPlayer()
    {
        var pm = player != null ? player.GetComponent<PlayerMovementNewInput>() : null;
        if (pm == null) return false;
        if (pm.isDead) return false; // Halott játékos figyelmen kívül hagyása

        float effectiveHearingRange = 0.5f;
        switch (pm.CurrentState)
        {
            case PlayerMovementNewInput.MovementState.Running:   effectiveHearingRange = hearingRange * 2f; break;
            case PlayerMovementNewInput.MovementState.Walking:   effectiveHearingRange = hearingRange;      break;
            case PlayerMovementNewInput.MovementState.Crouching: effectiveHearingRange = hearingRange * 0.5f; break;
            case PlayerMovementNewInput.MovementState.Idle:      effectiveHearingRange = 0.5f;              break;
        }
        return Vector3.Distance(transform.position, player.position) < effectiveHearingRange;
    }
}