Shader "Nuitrack/Depth3D_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_HeightMap("Height map", 2D) = "white" {}
        _CameraPosition("Camera position", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			sampler2D _HeightMap;
			float4 _CameraPosition;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
				fixed depth = 1 - Luminance(tex2Dlod(_HeightMap, float4(v.uv, 0, 0)));

				if (depth == 1)
					depth = 0;

				float4 deltaCam = _CameraPosition - v.vertex;
				float4 shiftToCam = normalize(deltaCam) * depth;
				float4 newVertex = v.vertex + shiftToCam * length(deltaCam);

                o.vertex = UnityObjectToClipPos(newVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//return tex2D(_HeightMap, i.uv).bgra;
                return tex2D(_MainTex, i.uv).bgra;
            }
            ENDCG
        }
    }
}
