﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    public bool debug = false;
    public int iterationCount;
    public int particleLifetime;
    [Space(10)]

    [Range(0f, 1f)]
    public float depositionRate;
    [Range(0f, 1f)]
    public float evaporationRate;

    public float startVolume;

    public float[,] Erode(int seed, float[,] heightMap, int heightMapIndex)
    {
        System.Random prng = new System.Random(seed);
        int width = heightMap.GetLength(0) - 1;
        int height = heightMap.GetLength(1) - 1;
        float minHeightValue = float.MaxValue;
        float maxHeightValue = float.MinValue;

        for (int i = 0; i < iterationCount; i++) {
            Vector2 position = new Vector2(prng.Next(1, width - 1), prng.Next(1, height - 1));
            Particle particle = new Particle(position, startVolume);

            for (int l = 0; l < particleLifetime; l++) {
                Vector2 currentPosition = particle.GetPosition();
                int curX = (int)currentPosition.x;
                int curY = (int)currentPosition.y;
                float currentHeight = heightMap[curX, curY];

                Vector2 gradient = CalculatePointGradient(heightMap, currentPosition.x, currentPosition.y);
                Vector2 speed = particle.GetSpeed() + gradient / particle.GetVolume();
                if (speed.magnitude == 0) {
                    break;
                }
                Vector2 nextPosition = currentPosition + speed;

                bool outOfHeightMap = nextPosition.x < 1 || nextPosition.x >= width || nextPosition.y < 1 || nextPosition.y >= height;
                if (outOfHeightMap) {
                    break;
                }

                float nextHeight = heightMap[(int)nextPosition.x, (int)nextPosition.y];
                float heightDiff = currentHeight - nextHeight;
                float capasity = particle.GetVolume() * speed.magnitude * heightDiff;
                if (capasity < 0) {
                    capasity = 0f;
                }
                float capasityDiff = capasity - particle.GetSediment() * depositionRate;
                heightMap[curX, curY] -= capasityDiff;

                if (heightMap[curX, curY] > maxHeightValue) {
                    maxHeightValue = heightMap[curX, curY];
                }
                if (heightMap[curX, curY] < minHeightValue) {
                    minHeightValue = heightMap[curX, curY];
                }

                particle.SetSediment(particle.GetSediment() + capasityDiff);
                particle.SetSpeed(speed);
                particle.SetPosition(nextPosition);
                particle.SetVolume(particle.GetVolume() * (1 - evaporationRate));

                if (debug) {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    float sphereHeight = heightMap[(int)currentPosition.x, (int)currentPosition.y] * FindObjectOfType<MapGenerator>().heightMultiplier;
                    sphere.transform.position = new Vector3(currentPosition.x, sphereHeight, currentPosition.y);
                    sphere.transform.localScale = Vector3.one * particle.GetVolume() * 0.4f;
                    sphere.GetComponent<Renderer>().sharedMaterial.color = Color.red;
                }
            }

        }

        NoiseGenerator.SetMaxValue(heightMapIndex, maxHeightValue);
        NoiseGenerator.SetMinValue(heightMapIndex, minHeightValue);
        return heightMap;
    }

    Vector2 CalculatePointGradient(float[,] heightMap, float coordX, float coordY) 
    {
        int x = (int)coordX;
        int y = (int)coordY;
        float diffX = coordX - x;
        float diffY = coordY - y;

        float p00 = heightMap[x, y];
        float p10 = heightMap[x + 1, y];
        float p01 = heightMap[x, y + 1];
        float p11 = heightMap[x + 1, y + 1];

        float gradientX = (p00 - p10) * diffY + (p01 - p11) * (1 - diffY);
        float gradientY = (p00 - p01) * diffX + (p10 - p11) * (1 - diffX);

        Vector2 gradient = new Vector2(gradientX, gradientY);

        return gradient;
    }

    class Particle
    {
        Vector2 position;
        Vector2 speed;
        float volume;
        float sediment;        

        public Particle(Vector2 position, float volume)
        {
            this.position = position;
            this.volume = volume;
            speed = Vector2.zero;
            sediment = 0f;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public void SetPosition(Vector2 position)
        {
            this.position = position;
        }

        public Vector2 GetSpeed()
        {
            return speed;
        }

        public void SetSpeed(Vector2 speed)
        {
            this.speed = speed;
        }

        public float GetVolume()
        {
            return volume;
        }

        public void SetVolume(float volume)
        {
            this.volume = volume;
        }

        public float GetSediment()
        {
            return sediment;
        }

        public void SetSediment(float sediment)
        {
            this.sediment = sediment;
        }

    }
}
