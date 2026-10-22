Shader "Yagoda/Transparent" {
Properties {
 _MainTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Transparent" }
 Pass {
  Tags { "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "color", Color
   Bind "texcoord", TexCoord
  }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  SetTexture [_MainTex] { combine texture, texture alpha * primary alpha }
 }
}
}