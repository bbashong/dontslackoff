Shader "Custom/CameraCanvasShader" {
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_DrawTex ("DrawTex", 2D) = "zero" {} /* only use alpha value */
		_BrushTex ("BrushTex", 2D) = "zero" {} 
		_BrushColor ("BrushColor", Color) = (1, 0.137, 0.38, 0.4)
		_Glossiness ("Smoothness", Range(0, 1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _DrawTex;
		sampler2D _BrushTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_DrawTex;
			float2 uv_BrushTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _BrushColor;

		UNITY_INSTANCING_CBUFFER_START(Props)
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 dc = tex2D (_DrawTex, IN.uv_DrawTex);
			fixed4 bc = tex2D (_BrushTex, IN.uv_BrushTex);
			if (bc.a > 0) {
			    // brush itself
			    o.Albedo = bc.rgb;
			    o.Alpha = bc.a;
			}
			else if (dc.a > 0) {
			    // with brush
                o.Albedo = c.rgb * c.a + _BrushColor.rgb * _BrushColor.a;
                o.Alpha = c.a;
			}
			else {
			    // no brush
                o.Albedo = c.rgb;
                o.Alpha = c.a;
			}
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
