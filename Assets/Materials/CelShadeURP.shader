Shader "Custom/CelShadeURP"
{
    Properties
    {
        _MainTex       ("Albedo",             2D)           = "white" {}
        _Color         ("Base Color",         Color)        = (1,1,1,1)

        [Header(Shadow)]
        [Toggle(_USE_SHADOW)]  _UseShadow   ("Shadow",      Float)        = 1
        _ShadowColor   ("Shadow Color",       Color)        = (0.2, 0.2, 0.3, 1)

        [Header(Cel Bands)]
        _Bands         ("Light Bands",        Range(1, 10)) = 3

        [Header(Specular)]
        [Toggle(_USE_SPEC)]    _UseSpec     ("Specular",     Float)        = 1
        _SpecColor2    ("Specular Color",     Color)        = (1,1,1,1)
        _Glossiness    ("Glossiness",         Range(1, 512)) = 64
        _SpecThreshold ("Specular Threshold", Range(0, 1))  = 0.5

        [Header(Rim Light)]
        [Toggle(_USE_RIM)]     _UseRim      ("Rim Light",    Float)        = 1
        _RimColor      ("Rim Color",         Color)         = (0.8, 0.9, 1, 1)
        _RimPower      ("Rim Power",         Range(0.1, 8)) = 3
        _RimThreshold  ("Rim Threshold",     Range(0, 1))   = 0.1

        [Header(Outline)]
        [Toggle(_USE_OUTLINE)] _UseOutline  ("Outline",      Float)        = 1
        _OutlineColor  ("Outline Color",     Color)         = (0,0,0,1)
        _OutlineWidth  ("Outline Width",     Range(0, 1)) = 0.02
        [IntRange] _StencilRef ("Stencil Ref", Range(0, 255)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "CELSHADE"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #pragma shader_feature_local _USE_SHADOW
            #pragma shader_feature_local _USE_SPEC
            #pragma shader_feature_local _USE_RIM

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float  _Bands;
                float4 _SpecColor2;
                float  _Glossiness;
                float  _SpecThreshold;
                float4 _RimColor;
                float  _RimPower;
                float  _RimThreshold;
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _UseOutline;
                float  _StencilRef;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vni = GetVertexNormalInputs(IN.normalOS);
                OUT.positionCS  = vpi.positionCS;
                OUT.positionWS  = vpi.positionWS;
                OUT.normalWS    = vni.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.shadowCoord = GetShadowCoord(vpi);
                OUT.fogFactor   = ComputeFogFactor(vpi.positionCS.z);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(GetCameraPositionWS() - IN.positionWS);

                Light  mainLight  = GetMainLight(IN.shadowCoord);
                float3 L          = normalize(mainLight.direction);
                float3 H          = normalize(L + V);
                float3 lightColor = mainLight.color;

                float NdotL = dot(N, L) * 0.5 + 0.5;

                #if defined(_USE_SHADOW)
                    float shadow  = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
                    float celDiff = floor(NdotL * shadow * _Bands) / _Bands;
                #else
                    float celDiff = floor(NdotL * _Bands) / _Bands;
                #endif

                #if defined(_USE_SPEC)
                    float spec    = pow(max(dot(N, H), 0.0), _Glossiness);
                    float celSpec = step(_SpecThreshold, spec);
                #endif

                #if defined(_USE_RIM)
                    float rim    = 1.0 - saturate(dot(V, N));
                    float celRim = step(_RimThreshold, pow(rim, _RimPower));
                #endif

                float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                float4 result;

                #if defined(_USE_SHADOW)
                    result.rgb = albedo.rgb * lerp(_ShadowColor.rgb, lightColor, celDiff);
                #else
                    result.rgb = albedo.rgb * lightColor * celDiff;
                #endif

                #if defined(_USE_SPEC)
                    result.rgb += _SpecColor2.rgb * celSpec * lightColor;
                #endif

                #if defined(_USE_RIM)
                    result.rgb += _RimColor.rgb * celRim;
                #endif

                result.rgb = MixFog(result.rgb, IN.fogFactor);
                result.a   = albedo.a;
                return result;
            }
            ENDHLSL
        }

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZTest LEqual
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Offset 1, 1

            HLSLPROGRAM
            #pragma vertex   vert_outline
            #pragma fragment frag_outline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float  _Bands;
                float4 _SpecColor2;
                float  _Glossiness;
                float  _SpecThreshold;
                float4 _RimColor;
                float  _RimPower;
                float  _RimThreshold;
                float4 _OutlineColor;
                float  _OutlineWidth;
                float  _UseOutline;
                float  _StencilRef;
            CBUFFER_END

            struct Attributes_O
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings_O
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings_O vert_outline(Attributes_O IN)
            {
                Varyings_O OUT;

                float enabled = step(0.5, _UseOutline);
                float3 normalOS = normalize(IN.normalOS);
                float3 posOS = IN.positionOS.xyz + normalOS * (_OutlineWidth * enabled);

                float3 posWS = TransformObjectToWorld(posOS);
                OUT.positionCS = TransformWorldToHClip(posWS);

                return OUT;
            }

            float4 frag_outline(Varyings_O IN) : SV_Target
            {
                clip(_UseOutline - 0.5);
                return _OutlineColor;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }

    FallBack "Universal Render Pipeline/Lit"
}