Shader "USB/USB_diffuse_shading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightInt ("Light Intensity", Range(0, 1)) = 1
        _Ambient ("Ambient Color", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal_world : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _LightInt;
            float4 _LightColor0;
            float _Ambient;

            float3 LambertShading
            (
                float3 colorRefl, // Dr
                float lightInt, // Dl
                float3 normal, // n
                float3 lightDir // l
            )
            {
                return colorRefl * lightInt * max(0, dot(normal, lightDir));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal_world = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0))).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // float3 ambient_color = UNITY_LIGHTMODEL_AMBIENT * _Ambient;
                float3 ambient_color = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) * _Ambient;

                float3 normal = i.normal_world;
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                fixed3 colorRefl = _LightColor0.rgb;
                half3 diffuse = LambertShading( colorRefl, _LightInt, normal, lightDir);
                col.rgb *= diffuse;
                col.rgb += ambient_color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
