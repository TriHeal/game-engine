using UnityEngine;

/// <summary>
/// Single source of truth for the "Serene Play" color palette (see
/// DESIGN.md). Lives as one shared asset so every scene's buttons, text,
/// and backgrounds pull from the same values instead of each scene
/// hardcoding its own color swatches.
/// </summary>
[CreateAssetMenu(fileName = "UITheme", menuName = "Tri-Heal/UI Theme")]
public class UITheme : ScriptableObject
{
    [Header("Surface")]
    public Color surface = HexColor("#f7f9ff");
    public Color surfaceDim = HexColor("#c9dcf3");
    public Color surfaceBright = HexColor("#f7f9ff");
    public Color surfaceContainerLowest = HexColor("#ffffff");
    public Color surfaceContainerLow = HexColor("#edf4ff");
    public Color surfaceContainer = HexColor("#e3efff");
    public Color surfaceContainerHigh = HexColor("#d9eaff");
    public Color surfaceContainerHighest = HexColor("#d1e4fb");
    public Color surfaceVariant = HexColor("#d1e4fb");
    public Color onSurface = HexColor("#091d2e");
    public Color onSurfaceVariant = HexColor("#3f484e");
    public Color inverseSurface = HexColor("#203243");
    public Color inverseOnSurface = HexColor("#e8f2ff");

    [Header("Outline")]
    public Color outline = HexColor("#6f787f");
    public Color outlineVariant = HexColor("#bfc8cf");

    [Header("Primary (Sky Blue)")]
    public Color surfaceTint = HexColor("#00658c");
    public Color primary = HexColor("#00658c");
    public Color onPrimary = HexColor("#ffffff");
    public Color primaryContainer = HexColor("#6dbeed");
    public Color onPrimaryContainer = HexColor("#004c6a");
    public Color inversePrimary = HexColor("#80d0ff");
    public Color primaryFixed = HexColor("#c5e7ff");
    public Color primaryFixedDim = HexColor("#80d0ff");
    public Color onPrimaryFixed = HexColor("#001e2d");
    public Color onPrimaryFixedVariant = HexColor("#004c6a");

    [Header("Secondary (Meadow Green) - positive actions / success / CTA")]
    public Color secondary = HexColor("#1d6c2a");
    public Color onSecondary = HexColor("#ffffff");
    public Color secondaryContainer = HexColor("#a4f6a2");
    public Color onSecondaryContainer = HexColor("#24732f");
    public Color secondaryFixed = HexColor("#a4f6a2");
    public Color secondaryFixedDim = HexColor("#89d989");
    public Color onSecondaryFixed = HexColor("#002105");
    public Color onSecondaryFixedVariant = HexColor("#005317");

    [Header("Tertiary (Sun Yellow) - highlights / celebration")]
    public Color tertiary = HexColor("#735c00");
    public Color onTertiary = HexColor("#ffffff");
    public Color tertiaryContainer = HexColor("#d5b243");
    public Color onTertiaryContainer = HexColor("#574500");
    public Color tertiaryFixed = HexColor("#ffe088");
    public Color tertiaryFixedDim = HexColor("#e7c353");
    public Color onTertiaryFixed = HexColor("#241a00");
    public Color onTertiaryFixedVariant = HexColor("#574500");

    [Header("Error")]
    public Color error = HexColor("#ba1a1a");
    public Color onError = HexColor("#ffffff");
    public Color errorContainer = HexColor("#ffdad6");
    public Color onErrorContainer = HexColor("#93000a");

    [Header("Background")]
    public Color background = HexColor("#f7f9ff");
    public Color onBackground = HexColor("#091d2e");

    [Header("Shape (px, matches DESIGN.md 'rounded' scale)")]
    public float radiusSm = 8f;
    public float radiusDefault = 16f;
    public float radiusMd = 24f;
    public float radiusLg = 32f;
    public float radiusXl = 48f;

    [Header("Spacing (px, matches DESIGN.md 'spacing' scale)")]
    public float spacingUnit = 8f;
    public float containerPadding = 32f;
    public float gutter = 24f;
    public float stackGapLg = 40f;
    public float stackGapMd = 24f;
    public float stackGapSm = 12f;

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var color);
        return color;
    }
}
