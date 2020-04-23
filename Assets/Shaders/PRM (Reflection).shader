Shader "Freedom Engine/PRM (Reflection)"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _PRMTex ("PRM (RGB)", 2D) = "white" {}
        _ReflTex ("Reflection (RGB)", 2D) = "" {}
        _ReflPow ("Reflection Power", Range(0, 1)) = 1
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
        sampler2D _ReflTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        fixed _ReflPow;

        float2 SphereMap(float3 viewDir, float3 normal)
        {
            float3 r = reflect(normalize(viewDir), normalize(normal));
            r = mul((float3x3)UNITY_MATRIX_MV, r);
            r.z -= 1;
            float m = 2 * length(r);
            return r.xy / m + 0.5;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 prm = tex2D (_PRMTex, IN.uv_MainTex).rgb;
            float3 albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
            float3 reflection = tex2D (_ReflTex, SphereMap(IN.viewDir, o.Normal)).rgb;

            o.Albedo = albedo;
            o.Metallic = prm.r;
            o.Smoothness = prm.g;
            o.Occlusion = prm.b;
            o.Emission = reflection * _ReflPow;
        }

        ENDCG
    }

    FallBack "Diffuse"
}
