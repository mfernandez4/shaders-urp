Shader "USB/USB_normal_map"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // #include "HLSLSupport.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_normal : TEXCOORD1;
                float3 wNormal : TEXCOORD2;
                float4 wTangent : TEXCOORD3;
                float3 wBinormal : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalMap;
            float4 _NormalMap_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // add tiling and offset to the normal map
                o.uv_normal = TRANSFORM_TEX(v.uv, _NormalMap);
                // transformate normal and tangent to world space
                // o.wNormal = normalize(mul(unity_ObjectToWorld, float4(v.normal, 0))).xyz;
                o.wNormal = normalize(TransformObjectToWorld(v.normal));
                o.wTangent = normalize(float4( TransformWorldToObject(v.tangent.xyz), 0));
                // o.wTangent = normalize(mul(v.tangent, unity_WorldToObject));
                // calculate binormal
                o.wBinormal = normalize(cross(o.wNormal, o.wTangent.xyz) * v.tangent.w);
                return o;
            }

            float3 dxt_compression (float4 normal_map)
            {
                #if defined (UNITY_NO_DXT5nm)
                    return normalMap.rgb * 2 - 1; 
                #else
                    float3 normal_col = float3 (normal_map.a * 2 - 1, normal_map.g * 2 - 1, 0);
                    normal_col.b = sqrt(1 - (pow(normal_col.r, 2) + pow(normal_col.g, 2)) );

                    return normal_col;
                #endif
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float4 normal_map = tex2D(_NormalMap, i.uv_normal);
                // float3 normal_compressed = dxt_compression(normal_map);
                float3 normal_compressed = UnpackNormal(normal_map);
                float3x3 tbn_matrix = float3x3
                (
                    i.wTangent.xyz,
                    i.wBinormal,
                    i.wNormal
                );
                float3 normal_col = normalize(mul(normal_compressed, tbn_matrix));
                // float3 normal_col = normal_compressed;
                
                return float4(normal_col, 1);
            }
            ENDHLSL
        }
    }
}
