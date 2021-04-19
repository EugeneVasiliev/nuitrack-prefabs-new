Shader "Example/Normal Extrusion" 
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _HeightMap("Height map", 2D) = "white" {}
        _Amount("Extrusion Amount", Range(-1024, 1024)) = 0.5
        _CameraPosition("Camera position", Vector) = (0,0,0,0)

		 _Emission("Emission", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
            
        struct Input 
        {
            float2 uv_MainTex;
        };
         
        float _Amount;
        sampler2D _MainTex;
        sampler2D _HeightMap;
        float3 _CameraPosition;
		float _Emission;

        void vert(inout appdata_full v) 
        {
            float4 tex = tex2Dlod(_HeightMap, float4(v.texcoord.xy, 0, 0));
            float3 toCam = normalize(v.vertex - _CameraPosition) * tex.b;
            v.vertex.xyz += toCam * _Amount;
        }
         

        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Emission = o.Albedo * _Emission;
        }

        ENDCG
      }
      Fallback "Diffuse"
}