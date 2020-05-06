Shader "Custom/Terrain" {
    Properties{
        _HumidityMap("Humidity map", 2D) = "white" {}
        _BiomsBlend("Bioms blend", Range(0, 0.5)) = 0.25
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
        half _BiomsBlend;

        uint mapWidth;
        uint mapHeight;
        uint chunkWidth;
        uint chunkHeight;

        float maxHeight;
        float minHeight;

        uint hdCount;
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

            uint hdLenghts[maxHDCount];
            uint hdIncRegs[maxRegCount];
            uint incRegCount = 0;
            for (uint t = 0; t < hdCount; t++) {
                hdLenghts[t] = round(hdLenghtsFloat[t]);
                incRegCount += hdLenghts[t];
            }
            for (uint r = 0; r < incRegCount; r++) {
                hdIncRegs[r] = round(hdIncRegsFloat[r]);
            }

            float humidityRange = (float)1 / hdCount;
            float2 coord = float2(IN.worldPos.x / (chunkWidth * mapWidth), IN.worldPos.z / (chunkHeight * mapHeight));
            float humidityValue = tex2D(_HumidityMap, coord).r;

            for (uint i = 0; i < hdCount; i++) {  
                float biomBlendHeight = _BiomsBlend * InverseLerp(0, _BiomsBlend, humidityValue);
                float humidityStrenght = InverseLerp(-_BiomsBlend - epsilon, biomBlendHeight, humidityValue - (humidityRange * i));

                uint hdStartIndex = 0;
                for (uint j = 0; j < i; j++) {
                    hdStartIndex += hdLenghts[j];
                }

                uint hdLenght = hdLenghts[i];
                uint incReg[maxIncRegCount];
                for (uint l = 0; l < hdLenght; l++) {
                    incReg[l] = hdIncRegs[hdStartIndex + l];
                }

                float3 heightColor = float3(0, 0, 0);
                float heightPercent = InverseLerp(minHeight, maxHeight, IN.worldPos.y);
                for (uint h = 0; h < hdLenght; h++) {
                    uint index = incReg[h];
                    float drawStrenght = InverseLerp(-regionBlendAmounts[index] - epsilon, regionBlendAmounts[index], heightPercent - heights[index]);

                    float slope = 1 - IN.worldNormal.y;
                    float blendHeight = slopeThresholds[index] * (1 - slopeBlendAmounts[index]);
                    float mainWeight = 1 - InverseLerp(blendHeight, slopeThresholds[index], slope);

                    float3 currentColor = mainColors[index] * mainWeight + slopeColors[index] * (1 - mainWeight);
                    heightColor = heightColor * (1 - drawStrenght) + currentColor * drawStrenght;
                }
                o.Albedo = o.Albedo * (1 - humidityStrenght) + heightColor * humidityStrenght;
            }

        }
        ENDCG
    }
}
