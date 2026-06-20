using UnityEngine;
using UnityEngine.UI;
using EventSystems = UnityEngine.EventSystems;

public class BreathingExerciseController : MonoBehaviour, 
    EventSystems.IPointerDownHandler, 
    EventSystems.IPointerUpHandler, 
    EventSystems.IDragHandler
{
    public enum ExerciseState { Idle, Inhaling, Holding, Exhaling, CycleComplete }

    [Header("Core State")]
    public ExerciseState currentState = ExerciseState.Idle;
    public int currentCycle = 1;
    public const int MaxCycles = 3;

    [Header("UI & Visual Elements")]
    [Tooltip("The main orb/circle UI element that will scale.")]
    public RectTransform breathingCircle;
    // [Tooltip("The Raft controller script to pass speed and boosts to.")]
    // public RaftWakeController raftController;

    [Tooltip("SailWind controller")]
    public SailWind saildWind;

    [Header("Timing Configurations")]
    public float inhaleDuration = 4.0f;
    public float holdDuration = 1.5f;
    public float exhaleDuration = 6.0f;

    [Header("Visual Feedback Scaling")]
    public float minCircleScale = 1.0f;
    public float maxCircleScale = 2.5f;

    // Internal tracking variables
    private float currentProgress = 0.0f; // Maps 0.0 to 1.0 per state
    private bool isPointerDown = false;
    private Vector2 pointerStartPos;
    private Vector2 currentDragDelta;

    void Update()
    {
        HandleKeyboardInput();
        ProcessStateMachine();
        UpdateVisuals();
    }

    private void ProcessStateMachine()
    {
        bool isDrivingInputActive = IsInputDrivingCorrectly();

        if (!isDrivingInputActive)
        {
            // Interruption Handling: If they stop interacting or break the rule,
            // the animation pauses and decays back to the start of the current phase.
            if (currentState != ExerciseState.Idle && currentState != ExerciseState.CycleComplete)
            {
                currentProgress = Mathf.Max(0.0f, currentProgress - Time.deltaTime * 0.5f);
                if (currentProgress == 0.0f && currentState != ExerciseState.Inhaling)
                {
                    // Revert back to beginning of cycle if they completely stop
                    currentState = ExerciseState.Inhaling;
                }
            }
            return;
        }

        // Drive the exercise progress forward
        switch (currentState)
        {
            case ExerciseState.Idle:
                // Transition immediately to inhaling once touch/input begins
                currentState = ExerciseState.Inhaling;
                currentProgress = 0.0f;
                break;

            case ExerciseState.Inhaling:
                currentProgress += Time.deltaTime / inhaleDuration;
                // if (raftController != null) raftController.currentRaftSpeed = 2.0f; // Slow movement

                if (currentProgress >= 1.0f)
                {
                    currentState = ExerciseState.Holding;
                    currentProgress = 0.0f;
                }
                break;

            case ExerciseState.Holding:
                currentProgress += Time.deltaTime / holdDuration;
                
                if (currentProgress >= 1.0f)
                {
                    currentState = ExerciseState.Exhaling;
                    currentProgress = 0.0f;
                }
                break;

            case ExerciseState.Exhaling:
                currentProgress += Time.deltaTime / exhaleDuration;
                // if (raftController != null) raftController.currentRaftSpeed = 2.0f; // Keep crawling forward

                if (currentProgress >= 1.0f)
                {
                    TriggerCycleCompletion();
                }
                break;
        }
    }

    private bool IsInputDrivingCorrectly()
    {
        // 1. Keyboard checks
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (currentState == ExerciseState.Inhaling || currentState == ExerciseState.Holding) return true;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            if (currentState == ExerciseState.Exhaling) return true;
        }

        // 2. Touch/Mouse Drag checks
        if (isPointerDown)
        {
            if (currentState == ExerciseState.Holding) return true; // Just holding counts for pause phase
            
            if (currentState == ExerciseState.Inhaling && currentDragDelta.x > 10f) return true; // Dragged Right
            if (currentState == ExerciseState.Exhaling && currentDragDelta.x < -10f) return true; // Dragged Left/Back
        }

        return false;
    }

    private void UpdateVisuals()
    {
        if (breathingCircle == null) return;

        float currentScaleFactor = minCircleScale;

        switch (currentState)
        {
            case ExerciseState.Idle:
                currentScaleFactor = minCircleScale;
                break;
            case ExerciseState.Inhaling:
                currentScaleFactor = Mathf.Lerp(minCircleScale, maxCircleScale, currentProgress);
                break;
            case ExerciseState.Holding:
                currentScaleFactor = maxCircleScale;
                break;
            case ExerciseState.Exhaling:
                // Inverse interpolation: goes from max scale back down to minimum
                currentScaleFactor = Mathf.Lerp(maxCircleScale, minCircleScale, currentProgress);
                break;
        }

        breathingCircle.localScale = new Vector3(currentScaleFactor, currentScaleFactor, 1.0f);
    }

    private void TriggerCycleCompletion()
    {
        currentState = ExerciseState.CycleComplete;
        
        // Give the duck raft its forward boost!
        //if (raftController != null)
        //{
        //    raftController.currentRaftSpeed = 15.0f; // Temporary speed surge
        //}

        Debug.Log($"Cycle {currentCycle} Complete!");
        
        saildWind.SetByCycleCounter(currentCycle);

        // Handle structural count
        if (currentCycle < MaxCycles)
        {
            currentCycle++;
            ResetToNextCycle();
        }
        else
        {
            HandleExerciseFinished();
        }
    }

    private void ResetToNextCycle()
    {
        currentState = ExerciseState.Idle;
        currentProgress = 0.0f;
        // Introduce environment or progression change calls here
    }

    private void HandleExerciseFinished()
    {
        Debug.Log("Entire breathing exercise complete! Calm sea unlocked.");
        // Turn off UI interaction or fire full sequence success states
    }

    private void HandleKeyboardInput()
    {
        // Fallback option: Spacebar can act as an immediate starter for keyboard testing
        if (Input.GetKeyDown(KeyCode.Space) && currentState == ExerciseState.Idle)
        {
            currentState = ExerciseState.Inhaling;
        }
    }

    // --- Unity UI Pointer Implementation Event Hooks ---

    public void OnPointerDown(EventSystems.PointerEventData eventData)
    {
        isPointerDown = true;
        pointerStartPos = eventData.position;
        currentDragDelta = Vector2.zero;

        if (currentState == ExerciseState.Idle)
        {
            currentState = ExerciseState.Inhaling;
        }
    }

    public void OnDrag(EventSystems.PointerEventData eventData)
    {
        if (!isPointerDown) return;
        // Track displacement from initial contact point
        currentDragDelta = eventData.position - pointerStartPos;
    }

    public void OnPointerUp(EventSystems.PointerEventData eventData)
    {
        isPointerDown = false;
        currentDragDelta = Vector2.zero;

        // If they release during the Exhale phase final edge, complete it.
        // Otherwise, the state machine will see a lack of input and handle the pause decay.
        if (currentState == ExerciseState.Exhaling && currentProgress >= 0.85f)
        {
            TriggerCycleCompletion();
        }
    }
}