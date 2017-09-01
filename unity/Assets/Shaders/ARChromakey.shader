// Created by Jam3 - http://www.jam3.com/

// NOTE: The basis of this shader is taken out of the Unity examples on vertex and fragment shaders
// https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html

Shader "Custom/ARChromakey" {
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskColor ("Mask Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0.0,1.0)) = 0.3
    }
    SubShader
    {
        Pass
        {
        	Tags {"Queue"="Transparent" "RenderType"="Transparent" "LightMode"="ForwardBase"}
        	ZWrite Off
        	Blend SrcAlpha OneMinusSrcAlpha
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 diff : COLOR0;
                float2 uv : TEXCOORD0;
                float2 screenCoord : TEXCOORD1;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);

                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));

                // get clip space coords and normalize to -1,1
				float4 p = o.vertex; //same as UnityObjectToClipPos(v.vertex);
				// get screen coords from clip space coords by performing the "perspective division"
				p.xy /= p.w;
				o.screenCoord = p.xy;

                return o;
            }

            sampler2D _MainTex;
            float4 _MaskColor;
            float _Threshold;
            sampler2D _TangoCamTexture;
			float4 _TangoCamTexture_ST;

            fixed4 frag (v2f i) : SV_Target
            {
            	// get the color of the pixel of the tango camera on the screen
            	fixed4 camMaskCol = tex2D(_TangoCamTexture, 0.5 + 0.5 * i.screenCoord); // map screenCoord from [-1,1] to [0,1]

            	// get the color of the main texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // texture lightness, brighten it up a bit
                // NOTE: hardcode this into the texture (photoshop)
                col.rgb *= 1.2;

                // multiply with light but not affecting alpha
                col.rgb *= i.diff.rgb;

                // use the original texture alpha but also use the mask to determine if the pixel should be shown or hidden
                // TODO: maybe we can do this in a faster way than using the expensive "length()" here
                col.a = min(col.a, 1 - step(length(camMaskCol.xyz - _MaskColor.xyz), _Threshold));
                return col;
            }
            ENDCG
        }
    }
}