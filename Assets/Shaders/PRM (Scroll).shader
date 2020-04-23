Shader "Freedom Engine/Emissive (Scroll)"
{
    Properties
    {
        _Emission ("Albedo (RGB)", 2D) = "white" {}
        _ScrollXSpeed("X Speed", float) = 0
        _ScrollYSpeed("Y Speed", float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha One
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _Emission;

        struct Input
        {
            float2 uv_MainTex;
        };

        float _ScrollXSpeed;
        float _ScrollYSpeed;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 uv = IN.uv_MainTex;
            uv.x += _ScrollXSpeed * _Time;
            uv.y += _ScrollYSpeed * _Time;
            
            float3 emission = tex2D (_Emission, uv).rgb;

            o.Emission = emission * 1.5;
        }

        ENDCG
    }

    FallBack "Diffuse"
}
