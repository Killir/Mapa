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

        UNITY_DECLARE_TEX2DARRAY(mainTexturesArray);
        UNITY_DECLARE_TEX2DARRAY(slopeTexturesArray);

        uint mapWidth;
        uint mapHeight;
        uint chunkWidth;
        uint chunkHeight;

        float maxHeight;
        float minHeight;

        uint hdCount;
        float hdLenghtsFloat[maxHDCount];
        float hdIncRegsFloat[maxRegCount];

        float scales[maxRegCount];
        float colorStrenghts[maxRegCount];
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

        float3 MainTriplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
            float3 scaledWorldPos = worldPos / scale;
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(mainTexturesArray, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(mainTexturesArray, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(mainTexturesArray, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
        }

        float3 SlopeTriplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
            float3 scaledWorldPos = worldPos / scale;
            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(slopeTexturesArray, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(slopeTexturesArray, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(slopeTexturesArray, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
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

            float heightPercent = InverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
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
                for (uint h = 0; h < hdLenght; h++) {
                    uint index = incReg[h];
                    float drawStrenght = InverseLerp(-regionBlendAmounts[index] - epsilon, regionBlendAmounts[index], heightPercent - heights[index]);

                    float slope = 1 - IN.worldNormal.y;
                    float blendHeight = slopeThresholds[index] * (1 - slopeBlendAmounts[index]);
                    float mainWeight = 1 - InverseLerp(blendHeight, slopeThresholds[index], slope);

                    float3 currentColor = (mainColors[index] * mainWeight + slopeColors[index] * (1 - mainWeight)) * colorStrenghts[index];
                    float3 mainTextureColor = MainTriplanar(IN.worldPos, scales[index], blendAxes, index);
                    float3 slopeTextureColor = SlopeTriplanar(IN.worldPos, scales[index], blendAxes, index);
                    float3 currentTextureColor = (mainTextureColor * mainWeight + slopeTextureColor * (1 - mainWeight)) * (1 - colorStrenghts[index]);

                    heightColor = heightColor * (1 - drawStrenght) + (currentColor + currentTextureColor) * drawStrenght;
                }
                o.Albedo = o.Albedo * (1 - humidityStrenght) + heightColor * humidityStrenght;
            }

        }
        ENDCG
    }
}
