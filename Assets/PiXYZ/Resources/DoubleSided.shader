// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/DoubleSided" {

//This shader doesn't work with shadows since it's not compatible with flipped normals.
//Please disable "Receive shadows" on your Mesh renderer.
//[Optional] : Activate "Two sided" on "Cast Shadows".

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque"}
		LOD 200
		
		Cull Back

		CGPROGRAM
		// Physically based Standard lighting model
		#pragma surface surf Standard

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		uniform float4 _planeEquation;
		uniform int _cuttingOn;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _ColorRed;

		void surf(Input IN, inout SurfaceOutputStandard o) {

			fixed4 red = fixed4(1, 0, 0, 1);

			if (_cuttingOn)
				clip(dot(IN.worldPos, _planeEquation.xyz) - _planeEquation.w);
			// Albedo comes from a texture tinted by color

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			if (_cuttingOn) {
				clip(dot(IN.worldPos, _planeEquation.xyz) - _planeEquation.w);
				if (dot(IN.worldPos, _planeEquation.xyz) - _planeEquation.w < 0.004) {
					o.Albedo = red.rgb;
					o.Emission = red.rgb;
				}
			}

		}
		ENDCG

		Cull Front
		CGPROGRAM
		// Physically based Standard lighting model
		#pragma surface surf Standard 
		#pragma vertex vert
			// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		uniform float4 _planeEquation;
		uniform int _cuttingOn;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};

		//Flip normals for back faces
		void vert(inout appdata_full v)
		{
		  v.normal *= -1;

		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {

			fixed4 red = fixed4(1, 0, 0, 1);

			if(_cuttingOn)
				clip(dot(IN.worldPos, _planeEquation.xyz) - _planeEquation.w);
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;

			if (_cuttingOn) {
				clip(dot(IN.worldPos, _planeEquation.xyz) - _planeEquation.w);
				if (dot(IN.worldPos, _planeEquation.xyz) - _planeEquation.w < 0.004) {
					o.Albedo = red.rgb;
					o.Emission = red.rgb;
				}
			}
		}

		ENDCG

		//Clip all shadows from object 
		Pass{
			Name "Caster"
			Tags{ "LightMode" = "ShadowCaster" }
			Offset 1, 1

			Fog{ Mode Off }
			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				float3 wpos : TEXCOORD3;
				float2  uv : TEXCOORD1;
				//float4 pos  : POSITION;
				float3 vpos : TEXCOORD2;
			};

			struct Input {
				float3 worldPos;
			};

			uniform float4 _MainTex_ST;
			uniform float4 _planeEquation;
			uniform int _cuttingOn;
			float yBound;
			float xBound;
			v2f vert(appdata_base v)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.wpos = worldPos;
            	TRANSFER_SHADOW_CASTER(o)
				o.vpos = v.vertex.xyz;
				return o;
			}

			uniform sampler2D _MainTex;
			uniform fixed _Cutoff;
			uniform fixed4 _Color;

			float4 frag(v2f i) : SV_TARGET
			{
				if (_cuttingOn) {
					clip(dot(i.wpos, _planeEquation.xyz) - _planeEquation.w);
				}

				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG

			}	
	} 
	FallBack "Diffuse"
}
