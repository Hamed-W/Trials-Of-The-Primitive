using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset) {

        // We make an array for every coordinate in the map.
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        // Gets a random offset for the octaves.
        for (int i=0;i<octaves;i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Prevents division by zero or invalid behaviour when sampling the noise.
        if (scale <= 0) scale = 0.0001f;
         
        // Used to normalize values between 0-1 later.
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        //Centres the noise sampling around the middle of the map.
        float halfWidth = mapWidth/2f;
        float halfHeight = mapHeight/2f;

        // Generates a noise value for every coordinate in the map.
        for (int y=0; y< mapHeight; y++)
        {
            for (int x= 0; x<mapWidth; x++)
            {
                // Amplitude controls how strongly the current octave contributes.
                float amplitude = 1;

                // Frequency controls how detailed/small the noise pattern is.
                float frequency = 1;

                // Combined noise value from all octaves for this coordinate.
                float noiseHeight = 0;

                // Combine several layers (octaves) of Perlin noise.
                for (int i=0; i<octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    
                    // Usually perlin value is 0-1 but this brings us to -1 to 1.
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Add this octave's contribution to the final noise value.
                    noiseHeight += perlinValue * amplitude;

                    // Persistence reduces the influence of each successive octave.
                    amplitude *= persistence;

                    // Lacunarity increases the frequency of each successive octave.
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x,y] = noiseHeight;
            }
        }

        // Normalizes all generated heights in the heightmap to 0-1 range.
        for (int y=0; y< mapHeight; y++)
        {
            for (int x= 0; x<mapWidth; x++)
            {
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight,noiseMap[x,y]);
            }
        }

        return noiseMap;
    }
}
