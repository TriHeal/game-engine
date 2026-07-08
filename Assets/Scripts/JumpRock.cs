using UnityEngine;
using UnityEngine.UI;

public class JumpRock : MonoBehaviour
{
    [Header("Landing & Visuals")]
    public Transform landingPoint;
    public Transform cameraZoomPoint; // Empty GameObject placed close to this rock for camera view
    public Renderer rockRenderer;
    public ParticleSystem fogParticles; // Or a low-poly fog mesh

    [Header("Rock State")]
    [Range(0f, 100f)] public float fogPercentage = 100f;
    public string rockTitle = "מה קרה?";
    
    [Header("Question Records Data")]
    public StructureData storyData = new StructureData();

    private Material rockMaterial;

    private void Awake()
    {
        if (rockRenderer != null)
            rockMaterial = rockRenderer.material;
    }

    /// <summary>
    /// Call this when the child fills in answers to clear the fog!
    /// </summary>
    public void UpdateFogLevel(float newPercentage)
    {
        fogPercentage = Mathf.Clamp(newPercentage, 0f, 100f);

        // Update particle emission or transparency based on remaining fog
        if (fogParticles != null)
        {
            var main = fogParticles.main;
            var emission = fogParticles.emission;
            emission.rateOverTime = (fogPercentage / 100f) * 20f; // Scale fog down
            
            if (fogPercentage <= 0) fogParticles.Stop();
        }
    }

    /// <summary>
    /// Changes the rock's base material color (e.g. Purple like in your mockup)
    /// </summary>
    public void SetRockColor(Color newColor)
    {
        if (rockMaterial != null)
        {
            rockMaterial.color = newColor;
        }
    }
}

[System.Serializable]
public class StructureData
{
    public string locationText; // איפה זה היה?
    public string whoWasThere;   // מי היה שם?
    public string feelingText;   // מה הרגשתי?
    public string thoughtText;   // מה חשבתי?
    public string bodyFeeling;   // מה הרגשתי בגוף?
    public string realization;  // הבנתי משהו?
}