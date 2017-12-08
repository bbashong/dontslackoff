
Shader "Custom/PaperStackShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_MinHeight ("MinHeight", Float) = 0.5
	}
	SubShader {
            Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
//            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
		Pass
        {
            Tags {"LightMode"="ForwardBase"}
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            float _MinHeight;
            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0;
                float4 vertex : SV_POSITION;
                float4 orig_vertex : TEXCOORD1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.orig_vertex = v.vertex;
                o.orig_vertex.y -= _MinHeight;
                o.vertex = UnityObjectToClipPos(o.orig_vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;

                // the only difference from previous shader:
                // in addition to the diffuse lighting from the main light,
                // add illumination from ambient or light probes
                // ShadeSH9 function from UnityCG.cginc evaluates it,
                // using world space normal
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));
                return o;
            }
            
            sampler2D _MainTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col *= i.diff;
                if (i.orig_vertex.y < 0.0f)
                {
                    col.a = 0;
                }
                else {
                    col.a = 1;
                }
                return col;
            }
            ENDCG
        }
	}
	FallBack "Diffuse"
}
