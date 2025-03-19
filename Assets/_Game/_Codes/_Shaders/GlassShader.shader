Shader "Custom/RealisticGlass"
{   
    /// <summary>
    //Henuz bitmedi zaman olursa devam edilecek.
    /// </summary>

    Properties
    {
        _Color("Color", Color) = (1,1,1,0.2)
        _Smoothness("Smoothness", Range(0,1)) = 0.9
        _Refraction("Refraction", Range(0,1)) = 0.1
        _Metallic("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // Transparan blending
            Cull Back
            ZWrite Off // Transparanlýk için derinlik yazýmý kapalý

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            float4 _Color;
            float _Smoothness;
            float _Refraction;
            float _Metallic;
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - TransformObjectToWorld(IN.positionOS));
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float fresnel = pow(1.0 - saturate(dot(IN.viewDirWS, IN.normalWS)), 3.0);
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 refractionOffset = IN.normalWS.xy * _Refraction;
                float4 sceneColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV + refractionOffset);
                
                float4 color = sceneColor * _Color;
                color.rgb = lerp(color.rgb, sceneColor.rgb, _Metallic);
                color.a = saturate(fresnel + _Color.a);

                return color;
            }
            ENDHLSL
        }
    }
}
