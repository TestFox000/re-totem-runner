Shader "Yagoda/TexturesBlending" {
Properties {
 _MainTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
 _BlendTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
 _Blend ("Blending Factor", Float) = 0
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  BindChannels {
   Bind "vertex", Vertex
   Bind "texcoord", TexCoord
  }
  Color [_Color]
  Cull Off
  Blend SrcAlpha OneMinusSrcAlpha
  SetTexture [_MainTex] { combine texture, texture alpha }
  SetTexture [_BlendTex] { ConstantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous, texture alpha }
 }
}
}