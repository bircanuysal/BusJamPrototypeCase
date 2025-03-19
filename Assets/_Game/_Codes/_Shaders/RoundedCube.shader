Shader "Custom/RoundedCube"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Roundness ("Roundness", Range(0,1)) = 0.2
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input
        {
            float3 worldPos;
            float3 normal;
        };

        fixed4 _Color;
        float _Smoothness;
        float _Roundness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 pos = IN.worldPos;
            
            float3 center = round(pos);
            
            float3 localPos = pos - center;
            float dist = length(localPos);
            
            float roundFactor = _Roundness * 0.5;
            float smoothEdge = smoothstep(roundFactor, roundFactor + 0.1, dist);
            
            o.Albedo = _Color.rgb * (1.0 - smoothEdge);
            o.Smoothness = _Smoothness;
            o.Metallic = 0.0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
