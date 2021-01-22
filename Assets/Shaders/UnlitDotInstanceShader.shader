Shader "Unlit/UnlitDotInstanceShader"
{
    Properties
    {
        _Color ("Color", Color) = (1.0,1.0,1.0,1.0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color : COLOR;
            
            struct appdata
            {
                float4 vertex : POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct MeshProperties {
                float4x4 localTransform;
                float4x4 parentLocalToWorld;
            };

            StructuredBuffer<MeshProperties> _Properties;

            v2f vert (appdata v, uint instanceID: SV_InstanceID)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                const float4x4 worldTransform =
                    mul(_Properties[instanceID].parentLocalToWorld, _Properties[instanceID].localTransform);
                
                const float4 pos = mul(worldTransform, v.vertex);
                o.vertex = UnityObjectToClipPos(pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;
                return col;
            }
            ENDCG
        }
    }
}
