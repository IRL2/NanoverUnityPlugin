// Copyright (c) Intangible Realities Laboratory. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

Shader "NarupaXR/Opaque/Hyperballs"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _EdgeScale ("Edge Scale", Float) = 1
        _Diffuse ("Diffuse", Range(0, 1)) = 0.5
        _ParticleScale ("Particle Scale", Float) = 1
        _GradientWidth ("Gradient Width", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
           
            #pragma instancing_options procedural:setup
            
            #define POSITION_ARRAY
            #define EDGE_ARRAY
            #pragma multi_compile __ SCALE_ARRAY
            #pragma multi_compile __ COLOR_ARRAY
            #pragma multi_compile __ FILTER_ARRAY
            
            // Width of the gradient
            float _GradientWidth;
            
            // The scales to apply to the particles
            float _ParticleScale;
            
            // The scales to apply to the edges
            float _EdgeScale;
            
            // Color multiplier
            float4 _Color;
            
            // Diffuse factor (0 for flat, 1 for full diffuse)
            float _Diffuse;
            
            #include "UnityCG.cginc"
            #include "../Instancing.cginc"
            #include "../Transformation.cginc"
            #include "../Intersection.cginc"
            #include "../Depth.cginc"
            #include "../Lighting.cginc"
            
            void setup() {
                float3 edgeStartPoint = edge_position(0);
                float3 edgeEndPoint = edge_position(1);
                
                // Transformation of box
                setup_isotropic_edge_transformation(edgeStartPoint, edgeEndPoint, 1);
            }
            
            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 rayOrigin : TEXCOORD0;
                float3 rayDirection : TEXCOORD1;
                float3 bondStart : TEXCOORD2;
                float3 bondDir : TEXCOORD3;
                float3 bondConst : TEXCOORD4;
            };
            
            v2f vert (appdata i)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(i);
                
                o.vertex = UnityObjectToClipPos(i.vertex);
                
                float3 v = mul(unity_ObjectToWorld, i.vertex);
                float3 c = _WorldSpaceCameraPos.xyz;
                
                o.rayOrigin = v;
                o.rayDirection = v - c;
                
                float3 p1 = edge_position(0);
                float3 p2 = edge_position(1);
                
                o.bondStart = p1;
                o.bondDir = normalize(p2 - p1);
                
                float dist = length(p2 - p1);
              
                float R1 = 0.1 * 0.1;
                float R2 = 0.1 * 0.1;
                float gamma = 0.4;
                float G = 1 + gamma * gamma;
                float U = (R1 - R2) / (2.0 * dist) + (dist  * (G - 1)) / (2.0 * G);
               
                
                o.bondConst.x = R1;
                o.bondConst.y = G;
                o.bondConst.z = U;
                
                
                return o;
            }
            
            float quad(float4 l, float4x4 m, float4 r) {
                return dot(l, mul(m, r));
            }
            
            float quad(float4 l, float4x4 m) {
                return dot(l, mul(m, l));
            }
            
            struct fout {
                fixed4 color : SV_Target;
                float depth : SV_Depth;
            };
            
            float4x4 quadratic_form(v2f i) {
                float4x4 mat = 0;
                
                float3 p = i.bondStart;
                float3 d = i.bondDir;
                float R1 = i.bondConst.x;
                float G = i.bondConst.y;
                float U = i.bondConst.z;
                
                float pdU = dot(p, d) + U;
                
                mat._11_12_13_14 = float4(1 - d.x * d.x * G, -d.x * d.y * G, -d.x * d.z * G, d.x * pdU * G - p.x);
                mat._21_22_23_24 = float4(-d.y * d.x * G, 1 - d.y * d.y * G, -d.y * d.z * G, d.y * pdU * G - p.y);
                mat._31_32_33_34 = float4(-d.z * d.x * G, -d.z * d.y * G, 1 - d.z * d.z * G, d.z * pdU * G - p.z);
                mat._41_42_43_44 = float4(d.x * pdU * G - p.x, d.y * pdU * G - p.y, d.z * pdU * G - p.z, dot(p, p) - R1 - G * pdU * pdU);
               
                return mat;
            }
            
            fout frag (v2f i)
            {
                fout o;
                o.color = fixed4(1,1,1,1);
                
                float4 p = float4(i.rayOrigin, 1);
                float4 d = float4(i.rayDirection, 0);
                
                float4x4 quad_form = quadratic_form(i);
                
                float dd = quad(d, quad_form);
                float pd = quad(p, quad_form, d);
                float pp = quad(p, quad_form);
               
                float b2a = pd / dd;
                float ca = pp / dd;
                
                float disc = b2a * b2a - ca;
                
                if(disc < 0)
                    discard;
                    
                float t = -b2a - sqrt(disc);
                
                OUTPUT_FRAG_DEPTH(o, p + d * t);
                return o;
            }
            
            ENDCG
        }
    }
}