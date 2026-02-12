Shader "Custom/GlassRefraction"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (0.5, 0.9, 1.0, 0.3)
        _FresnelColor ("Fresnel Color", Color) = (0.0, 1.0, 1.0, 1.0)
        _Distortion ("Distortion Strength", Range(0, 0.1)) = 0.03
        _FresnelPower ("Fresnel Power", Range(0.5, 5.0)) = 2.0
        _IOR ("Index of Refraction", Range(1.0, 2.0)) = 1.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        // Grab the camera opaque texture
        Pass
        {
            Name "GlassRefraction"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                half4 _FresnelColor;
                half _Distortion;
                half _FresnelPower;
                half _IOR;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.screenPos = ComputeScreenPos(vertexInput.positionCS);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Compute refraction direction
                float3 refractDir = refract(-viewDirWS, normalWS, 1.0 / _IOR);
                
                // Convert refraction to screen-space UV offset
                float3 refractViewSpace = TransformWorldToViewDir(refractDir);
                float2 uvOffset = refractViewSpace.xy * _Distortion;
                
                // Sample opaque texture with refraction offset
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                screenUV += uvOffset;
                
                // Clamp to prevent sampling outside screen
                screenUV = saturate(screenUV);
                
                half3 sceneColor = SampleSceneColor(screenUV);
                
                // Apply tint
                sceneColor *= _TintColor.rgb;
                
                // Fresnel effect for edge glow
                float fresnel = pow(1.0 - saturate(dot(viewDirWS, normalWS)), _FresnelPower);
                
                // Combine: refracted scene + fresnel glow
                half3 finalColor = lerp(sceneColor, _FresnelColor.rgb, fresnel * 0.6);
                
                // Alpha: more opaque at edges (fresnel), transparent in center
                half alpha = lerp(_TintColor.a, 1.0, fresnel * 0.5);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
}
