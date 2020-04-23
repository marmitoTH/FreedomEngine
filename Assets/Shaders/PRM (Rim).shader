Shader "Freedom Engine/PRM (Rim)"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _PRMTex ("PRM (RGB)", 2D) = "white" {}
        _FallTex ("Fall (RGB)", 2D) = "white" {}
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _PRMTex;
        sampler2D _FallTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        float _RimPower;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 prm = tex2D (_PRMTex, IN.uv_MainTex).rgb;
            float3 albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
            float3 fall = tex2D (_FallTex, IN.uv_MainTex).rgb;
            half rim = 1.0 - saturate(dot(normalize(IN.viewDir), normalize(o.Normal)));

            o.Albedo = albedo + (fall * pow (rim, _RimPower));
            o.Metallic = prm.r;
            o.Smoothness = prm.g;
            o.Occlusion = prm.b;
        }

        ENDCG
    }

    FallBack "Diffuse"
}
