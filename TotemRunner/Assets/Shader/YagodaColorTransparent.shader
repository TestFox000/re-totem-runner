Shader "Yagoda/ColorTransparent" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
}
SubShader { 
 Tags { "RenderType"="Transparent" }
 Pass {
  Tags { "RenderType"="Transparent" }
  Color [_Color]
  Blend SrcAlpha OneMinusSrcAlpha
 }
}
}