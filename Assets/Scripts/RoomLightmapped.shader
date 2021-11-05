Shader "Custom/RoomLightmapped"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _BumpMap ("Bump map", 2D) = "bump" {}
        _AltTex ("Lightmap", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _AltTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv2_AltTex;
            float3 tangentViewDir;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o)
            float3x3 objectToTangent = float3x3(
                v.tangent.xyz,
                cross(v.normal, v.tangent.xyz) * v.tangent.w,
                v.normal
            );
            o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed3 n = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            IN.tangentViewDir = normalize(IN.tangentViewDir);
            float height = n.z * 0.01;
            IN.uv_MainTex.xy += IN.tangentViewDir.xy * height;
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Normal = n;
            fixed4 lm = tex2D(_AltTex, IN.uv2_AltTex);
            o.Emission = lm.rgb * c.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
