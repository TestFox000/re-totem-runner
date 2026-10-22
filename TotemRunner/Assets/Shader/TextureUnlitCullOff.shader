Shader "Yagoda/TextureUnlitCullOff" {
Properties {
 _MainTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
  }
  Cull Off
  SetTexture [_MainTex] { combine texture }
 }
}
}