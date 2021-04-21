// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Nuitrack/Test"
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
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                struct v2f
                {
                    half3 objNormal : TEXCOORD0;
                    float3 coords : TEXCOORD1;
                    float2 uv : TEXCOORD2;
                    float4 pos : SV_POSITION;
                };

                float _Amount;
                sampler2D _MainTex;
                sampler2D _HeightMap;
                float3 _CameraPosition;
                float _Emission;

                // vertex shader
                // this time instead of using "appdata" struct, just spell inputs manually,
                // and instead of returning v2f struct, also just return a single output
                // float4 clip position
                v2f vert(float4 pos : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
                {
                    v2f o;
                    o.uv = uv;
                    o.objNormal = normal;
                    
                    float4 tex = tex2Dlod(_HeightMap, float4(pos.xy, 0, 0));

                    if (tex.b <= (1.0F / 256.0F))
                        tex.b = 1;

                    float3 toCam = normalize(pos - _CameraPosition) * tex.b;
                    pos.xyz += toCam * _Amount;

                    o.coords = pos.xyz;
                    o.pos = UnityObjectToClipPos(pos);

                    return o;
                }


            // pixel shader, no inputs needed
            fixed4 frag(v2f IN) : SV_Target
            {
                return tex2D(_MainTex, IN.uv).bgra; // just return it
            }
            ENDCG
        }
    }
}