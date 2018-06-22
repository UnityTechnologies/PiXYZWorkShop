



Shader "UnityKorea/StandardSmoothnessDemo" {
	
	
	Properties {
	
	  [Header(Simple StandardShader for Properties)]
   	  [Space(20)]
	    
	    
		_Color ("Color", Color) = (1.0, 1.0, 1.0 , 1.0)
		_Bright("Brightness", Range(0, 2)) = 1
		
		
	  [Space(20)]

		_MainTex ("Albedo (RGB)", 2D) = "white" {}		

		[Space(20)]
		[Normal][NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}		
		_BumpScale("Normal Intensity", Float) = 1.0		
			
	    [Space(20)]
		[NoScaleOffset]_MetallicMap("Meatllic", 2D) = "white" {}
		_Metallic("Metallic", Range(0,1)) = 0.5

		[Space(20)]
		[NoScaleOffset]_RoughMap("Smoothness", 2D) = "white" {}
		 _Glossiness("Smoothness", Range(0,1)) = 0.5			 	
		
		[Space(20)]		
		[MaterialToggle(_Ocu_ON)] _Ocu("Occlusion Toggle", float) = 0
		[NoScaleOffset]_OCMap("Occlusion", 2D) = "white" {}
		_OccInten("Occlusion", Range(0, 1)) = 1
		
   	    [Space(30)]
		[Header(Emissive Parameter)]
		[MaterialToggle(_Emi_ON)] _Emi("Emissive Toggle", float) = 0
		_EmissiveColor("Emissive Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_EmissiveTex ("Emissive Texture(RGB)", 2D) = "white" {}
		_EmissiveInten("Emissive Inten", Range(0, 5)) = 1
		
		[Space(20)]
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Off : 2side", Float) = 2
		
		
	}
	
	
	
	SubShader {
		
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull [_Cull]
		

		CGPROGRAM
		
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0
		#pragma shader_feature _Emi_ON
		#pragma shader_feature _Ocu_ON
		

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _MetallicMap;
		sampler2D _RoughMap;		
		

		struct Input {
			
			
			float2 uv_MainTex;
	
	    };

	
	    half _Glossiness;
		half _Metallic;
		half _BumpScale;
				
		fixed4 _Color;

		half _Bright;

		
        #if _Ocu_ON
		
		half _OccInten;
		sampler2D _OCMap;

        #endif



		#if _Emi_ON
		
		sampler2D _EmissiveTex;
		fixed4 _EmissiveColor;
		half _EmissiveInten;
		
		#endif


		
		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			fixed4 c  = tex2D(_MainTex, IN.uv_MainTex) * _Color * _Bright;
			fixed3 nm = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex)); 
			fixed sg = tex2D(_MetallicMap, IN.uv_MainTex).r;
			fixed sr = tex2D(_RoughMap, IN.uv_MainTex).r;
			
		
		    o.Albedo = c.rgb;
		    
		    
			o.Normal = nm * fixed3(1, _BumpScale, 1);
		
			o.Metallic   = sg * _Metallic;
			o.Smoothness = sr * _Glossiness;
			


			#if _Ocu_ON
			
			fixed oc = tex2D(_OCMap, IN.uv_MainTex).r;
			o.Occlusion = oc * _OccInten;

			#endif

			
			#if _Emi_ON
			
			fixed3 em = tex2D(_EmissiveTex, IN.uv_MainTex);
			
			o.Emission = em * _EmissiveColor * _EmissiveInten;
			
			#endif
						
			//o.Alpha = 1;
		}
		
		ENDCG
	}
	
	FallBack "Diffuse"
	
}
