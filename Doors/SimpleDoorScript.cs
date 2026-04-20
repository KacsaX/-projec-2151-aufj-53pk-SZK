using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SimpleDoorScript : MonoBehaviour
{
    public Animator animator;
    public GameObject[] pressETexts;
    public GameObject[] pressQTexts;
    public float interactDistance = 2f;

    public float halfOpenAnimLength = 0.5f;
    public float fullOpenAnimLength = 0.5f;
    public float halfCloseAnimLength = 0.5f;
    public float fullCloseAnimLength = 0.5f;

    public enum DoorState { Closed, HalfOpened, Opened }
    public DoorState state = DoorState.Closed;

    private bool playerNearby = false;
    private bool isAnimating = false;
    private Coroutine currentAnimCoroutine;
    private string currentAnimTrigger = "";

    // AI logic
    private HashSet<Collider> lurkersInTrigger = new HashSet<Collider>();
    private bool lockedOpen => lurkersInTrigger.Count > 0;

    void Update()
    {
        // minden játékos interakció blokkolása, ha Lurker a triggerben van
        if (lockedOpen)
        {
            // nyissa ki teljesen, ha még nem az
            if (state != DoorState.Opened && !isAnimating)
                StartAnim("FullOpen", DoorState.Opened, fullOpenAnimLength);

            // Hide prompts
            SetPromptActive(pressETexts, false);
            SetPromptActive(pressQTexts, false);
            return;
        }

        playerNearby = false;
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                if (hit.collider != null && (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform)))
                {
                    playerNearby = true;
                }
            }
        }

        // Prompt logic
        SetPromptActive(pressETexts, playerNearby && !isAnimating && (state == DoorState.Closed || state == DoorState.HalfOpened));
        SetPromptActive(pressQTexts, playerNearby && !isAnimating && (state == DoorState.HalfOpened || state == DoorState.Opened));

        // Player input
        if (playerNearby && Keyboard.current != null && !isAnimating)
        {
            if (state == DoorState.Closed && Keyboard.current.eKey.wasPressedThisFrame)
            {
                StartAnim("HalfOpen", DoorState.HalfOpened, halfOpenAnimLength);
            }
            else if (state == DoorState.HalfOpened)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartAnim("FullOpen", DoorState.Opened, fullOpenAnimLength);
                }
                else if (Keyboard.current.qKey.wasPressedThisFrame)
                {
                    StartAnim("HalfClose", DoorState.Closed, halfCloseAnimLength);
                }
            }
            else if (state == DoorState.Opened && Keyboard.current.qKey.wasPressedThisFrame)
            {
                StartAnim("FullClose", DoorState.Closed, fullCloseAnimLength);
            }
        }
    }

    private void SetPromptActive(GameObject[] prompts, bool active)
    {
        if (prompts == null) return;
        foreach (var txt in prompts)
            if (txt != null) txt.SetActive(active);
    }

    private void StartAnim(string trigger, DoorState nextState, float animLength)
    {
        if (currentAnimCoroutine != null)
            StopCoroutine(currentAnimCoroutine);

        currentAnimCoroutine = StartCoroutine(PlayAnim(trigger, nextState, animLength));
        currentAnimTrigger = trigger;
    }

    private System.Collections.IEnumerator PlayAnim(string trigger, DoorState nextState, float animLength)
    {
        isAnimating = true;
        animator.SetTrigger(trigger);

        yield return new WaitForSeconds(animLength);

        state = nextState;
        isAnimating = false;
        currentAnimTrigger = "";
        currentAnimCoroutine = null;
    }

void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Lurker"))
    {
        lurkersInTrigger.Add(other);

        // Lassítsa le és nyissa ki, ha az ajtó nincs teljesen nyitva VAGY animálódik
        if (state != DoorState.Opened || (isAnimating && currentAnimTrigger == "FullClose"))
        {
            StartAnim("FullOpen", DoorState.Opened, fullOpenAnimLength);

            LurkerAI lurker = other.GetComponent<LurkerAI>();
            if (lurker != null)
            {
                lurker.SlowDownForDoor(1f, 1.5f);
            }
        }
    }
}

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Lurker"))
        {
            lurkersInTrigger.Remove(other);
        }
    }
}