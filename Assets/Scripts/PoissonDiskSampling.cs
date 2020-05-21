using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiskSampling
{

    public static List<Vector2> GeneratePoints(int seed, int width, int height, int LOD, int chunkCount, float radius, int iterationCount)
    {
        System.Random prng = new System.Random(seed);
        float cellSize = radius / Mathf.Sqrt(2);
        float minPosition = 1f / LOD;
        float maxPositionX = (float)(width - MapGenerator.borderSize * 2) / LOD;
        float maxPositionY = (float)(height - MapGenerator.borderSize * 2) / LOD;

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();
        int[,] pointsMap = new int[Mathf.CeilToInt(maxPositionX / cellSize), Mathf.CeilToInt(maxPositionY / cellSize)];

        for (int j = 0; j < chunkCount; j++) {            
            Vector2 startSpawnPoints = new Vector2(NextFloat(prng, minPosition, maxPositionX), NextFloat(prng, minPosition, maxPositionY));
            spawnPoints.Add(startSpawnPoints);

            while (spawnPoints.Count > 0) {
                int spawnIndex = prng.Next(0, spawnPoints.Count);
                Vector2 spawnPosition = spawnPoints[spawnIndex];
                bool isPlaceFound = false;

                for (int i = 0; i < iterationCount; i++) {
                    float angle = (float)prng.NextDouble() * (Mathf.PI * 2);
                    Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                    Vector2 pointPosition = spawnPosition + (direction * NextFloat(prng, radius, radius * 2));
                    if (IsValid(pointPosition, minPosition, maxPositionX, maxPositionY, cellSize, radius, points, pointsMap)) {
                        isPlaceFound = true;
                        points.Add(pointPosition);
                        spawnPoints.Add(pointPosition);
                        pointsMap[(int)(pointPosition.x / cellSize), (int)(pointPosition.y / cellSize)] = points.Count;
                        break;
                    }
                }

                if (!isPlaceFound) {
                    spawnPoints.RemoveAt(spawnIndex);
                }
            }

        }

        return points;
    }

    static bool IsValid(Vector2 pointPosition, float minPosition, float maxPositionX, float maxPositionY, float cellSize, float radius, List<Vector2> points, int[,] pointsMap)
    {
        if (pointPosition.x >= minPosition && pointPosition.x < maxPositionX && pointPosition.y >= minPosition && pointPosition.y < maxPositionY) {
            int cellX = (int)(pointPosition.x / cellSize);
            int cellY = (int)(pointPosition.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, pointsMap.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, pointsMap.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++) {
                for (int y = searchStartY; y <= searchEndY; y++) {
                    int pointIndex = pointsMap[x, y] - 1;
                    if (pointIndex != -1) {
                        float sqrDst = (pointPosition - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        return false;
    }

    static float NextFloat(System.Random prng, float minValue, float maxValue)
    {
        return Mathf.Lerp(minValue, maxValue, (float)prng.NextDouble());
    }

}
