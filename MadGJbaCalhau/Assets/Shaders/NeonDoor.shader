Shader "Custom/NeonDoor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color1       ("Neon Colour A",   Color)  = (1, 0, 1, 1)      // magenta
        _Color2       ("Neon Colour B",   Color)  = (0, 1, 1, 1)      // cyan
        _Speed        ("Scroll Speed",    Float)  = 1.5
        _GlowIntensity("Glow Intensity",  Float)  = 3.0
        _EdgeWidth    ("Edge Width",      Float)  = 0.06
        _PulseSpeed   ("Pulse Speed",     Float)  = 2.0
        _PulseAmount  ("Pulse Amount",    Float)  = 0.3
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"
            "RenderType"      = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Lighting Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── Textures & samplers ────────────────────────────────────────
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // ── Constant buffer ────────────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color1;
                half4  _Color2;
                float  _Speed;
                float  _GlowIntensity;
                float  _EdgeWidth;
                float  _PulseSpeed;
                float  _PulseAmount;
            CBUFFER_END

            // ── Vertex ─────────────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                half4  color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                half4  color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color       = IN.color;
                return OUT;
            }

            // ── Fragment ───────────────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv   = IN.uv;
                float  t    = _Time.y;

                // ── Sample the original sprite ─────────────────────────────
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Discard fully transparent pixels (preserves sprite shape)
                clip(sprite.a - 0.01);

                // ── Edge mask: bright only near the silhouette border ──────
                // Sample neighbours to detect edges via alpha differences
                float2 texel = float2(ddx(uv.x), ddy(uv.y));
                float aU  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2( texel.x, 0)).a;
                float aD  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-texel.x, 0)).a;
                float aR  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0,  texel.y)).a;
                float aL  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(0, -texel.y)).a;
                float edgeDelta = abs(aU - aD) + abs(aR - aL);
                float edge  = smoothstep(0.0, _EdgeWidth, edgeDelta);

                // ── Scrolling colour along the edge ───────────────────────
                // Use UV position + time to create a moving stripe pattern
                float scroll    = uv.x + uv.y + t * _Speed;
                float stripe    = sin(scroll * 8.0) * 0.5 + 0.5;   // 0..1 wave
                half3 neonColor = lerp(_Color1.rgb, _Color2.rgb, stripe);

                // ── Pulse (overall brightness breathing) ──────────────────
                float pulse = 1.0 + _PulseAmount * sin(t * _PulseSpeed);

                // ── Compose ───────────────────────────────────────────────
                // Interior of sprite: slightly tinted by neon
                half3 interiorColor = sprite.rgb * lerp(half3(1,1,1), neonColor, 0.25);

                // Edge: full neon glow, boosted by intensity
                half3 edgeColor = neonColor * _GlowIntensity * pulse;

                // Blend interior vs edge
                half3 finalColor = lerp(interiorColor, edgeColor, edge);

                return half4(finalColor * IN.color.rgb, sprite.a * IN.color.a);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}

