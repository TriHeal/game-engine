using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds a UI Graphic (Image, TextMeshProUGUI, ...) to one named color role in
/// a shared UITheme asset, so editing the theme asset re-colors every element
/// that references it instead of each scene keeping its own hardcoded hex.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Graphic))]
public class ThemedGraphic : MonoBehaviour
{
    // Appended-only: existing entries must keep their original ordinal position,
    // since Unity serializes this enum as a plain int. Reordering or inserting
    // in the middle would silently reassign the role on every already-wired
    // ThemedGraphic component in the project. Add new roles at the end only.
    public enum ColorRole
    {
        Primary, OnPrimary, PrimaryFixed, OnPrimaryFixed,
        Secondary, OnSecondary,
        Tertiary, OnTertiary, TertiaryFixed, OnTertiaryFixed,
        Surface, OnSurface, SurfaceContainerHigh,
        Background, OnBackground,

        // -- appended --
        SurfaceDim, SurfaceBright,
        SurfaceContainerLowest, SurfaceContainerLow, SurfaceContainer, SurfaceContainerHighest,
        SurfaceVariant, OnSurfaceVariant,
        InverseSurface, InverseOnSurface,
        Outline, OutlineVariant,
        SurfaceTint,
        PrimaryContainer, OnPrimaryContainer, InversePrimary, PrimaryFixedDim, OnPrimaryFixedVariant,
        SecondaryContainer, OnSecondaryContainer, SecondaryFixed, SecondaryFixedDim, OnSecondaryFixed, OnSecondaryFixedVariant,
        TertiaryContainer, OnTertiaryContainer, TertiaryFixedDim, OnTertiaryFixedVariant,
        Error, OnError, ErrorContainer, OnErrorContainer,
    }

    public UITheme theme;
    public ColorRole role;

    private Graphic graphic;

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    public void Apply()
    {
        if (theme == null) return;
        if (graphic == null) graphic = GetComponent<Graphic>();
        if (graphic == null) return;

        // Preserve whatever alpha is already on the Graphic (e.g. a translucent
        // scrim) instead of overwriting it with the theme color's own alpha,
        // which is always 1 (hex colors parse fully opaque).
        Color resolved = Resolve(theme, role);
        Color current = graphic.color;
        graphic.color = new Color(resolved.r, resolved.g, resolved.b, current.a);
    }

    private static Color Resolve(UITheme theme, ColorRole role)
    {
        switch (role)
        {
            case ColorRole.Primary: return theme.primary;
            case ColorRole.OnPrimary: return theme.onPrimary;
            case ColorRole.PrimaryFixed: return theme.primaryFixed;
            case ColorRole.OnPrimaryFixed: return theme.onPrimaryFixed;
            case ColorRole.Secondary: return theme.secondary;
            case ColorRole.OnSecondary: return theme.onSecondary;
            case ColorRole.Tertiary: return theme.tertiary;
            case ColorRole.OnTertiary: return theme.onTertiary;
            case ColorRole.TertiaryFixed: return theme.tertiaryFixed;
            case ColorRole.OnTertiaryFixed: return theme.onTertiaryFixed;
            case ColorRole.Surface: return theme.surface;
            case ColorRole.OnSurface: return theme.onSurface;
            case ColorRole.SurfaceContainerHigh: return theme.surfaceContainerHigh;
            case ColorRole.Background: return theme.background;
            case ColorRole.OnBackground: return theme.onBackground;

            case ColorRole.SurfaceDim: return theme.surfaceDim;
            case ColorRole.SurfaceBright: return theme.surfaceBright;
            case ColorRole.SurfaceContainerLowest: return theme.surfaceContainerLowest;
            case ColorRole.SurfaceContainerLow: return theme.surfaceContainerLow;
            case ColorRole.SurfaceContainer: return theme.surfaceContainer;
            case ColorRole.SurfaceContainerHighest: return theme.surfaceContainerHighest;
            case ColorRole.SurfaceVariant: return theme.surfaceVariant;
            case ColorRole.OnSurfaceVariant: return theme.onSurfaceVariant;
            case ColorRole.InverseSurface: return theme.inverseSurface;
            case ColorRole.InverseOnSurface: return theme.inverseOnSurface;
            case ColorRole.Outline: return theme.outline;
            case ColorRole.OutlineVariant: return theme.outlineVariant;
            case ColorRole.SurfaceTint: return theme.surfaceTint;
            case ColorRole.PrimaryContainer: return theme.primaryContainer;
            case ColorRole.OnPrimaryContainer: return theme.onPrimaryContainer;
            case ColorRole.InversePrimary: return theme.inversePrimary;
            case ColorRole.PrimaryFixedDim: return theme.primaryFixedDim;
            case ColorRole.OnPrimaryFixedVariant: return theme.onPrimaryFixedVariant;
            case ColorRole.SecondaryContainer: return theme.secondaryContainer;
            case ColorRole.OnSecondaryContainer: return theme.onSecondaryContainer;
            case ColorRole.SecondaryFixed: return theme.secondaryFixed;
            case ColorRole.SecondaryFixedDim: return theme.secondaryFixedDim;
            case ColorRole.OnSecondaryFixed: return theme.onSecondaryFixed;
            case ColorRole.OnSecondaryFixedVariant: return theme.onSecondaryFixedVariant;
            case ColorRole.TertiaryContainer: return theme.tertiaryContainer;
            case ColorRole.OnTertiaryContainer: return theme.onTertiaryContainer;
            case ColorRole.TertiaryFixedDim: return theme.tertiaryFixedDim;
            case ColorRole.OnTertiaryFixedVariant: return theme.onTertiaryFixedVariant;
            case ColorRole.Error: return theme.error;
            case ColorRole.OnError: return theme.onError;
            case ColorRole.ErrorContainer: return theme.errorContainer;
            case ColorRole.OnErrorContainer: return theme.onErrorContainer;

            default: return Color.white;
        }
    }
}
