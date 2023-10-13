Shader"Unlit/GridShader"
{
    Properties
    {
        _Color ("MainColor", Color) = (0, 0, 0, 1)
        _GridColor ("GridColor", Color) = (1, 1, 1, 1)
        _GridSize ("GridSize", Range(0, 20)) = 1
        _LineWidth ("LineWidth", Range(0.4, 1)) = 0.1
        _Edge ("Edge", Range(0.001, 0.8)) = 0.1
        _Cutout ("Cutout Threshold", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
Cull Off // Disable backface culling
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
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
                float4 worldPos : TEXCOORD1; // Store world position
            };
 
            float4 _GridColor;
            float4 _Color;
            float _GridSize;
            float _LineWidth;
            float _Edge;
            float _Cutout;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Transform to world space
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _Color;

                float3 f = smoothstep(_LineWidth, _Edge + _LineWidth, frac(i.worldPos.xyz * _GridSize))
                                        + smoothstep(1 - _LineWidth, 1 - _Edge - _LineWidth, frac(i.worldPos.xyz * _GridSize));
                f *= step(0.001, fwidth(i.worldPos.xyz));
                float grid = max(max(f.x, f.y), f.z);
                col.rgb = col.rgb * (1 - grid) + grid * _GridColor.rgb;

                col.a = grid; // Set alpha to control transparency.

                            // Apply cutout using the _Cutout property.
                clip(col.a - _Cutout);

                return col;
}
            ENDCG
        }
    }
}
