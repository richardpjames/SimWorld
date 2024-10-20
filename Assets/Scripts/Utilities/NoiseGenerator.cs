using System;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public static float[,] Generate(Vector2Int size, float scale, Wave[] waves, Vector2 offset)
    {
        // create the noise map
        float[,] noiseMap = new float[size.x, size.y];
        // loop through each element in the noise map
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // calculate the sample positions
                float samplePosX = (float)x * scale + offset.x;
                float samplePosY = (float)y * scale + offset.y;
                float normalization = 0.0f;
                // loop through each wave
                foreach (Wave wave in waves)
                {
                    // sample the perlin noise taking into consideration amplitude and frequency
                    noiseMap[x, y] += wave.amplitude * Mathf.PerlinNoise(samplePosX * wave.frequency + wave.seed, samplePosY * wave.frequency + wave.seed);
                    normalization += wave.amplitude;
                }
                // normalize the value
                noiseMap[x, y] /= normalization;
            }
        }

        return noiseMap;
    }
}

[Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}