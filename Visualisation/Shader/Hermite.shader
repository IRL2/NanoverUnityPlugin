Shader "Narupa/Hermite"
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
            
            #include "Instancing.cginc" 
            
            
            
    struct SplineSegment {
        float3 startPoint;
        float3 endPoint;
        float3 startTangent;
        float3 endTangent;
        float3 startNormal;
        float3 endNormal;
        fixed4 startColor;
        fixed4 endColor;
        float3 startScale;
        float3 endScale;
    };
    
        StructuredBuffer<SplineSegment> SplineArray;

        SplineSegment instance_spline() {
            return SplineArray[instance_id];
        }
        
        
            
            #include "Transformation.cginc"
            
            float _Radius;
            float4 _Color;
            
            void procedural_setup() {
            }
            
            float3 GetHermitePoint(float t, float3 startPoint, float3 startTangent, float3 endPoint, float3 endTangent)
        {
            return (2 * t * t * t - 3 * t * t + 1) * startPoint + (t * t * t - 2 * t * t + t) * startTangent +
                   (-2 * t * t * t + 3 * t * t) * endPoint + (t * t * t - t * t) * endTangent;
        }

        float3 GetHermiteTangent(float t, float3 startPoint, float3 startTangent, float3 endPoint, float3 endTangent)
        {
            return (6 * t * t - 6 * t) * startPoint + (3 * t * t - 4 * t + 1) * startTangent +
                   (-6 * t * t + 6 * t) * endPoint + (3 * t * t - 2 * t) * endTangent;
        }
        
        float3 GetHermiteSecondDerivative(float t, float3 startPoint, float3 startTangent, float3 endPoint, float3 endTangent)
        {
            return (12 * t - 6) * startPoint + (6 * t - 4) * startTangent +
                   (-12 * t + 6) * endPoint + (6 * t - 2) * endTangent;
        }
        
        
        float4x4 GetTransformation(float3 x, float3 y, float3 z, float3 pos) {
            float4x4 m = 0;
            m._11_21_31_41 = float4(x, 0);
            m._12_22_32_42 = float4(y, 0);
            m._13_23_33_43 = float4(z, 0);
            m._14_24_34_44 = float4(pos, 1);
           return m;
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
            
            float4x4 mat = GetTransformation(width*_Radius*normalize(right.xyz), tangent.xyz, depth*_Radius*normalize(normal.xyz), pos.xyz);
            
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
                
                
                v.vertex = mul(ObjectToWorld, v.vertex);
            
                o.color = lerp(spline.startColor, spline.endColor, bias);
                
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
