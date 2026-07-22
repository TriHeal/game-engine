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
    public enum ColorRole
    {
        Primary, OnPrimary, PrimaryFixed, OnPrimaryFixed,
        Secondary, OnSecondary,
        Tertiary, OnTertiary, TertiaryFixed, OnTertiaryFixed,
        Surface, OnSurface, SurfaceContainerHigh,
        Background, OnBackground,
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
        graphic.color = Resolve(theme, role);
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
            default: return Color.white;
        }
    }
}
