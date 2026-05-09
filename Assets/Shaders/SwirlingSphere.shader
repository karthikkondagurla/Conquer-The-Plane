Shader "Custom/SwirlingSphere"
{
    Properties
    {
        _BaseColor      ("Base Color",          Color)          = (0.02, 0.04, 0.06, 1)
        _SwirlColorA    ("Swirl Color A",       Color)          = (0.0,  1.0,  0.55, 1)
        _SwirlColorB    ("Swirl Color B",       Color)          = (0.0,  0.4,  1.0,  1)
        _SwirlScale     ("Swirl Scale",         Float)          = 5.0
        _SwirlSpeed     ("Swirl Speed",         Float)          = 0.25
        _SwirlWidth     ("Swirl Line Width",    Range(0.01, 0.5)) = 0.18
        _SwirlSharpness ("Swirl Sharpness",     Range(1, 20))   = 8.0
        _EmissionPower  ("Emission Power",      Float)          = 3.5
        _IridPower      ("Iridescence Power",   Range(0.5, 5))  = 2.0
        _IridStrength   ("Iridescence Strength",Range(0, 2))    = 1.0
        _Metallic       ("Metallic",            Range(0,1))     = 0.85
        _Smoothness     ("Smoothness",          Range(0,1))     = 0.92
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ─────────────────────────────────────────────
            // Structs
            // ─────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 positionOS  : TEXCOORD1;   // object-space pos for swirl UV
                float3 normalWS    : TEXCOORD2;
                float3 viewDirWS   : TEXCOORD3;
            };

            // ─────────────────────────────────────────────
            // CBuffer
            // ─────────────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                half4  _BaseColor;
                half4  _SwirlColorA;
                half4  _SwirlColorB;
                float  _SwirlScale;
                float  _SwirlSpeed;
                float  _SwirlWidth;
                float  _SwirlSharpness;
                float  _EmissionPower;
                float  _IridPower;
                float  _IridStrength;
                float  _Metallic;
                float  _Smoothness;
            CBUFFER_END

            // ─────────────────────────────────────────────
            // Helpers
            // ─────────────────────────────────────────────

            // 2D hash for pseudo-noise
            float2 hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // Gradient noise (smooth)
            float gnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(dot(hash2(i + float2(0,0)), f - float2(0,0)),
                         dot(hash2(i + float2(1,0)), f - float2(1,0)), u.x),
                    lerp(dot(hash2(i + float2(0,1)), f - float2(0,1)),
                         dot(hash2(i + float2(1,1)), f - float2(1,1)), u.x),
                    u.y);
            }

            // Domain-warped swirl — 3 passes of warping create the organic curl
            float SwirlPattern(float2 uv, float time)
            {
                float2 p = uv;

                // Pass 1: warp domain
                float2 q = float2(
                    gnoise(p + float2(0.0, 0.0) + time * 0.10),
                    gnoise(p + float2(5.2, 1.3) + time * 0.08)
                );

                // Pass 2: warp again using q
                float2 r = float2(
                    gnoise(p + 4.0 * q + float2(1.7, 9.2) + time * 0.07),
                    gnoise(p + 4.0 * q + float2(8.3, 2.8) + time * 0.05)
                );

                // Final sample
                float n = gnoise(p + 4.0 * r + time * 0.06);
                return n * 0.5 + 0.5; // remap to [0,1]
            }

            // Iridescent color from view-normal angle
            half3 Iridescence(float NdotV, float strength)
            {
                // Cycle hue along a rainbow using sine waves offset by 120°
                float t = pow(1.0 - saturate(NdotV), _IridPower) * 6.2832;
                half3 col;
                col.r = sin(t + 0.0)   * 0.5 + 0.5;
                col.g = sin(t + 2.094) * 0.5 + 0.5;  // +120°
                col.b = sin(t + 4.189) * 0.5 + 0.5;  // +240°
                return col * strength;
            }

            // ─────────────────────────────────────────────
            // Vertex
            // ─────────────────────────────────────────────
            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vpi = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs   vni = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vpi.positionCS;
                output.positionWS = vpi.positionWS;
                output.positionOS = input.positionOS.xyz;
                output.normalWS   = vni.normalWS;
                output.viewDirWS  = GetWorldSpaceNormalizeViewDir(vpi.positionWS);
                return output;
            }

            // ─────────────────────────────────────────────
            // Fragment
            // ─────────────────────────────────────────────
            half4 frag(Varyings input) : SV_Target
            {
                float3 N = normalize(input.normalWS);
                float3 V = normalize(input.viewDirWS);
                float  NdotV = saturate(dot(N, V));

                // ── Swirl UV from object-space sphere normals ──
                // Map sphere normal to a 2D plane (like a lat-long projection)
                float3 osN = normalize(input.positionOS); // on unit sphere
                float2 swirlUV = float2(
                    atan2(osN.z, osN.x) / (2.0 * 3.14159) + 0.5,
                    asin(osN.y)          /        3.14159  + 0.5
                ) * _SwirlScale;

                float t = _Time.y * _SwirlSpeed;

                // ── Two overlapping swirl layers for complexity ──
                float s1 = SwirlPattern(swirlUV,              t);
                float s2 = SwirlPattern(swirlUV * 1.7 + 3.1, -t * 0.7);
                float swirl = (s1 + s2 * 0.6) / 1.6; // blend

                // ── Convert density to sharp glowing lines ──
                // Lines appear where swirl is near 0.5 (mid-band)
                float lineMask = 1.0 - abs(swirl - 0.5) / _SwirlWidth;
                lineMask = saturate(lineMask);
                lineMask = pow(lineMask, _SwirlSharpness);

                // ── Swirl color: lerp between two neon tones ──
                half3 swirlColor = lerp(_SwirlColorA.rgb, _SwirlColorB.rgb, s2 * 0.5 + 0.5);

                // ── Iridescence ──
                half3 irid = Iridescence(NdotV, _IridStrength);

                // ── Base dark surface ──
                half3 baseCol = _BaseColor.rgb;

                // ── Combine base + iridescence + swirl lines ──
                half3 surfaceColor = baseCol + irid * (1.0 - lineMask * 0.5);
                surfaceColor = lerp(surfaceColor, swirlColor * 0.3, lineMask * 0.4);

                // ── Emission: swirl lines glow bright ──
                half3 emission = swirlColor * lineMask * _EmissionPower;
                // Secondary dim glow in recessed areas for depth
                emission += _SwirlColorB.rgb * (1.0 - lineMask) * 0.08;

                // ── Basic lighting (main light) ──
                Light mainLight = GetMainLight();
                float NdotL     = saturate(dot(N, mainLight.direction));
                half3 diffuse   = surfaceColor * mainLight.color * NdotL * (1.0 - _Metallic);

                // ── Specular (GGX approximation) ──
                float3 H         = normalize(mainLight.direction + V);
                float NdotH      = saturate(dot(N, H));
                float roughness  = 1.0 - _Smoothness;
                float alpha      = roughness * roughness;
                float D          = alpha * alpha / (3.14159 * pow(NdotH * NdotH * (alpha * alpha - 1.0) + 1.0, 2.0));
                half3 specular   = mainLight.color * D * NdotL * _Metallic;

                // ── Ambient / IBL approximation ──
                half3 ambient    = surfaceColor * half3(0.08, 0.12, 0.15);

                half3 finalColor = ambient + diffuse + specular + emission;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster pass so the ball casts shadows
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex   ShadowVert
            #pragma fragment ShadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
