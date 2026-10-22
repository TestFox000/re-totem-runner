Shader "Yagoda/SunLight" {
Properties {
 _LightTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
 _DustTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Transparent" }
 Pass {
  Tags { "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord0
   Bind "texcoord1", TexCoord1
  }
  ZWrite Off
  Cull Off
  Blend SrcAlpha OneMinusSrcAlpha
  SetTexture [_LightTex] { combine texture, texture alpha * primary alpha }
  SetTexture [_DustTex] { combine texture, texture alpha * previous alpha }
 }
}
}