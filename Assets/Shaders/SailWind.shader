Shader "Tri-Heal/SailWind"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _ShadeColor ("Wind Band Color", Color) = (0.45,0.45,0.5,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.1
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _WindSpeed ("Wind Speed", Float) = 3.0
        _WindFrequency ("Wind Frequency", Float) = 18.0
        _WindAmplitude ("Wind Amplitude", Float) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _ShadeColor;
        float _WindSpeed;
        float _WindFrequency;
        float _WindAmplitude;

        // Mesh geometry never moves. Instead, sharpened traveling bands sweep across the
        // sail using its world-space X position (left -> right), lerping into a distinct
        // darker "wind band" color (not just a brightness multiplier) so the bands read
        // clearly even against the mesh's own static curvature shading/specular. World
        // position is used instead of UV because the sail shares a combined mesh whose UV0
        // only covers a narrow slice, which made a UV-based band invisible. Smoothness is
        // also lowered so specular highlights don't wash the bands out.
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float phase1 = _Time.y * _WindSpeed - IN.worldPos.x * _WindFrequency;
            float phase2 = _Time.y * _WindSpeed * 1.7 - IN.worldPos.x * _WindFrequency * 2.3;
            float wave = sin(phase1) * 0.6 + sin(phase2) * 0.4;

            // Sharpen the wave into crisp bands instead of a soft sine gradient.
            float band = saturate(pow(abs(wave), 0.35)) * sign(wave);
            float t = saturate(band * _WindAmplitude + 0.5);

            o.Albedo = lerp(_ShadeColor.rgb, _Color.rgb, t);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
