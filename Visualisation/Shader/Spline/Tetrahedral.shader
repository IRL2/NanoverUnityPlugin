Shader "Narupa/Spline/Tetrahedral"
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
                float bias : TEXCOORD3;
            };
            
            float _Diffuse;
            
            // Normalize a vector, accounting for vectors near zero
            float2 normalizeButIgnoreZero(float2 vec) {
                float mag = sqrt(dot(vec, vec));
                if(mag < 1e-9)
                    return float2(0, 0);
                return vec / mag;
            }

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                
                // The spline segment being rendered
                SplineSegment curve = instance_spline();
                
                // The distance t along the segment is encoded in the y value of the mesh (from 0 to 1)
                float bias = v.vertex.y;
                // The vertices are projected onto the XZ plane
                v.vertex.y = 0;
                
                // Normalize the vertex relative to the origin
                v.vertex.xz = normalize(v.vertex.xz);
                
                // The center of the spline at this part of the segment
                float3 pos = GetHermitePoint(bias, curve.startPoint, curve.startTangent, curve.endPoint, curve.endTangent);
            
                
                // The tangent at this point of the curve
                float3 tangent = normalize(GetHermiteTangent(bias, curve.startPoint, curve.startTangent, curve.endPoint, curve.endTangent));
                
                // The normal to orientate the plane. To avoid twisting, we use the average of the start and end normal for every part of the segment
                float3 axis = curve.startNormal + curve.endNormal;
                axis = axis - dot(axis, tangent) * tangent;
                axis = normalize(axis);
            
                // The binormal, the third axis needed to define the reference frame for this part of the segment
                float3 binormal = -cross(tangent, axis);
                binormal = normalize(binormal);
                
                // Factor to fix x flipping
                float signX = sign(determinant(ObjectToWorld));
                binormal *= signX;
                
                // The matrix representing this reference frame
                float4x4 mat = get_transformation_matrix(-binormal.xyz, tangent.xyz, axis.xyz, pos.xyz);
                
                // The size of the normals
                float size = lerp(curve.startScale.x, curve.endScale.x, smoothstep(0, 1, bias));
                
                float3 normal = lerp(curve.startNormal, curve.endNormal, bias);
                normal = normal - dot(normal, tangent) * tangent;
                normal *= size;

                // Lerp between -start normal to end normal linearly
                float3 normal2 = lerp(-curve.startNormal, curve.endNormal, bias);
                normal2 = normal2 - dot(normal2, tangent) * tangent;
                normal2 *= size;
                
                float2 n1 = float2(dot(normal, normalize(axis)), dot(normal, normalize(binormal)));
                float2 n2 = float2(dot(normal2, normalize(axis)), dot(normal2, normalize(binormal)));
               
                float2 d1 = 0.5 * (n1 + n2);
                float2 d2 = 0.5 * (n1 - n2);
                
                float2 vNorm = normalize(v.vertex.xz);
                float2 d1Norm = normalizeButIgnoreZero(d1);
                float2 d2Norm = normalizeButIgnoreZero(d2);
      
                float dot1 = dot(vNorm, d1Norm);
                float dot2 =  dot(vNorm, d2Norm);
                
                float a1 = abs(dot1);
                float a2 = abs(dot2);
                
                a1 = 1 - (1- a1) * (1-a1);
                a2 = 1 - (1-a2) * (1-a2);
               
                v.vertex.xz = 0.5 * _Radius * v.vertex.xz;
               
                v.vertex.xz += sign(dot1) * a1 * d1 + sign(dot2) * a2 * d2;
                
                // Transform the vertex by both the reference frame and into world space
                v.vertex = mul(mat, float4(v.vertex.xyz, 1));
                v.vertex = mul(ObjectToWorld, float4(v.vertex.xyz, 1));
                
                o.normal = normalize(mul(mat, float4(v.normal.xyz, 0)));
                o.normal = normalize(mul(ObjectToWorldInverseTranspose, float4(o.normal.xyz, 0)));
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldVertex = v.vertex;
                
                o.color = lerp(pow(curve.startColor, 2.2), pow(curve.endColor, 2.2), bias);
                o.bias = bias;
                
                return o;
            }
            
            #include "Lighting.cginc"
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = i.color;
                float3 n = normalize(i.normal);
                float3 l = normalize(_WorldSpaceLightPos0.xyz);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                float d = cos((i.bias + _Time.x * 4) * 3.1415f);
                d = d * d * d * d;
                d = d * d;
                
               
                color = DIFFUSE(color, n, l, _Diffuse);
                
                 //color += 0.25 * fixed4(1,1,1,1) * d;
                
                return color;
            }
            ENDCG
        }
    }
}
