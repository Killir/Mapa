using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    public bool getLog = false;
    public int iterationCount;
    public int particleLifetime;
    [Space(10)]
    [Range(0f, 1f)]
    public float inertia;
    [Range(0f, 1f)]
    public float depositionRate;
    [Range(0f, 1f)]
    public float evaporationRate;

    public float startVolume;

    public float[,] Erode(int seed, float[,] heightMap)
    {
        System.Random prng = new System.Random(seed);
        int width = heightMap.GetLength(0) - 1;
        int height = heightMap.GetLength(1) - 1;

        for (int i = 0; i < iterationCount; i++) {
            Vector2 currentPosition = new Vector2(prng.Next(1, width), prng.Next(1, height));
            Particle particle = new Particle(currentPosition, startVolume);

            for (int k = 0; k < particleLifetime; k++) {
                currentPosition = particle.GetPosition();

                Vector2 gradient = CalculatePointGradient(heightMap, particle.GetPosition().x, particle.GetPosition().y);
                Vector2 speed = particle.GetSpeed() + (gradient / particle.GetVolume());
                particle.SetSpeed(speed);
                particle.SetPosition(currentPosition + speed);

                if (speed.magnitude == 0) {
                    break;
                }
                if (particle.GetPosition().x < 1 || particle.GetPosition().x > width || particle.GetPosition().y < 1 || particle.GetPosition().y > height) {
                    break;
                }

                float heightDiff = heightMap[(int)currentPosition.x, (int)currentPosition.y] - heightMap[(int)particle.GetPosition().x, (int)particle.GetPosition().y];
                float capasity = particle.GetVolume() * speed.magnitude * heightDiff;
                if (capasity < 0) {
                    capasity = 0f;
                }
                float capasityDiff = capasity - particle.GetSediment() * depositionRate;
                particle.SetSediment(particle.GetSediment() + capasityDiff);
                heightMap[(int)currentPosition.x, (int)currentPosition.y] -= capasityDiff;

                particle.SetVolume(particle.GetVolume() * (1 - evaporationRate));
            }
        }

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
