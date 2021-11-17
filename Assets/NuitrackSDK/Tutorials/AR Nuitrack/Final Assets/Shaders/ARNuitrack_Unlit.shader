Shader "NuitrackSDK/Tutorials/ARNuitrack/ARNuitrack_Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DepthTex ("Texture", 2D) = "white" {}
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
            sampler2D _DepthTex;

			float4 _CameraPosition;
            float4 _MainTex_ST;
            float4 _DepthTex_ST;

            float _Clamp;

            v2f vert (appdata v)
            {
                v2f o;

                float2 coord = float2(v.uv.x, 1 - v.uv.y);

                float depth = 1 - tex2Dlod(_DepthTex, float4(coord, 0, 0)).r;

				if (depth == 1)
					depth = 0;

				float4 deltaCam = _CameraPosition - v.vertex;
				float4 shiftToCam = normalize(deltaCam) * depth;
				float4 newVertex = v.vertex + shiftToCam * length(deltaCam);

                o.vertex = UnityObjectToClipPos(newVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, float2(i.uv.x, 1 - i.uv.y));
            }
            ENDCG
        }
    }
}
