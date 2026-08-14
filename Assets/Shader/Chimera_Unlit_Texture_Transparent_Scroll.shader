Shader "Chimera/Unlit_Texture_Transparent_Scroll" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0,0,0,0)
    }
    
    SubShader {
        Tags { "Queue" = "Transparent" }
        Pass {
            Tags { "Queue" = "Transparent" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZClip Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            uniform float4 _MainTex_ST;
            uniform float4 _ScrollSpeed;
            uniform float4 _Color;
            sampler2D _MainTex;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv + _Time.y * _ScrollSpeed.xy);
                col *= i.color;
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Diffuse"
}