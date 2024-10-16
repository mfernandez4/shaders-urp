Shader "USB/USB_function_ABS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RotationABS ("Rotation", Range(0, 360)) = 0
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float3 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RotationABS;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
            {
                Rotation = Rotation * (3.14159265359f / 180.f);
                UV -= Center;
                float s = sin(Rotation);
                float c = cos(Rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.5f; // Transform from [-1,1] to [-0.5, 0.5]
                rMatrix += 0.5f; // Transform from [-0.5, 0.5] to [0, 1]
                rMatrix = rMatrix * 2 - 1; // Transform from [0,1] to [-1,1]
                UV.xy = mul(UV.yx, rMatrix); // Multiply the UV by the rotation matrix
                UV += Center; // Offset the UV back to the original position 
                Out = UV;
            }

            float4 frag (v2f i) : SV_Target
            {
                // calculate the abs value of U
                float u = abs(i.uv.x - 0.5);
                float v = abs(i.uv.x - 0.5);
                float rot = _RotationABS;
                float center = 0.5;
                // generate new UV coordinates for the texture
                float2 uv = 0;

                Unity_Rotate_Degrees_float(float2(u,v), center, rot, uv);
                // sample the texture
                // float4 col = tex2D(_MainTex, float2(u,v));
                float4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDHLSL
        }
    }
}
