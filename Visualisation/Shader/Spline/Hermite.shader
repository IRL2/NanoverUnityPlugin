Shader "Narupa/Spline/Hermite"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Radius ("Radius", float) = 1
        _Diffuse ("Diffuse", float) = 0.5
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
            
            #include "UnityCG.cginc"
            
            #pragma multi_compile_instancing
            
            #pragma instancing_options procedural:procedural_setup
            
            #include "../Instancing.cginc" 
            #include "Spline.cginc"
            #include "../Transformation.cginc"
            #include "../Lighting.cginc"
            
            float _Radius;
            float4 _Color;
            
            void procedural_setup() {
            }
            
        float4x4 GetHermiteMatrix(float t) {
            SplineSegment curve = instance_spline();
            
            float3 normal = lerp(curve.startNormal, curve.endNormal, t);
            
            float3 tangent = normalize(GetHermiteTangent(t, curve.startPoint, curve.startTangent, curve.endPoint, curve.endTangent));
            
            float3 pos = GetHermitePoint(t, curve.startPoint, curve.startTangent, curve.endPoint, curve.endTangent);
            
            normal = normal - dot(normal, tangent) * tangent;
            
            float3 right = -cross(normal, tangent);
            
            float width = lerp(curve.startScale.x, curve.endScale.x, t);
            float depth = lerp(curve.startScale.y, curve.endScale.y, t);
            
            float4x4 mat = get_transformation_matrix(width*_Radius*normalize(right.xyz), tangent.xyz, depth*_Radius*normalize(normal.xyz), pos.xyz);
            
            return mat;
        }

            struct appdata
            {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : TEXCOORD0;
                float4 worldVertex : TEXCOORD1;
                float4 normal : TEXCOORD2;
            };
            
            float _Diffuse;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                SplineSegment spline = instance_spline();
                
                float bias = v.vertex.y;
                float4x4 mat = GetHermiteMatrix(bias);
                v.vertex.y = 0;
                v.vertex = mul(mat, v.vertex);
                o.normal = normalize(mul(mat, float4(v.normal.xyz, 0)));
                
                float3 curveNormal = lerp(spline.startNormal, spline.endNormal, bias);
                
                v.vertex = mul(ObjectToWorld, v.vertex);
                
                float t = smoothstep(0, 1, bias);
            
                o.color = lerp(spline.startColor, spline.endColor, t);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                o.worldVertex = v.vertex;
                
                return o;
            }
            
            #include "Lighting.cginc"
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = i.color;
                float3 n = normalize(i.normal);
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                color = DIFFUSE(color, n, l, _Diffuse);
                
                return color;
            }
            ENDCG
        }
    }
}
