using UnityEngine;

public class SailWind : MonoBehaviour
{
    [Header("Transition")]
    public float transitionSpeed = 6f;

    private float targetScaleY;
    private float targetPosY;

    private float currentScaleY;
    private float currentPosY;

    private Vector3 baseScale;
    private Vector3 basePos;

    void Start()
    {
        baseScale = transform.localScale;
        basePos = transform.localPosition;

        currentScaleY = baseScale.y;
        currentPosY = basePos.y;

        SetSlack(); // start state
    }

    void Update()
    {
            // -------------------------
            // DEBUG INPUT (TEST ONLY)
            // -------------------------
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetSlack();

            if (Input.GetKeyDown(KeyCode.Alpha2))
                SetLow();

            if (Input.GetKeyDown(KeyCode.Alpha3))
                SetMid();

            if (Input.GetKeyDown(KeyCode.Alpha4))
                SetFull();
                
        // Smooth interpolation
        currentScaleY = Mathf.Lerp(currentScaleY, targetScaleY, Time.deltaTime * transitionSpeed);
        currentPosY   = Mathf.Lerp(currentPosY, targetPosY, Time.deltaTime * transitionSpeed);

        // Apply scale
        Vector3 s = transform.localScale;
        s.y = currentScaleY;
        transform.localScale = s;

        // Apply position compensation
        Vector3 p = transform.localPosition;
        p.y = currentPosY;
        transform.localPosition = p;
    }

    // -------------------------
    // PUBLIC STATES (4 MODES)
    // -------------------------

    public void SetSlack()
    {
        SetTarget(100.00f,  0.000f);
    }

    public void SetLow()
    {
        SetTarget(133f, 0.02f);
    }

    public void SetMid()
    {
        SetTarget(166f, 0.04f);
    }

    public void SetFull()
    {
        SetTarget(200f, 0.06f);
    }

    public void SetByCycleCounter(int cycle)
    {
        switch (cycle)
        {
            case 0:
                SetSlack();
                break;
            case 1:
                SetSlack();
                break;
            case 2:
                SetLow();
                break;
            case 3:
                SetMid();
                break;
            case 4:
                SetFull();
                break;
            default:
                SetFull();
                break;
        }
    }

    // -------------------------
    // INTERNAL
    // -------------------------

    private void SetTarget(float scaleY, float posY)
    {
        targetScaleY = scaleY;
        targetPosY = posY;
    }
}