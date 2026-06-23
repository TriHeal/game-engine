using UnityEngine;

public class ShowerEmissionController : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] showers;

    [Header("Level (stepped accumulator)")]
    [SerializeField] private float step = 20f;
    [SerializeField] private float minLevel = 0f;
    [SerializeField] private float maxLevel = 60f;

    [Header("Transition")]
    [SerializeField] private float transitionSpeed = 6f;

    private float level;
    private float[] currentRates;

    void Start()
    {
        level = minLevel;
        currentRates = new float[showers.Length];
        Debug.Log($"[Shower] Start: showers.Length={showers.Length}");
        for (int i = 0; i < showers.Length; i++)
        {
            currentRates[i] = showers[i] != null ? showers[i].emission.rateOverTime.constant : 0f;
            Debug.Log($"[Shower] showers[{i}]={(showers[i] != null ? showers[i].name : "NULL")}, initialRate={currentRates[i]}");
        }
    }

    void Update()
    {
        for (int i = 0; i < showers.Length; i++)
        {
            if (showers[i] == null) continue;

            currentRates[i] = Mathf.Lerp(currentRates[i], level, Time.deltaTime * transitionSpeed);

            var emission = showers[i].emission;
            emission.rateOverTime = currentRates[i];
        }
    }

    public void IncreaseLevel()
    {
        level = Mathf.Min(level + step, maxLevel);
        Debug.Log($"[Shower] IncreaseLevel -> level={level}");
    }

    public void DecreaseLevel()
    {
        level = Mathf.Max(level - step, minLevel);
        Debug.Log($"[Shower] DecreaseLevel -> level={level}");
    }

    public void ResetLevel()
    {
        level = minLevel;
        for (int i = 0; i < currentRates.Length; i++)
        {
            currentRates[i] = minLevel;
            if (showers[i] == null) continue;

            var emission = showers[i].emission;
            emission.rateOverTime = minLevel;
        }
        Debug.Log($"[Shower] ResetLevel -> level={level}");
    }
}
