Shader "Yagoda/GuiTexture" {
Properties {
 _MainTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
}
SubShader {
 Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
 Pass {
  Lighting Off
  ZTest False
  ZWrite Off
  Cull Off
  Blend SrcAlpha OneMinusSrcAlpha

  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"

  sampler2D _MainTex;
  float4 _MainTex_ST;

  struct appdata_t {
   float4 vertex : POSITION;
   float2 texcoord : TEXCOORD0;
   float4 color : COLOR;
  };

  struct v2f {
   float4 pos : SV_POSITION;
   float2 uv : TEXCOORD0;
   float4 color : COLOR;
  };

  v2f vert (appdata_t v)
  {
   v2f o;
   o.pos = UnityObjectToClipPos(v.vertex);
   o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
   o.color = v.color;
   return o;
  }

  fixed4 frag (v2f i) : SV_Target
  {
   fixed4 c = tex2D(_MainTex, i.uv);
   c.a *= i.color.a;
   return c;
  }
  ENDCG
 }
}
}