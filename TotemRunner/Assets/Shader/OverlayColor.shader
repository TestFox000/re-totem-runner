Shader "Yagoda/OverlayColor" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
}
SubShader { 
 Tags { "QUEUE"="Overlay" "RenderType"="Overlay" }
 Pass {
  Tags { "QUEUE"="Overlay" "RenderType"="Overlay" }
  Color [_Color]
 }
}
}