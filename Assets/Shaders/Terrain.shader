Shader "Custom/Terrain" {
    Properties{
        _HumidityMap("Humidity map", 2D) = "white" {}
        _RegionsBlend("Regions blend", Range(0, 0.5)) = 0.25
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        const static float epsilon = 1E-4;
        const static int maxHDCount = 8;
        const static int maxIncRegCount = 16;
        const static int maxRegCount = 128;

        sampler2D _HumidityMap;
        half _RegionsBlend;

        int mapWidth;
        int mapHeight;
        int chunkWidth;
        int chunkHeight;

        float maxHeight;
        float minHeight;

        int hdCount;
        float hdLenghtsFloat[maxHDCount];
        float hdIncRegsFloat[maxRegCount];

        float3 mainColors[maxRegCount];
        float3 slopeColors[maxRegCount];
        float heights[maxRegCount];
        float slopeThresholds[maxRegCount];
        float slopeBlendAmounts[maxRegCount];
        float regionBlendAmounts[maxRegCount];

        struct Input {
            float3 worldPos;
            float3 worldNormal;
        };

        float InverseLerp(float a, float b, float value) {
            return saturate((value - a) / (b - a));
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {

            int hdLenghts[maxHDCount];
            int hdIncRegs[maxRegCount];
            int incRegCount = 0;
            for (int t = 0; t < hdCount; t++) {
                hdLenghts[t] = round(hdLenghtsFloat[t]);
                incRegCount += hdLenghts[t];
            }
            for (int r = 0; r < incRegCount; r++) {
                hdIncRegs[r] = round(hdIncRegsFloat[r]);
            }

            float humidityRange = (float)1 / hdCount;
            float2 coord = float2(IN.worldPos.x / (chunkWidth * mapWidth), IN.worldPos.z / (chunkHeight * mapHeight));
            float humidityValue = tex2D(_HumidityMap, coord).r;
            //float3 humidityColor = float3(0, 0, 0);

            for (int i = 0; i < hdCount; i++) {  
                float regionBlendHeight = _RegionsBlend * InverseLerp(0, _RegionsBlend, humidityValue);
                float humidityStrenght = InverseLerp(-_RegionsBlend - epsilon, regionBlendHeight, humidityValue - (humidityRange * i));
                
                //float humidityStrenght = InverseLerp(-_RegionsBlend - epsilon, _RegionsBlend, humidityValue - (humidityRange * i));

                int hdStartIndex = 0;
                for (int j = 0; j < i; j++) {
                    hdStartIndex += hdLenghts[j];
                }

                int hdLenght = hdLenghts[i];
                int incReg[maxIncRegCount];
                for (int l = 0; l < hdLenght; l++) {
                    incReg[l] = hdIncRegs[hdStartIndex + l];
                }

                float3 heightColor = float3(0, 0, 0);
                float heightPercent = InverseLerp(minHeight, maxHeight, IN.worldPos.y);
                for (int h = 0; h < hdLenght; h++) {
                    int index = incReg[h];
                    float drawStrenght = InverseLerp(-regionBlendAmounts[index] / 2 - epsilon, regionBlendAmounts[index] / 2, heightPercent - heights[index]);

                    float slope = 1 - IN.worldNormal.y;
                    float blendHeight = slopeThresholds[index] * (1 - slopeBlendAmounts[index]);
                    float mainWeight = 1 - InverseLerp(blendHeight, slopeThresholds[index], slope);

                    float3 currentColor = mainColors[index] * mainWeight + slopeColors[index] * (1 - mainWeight);
                    heightColor = heightColor * (1 - drawStrenght) + currentColor * drawStrenght;
                }
                o.Albedo = o.Albedo * (1 - humidityStrenght) + heightColor * humidityStrenght;
            }

            //o.Albedo = humidityColor;
        }
        ENDCG
    }
}
