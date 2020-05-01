Shader "Custom/Terrain" {
    Properties{
        _MainColor("Main Color", Color) = (0,1,0,1)
        _SlopeColor("Slope Color", Color) = (1,1,1,1)
        _SlopeThreshold("Slope Threshold", Range(0,1)) = 0.5
        _BlendAmount("Blend Amount", Range(0,1)) = 0.5
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float3 worldPos;
            float3 worldNormal;
        };
        
        half _SlopeThreshold;
        half _BlendAmount;
        fixed4 _MainColor;
        fixed4 _SlopeColor;

        float maxHeight; 
        float minHeight; //эрозия влияет на высоту после выставления max и min значений в NoiseGenerator
                         //нужно заного отслеживать мин и макс значения при эрозии

        //humidity map будет передоваться в шейдер как текстура с шумом.
        //значения будут браться по uv-координатам и преобразовываться в индекс через инверсную интерполяцию как в RegionMap.Evaluate()

        float InverseLerp(float a, float b, float value) {
            return saturate((value - a) / (b - a));
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {

            float slope = 1 - IN.worldNormal.y; // slope = 0 when terrain is completely flat
            float blendHeight = _SlopeThreshold * (1 - _BlendAmount);
            float mainWeight = 1 - saturate((slope - blendHeight) / (_SlopeThreshold - blendHeight));

            float heightPercent = InverseLerp(minHeight, maxHeight, IN.worldPos.y);
            o.Albedo = (_MainColor * mainWeight + _SlopeColor * (1 - mainWeight))/* * heightPercent*/; 
        }
        ENDCG
    }
}
