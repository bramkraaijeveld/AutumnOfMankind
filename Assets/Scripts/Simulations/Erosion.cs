using UnityEngine;
using System.Collections.Generic;

public class Erosion {

    public static float gravity = 9.81f;

    /*  public static IEnumerable<Matrix> Thermal(Matrix heightMap, Matrix soilMap, float talus, float maxflow, int iterations) {
            ComputeShader shader = Resources.Load<ComputeShader>("Erosion");

            int kernel = shader.FindKernel("Thermal");
            int size = heightMap.Size;

            shader.SetInt("size", size);
            shader.SetFloat("talus", talus);
            shader.SetFloat("maxflow", maxflow);

            ComputeBuffer heightMapBuffer = new ComputeBuffer(size * size, sizeof(float));
            heightMapBuffer.SetData(heightMap.Data);
            shader.SetBuffer(kernel, "heightMap", heightMapBuffer);

            ComputeBuffer soilMapBuffer = new ComputeBuffer(size * size, sizeof(float));
            soilMapBuffer.SetData(soilMap.Data);
            shader.SetBuffer(kernel, "soilMap", soilMapBuffer);

            ComputeBuffer outflowBuffer = new ComputeBuffer(size * size, sizeof(float) * 4);
            shader.SetBuffer(kernel, "outflowMap", outflowBuffer);

            float[,] soilData = new float[size, size];

            for (int i = 0; i < iterations; i++) {
                // Dispatch Flow Calculation
                shader.SetInt("step", 0);
                shader.Dispatch(kernel, size / 8, size / 8, 1);

                // Dispatch Flow Accumulation
                shader.SetInt("step", 1);
                shader.Dispatch(kernel, size / 8, size / 8, 1);

                soilMapBuffer.GetData(soilData);

                yield return new Matrix(soilData);
            }

            heightMapBuffer.Release();
            soilMapBuffer.Release();
            outflowBuffer.Release();
        }
    */

    public static IEnumerable<Matrix[]> Hydraulic(Matrix terrainMap, Matrix rainMap, float pipeArea, float pipeLength, float maxSedimentCapacity, float maxErosionDepth, float dissolveSpeed, float depositionSpeed, float evaporation, float tiltlim, float deltaTime, int cycles, int iterations) {
        ComputeShader shader = Resources.Load<ComputeShader>("Hydraulic");
        int kernel = shader.FindKernel("Hydraulic");
        int size = terrainMap.Size;

        shader.SetInt("size", size);
        shader.SetFloat("dt", deltaTime);
        shader.SetFloat("g", gravity);
        shader.SetFloat("pipeArea", pipeArea);
        shader.SetFloat("pipeLength", pipeLength);
        shader.SetFloat("maxSedimentCapacity", maxSedimentCapacity);
        shader.SetFloat("maxErosionDepth", maxErosionDepth);
        shader.SetFloat("dissolveSpeed", dissolveSpeed);
        shader.SetFloat("depositionSpeed", depositionSpeed);
        shader.SetFloat("evaporation", evaporation);
        shader.SetFloat("tiltlim", tiltlim);

        ComputeBuffer terrainBuffer = new ComputeBuffer(size * size, sizeof(float));
        terrainBuffer.SetData(terrainMap.Data);
        shader.SetBuffer(kernel, "heightMap", terrainBuffer);

        ComputeBuffer rainBuffer = new ComputeBuffer(size * size, sizeof(float));
        rainBuffer.SetData(rainMap.Data);
        shader.SetBuffer(kernel, "rainMap", rainBuffer);

        ComputeBuffer waterBuffer = new ComputeBuffer(size * size, sizeof(float));
        waterBuffer.SetData(new float[size, size]);
        shader.SetBuffer(kernel, "waterMap", waterBuffer);

        ComputeBuffer fluxBuffer = new ComputeBuffer(size * size, sizeof(float) * 4);
        fluxBuffer.SetData(new Vector4[size, size]);
        shader.SetBuffer(kernel, "fluxMap", fluxBuffer);

        ComputeBuffer velocityBuffer = new ComputeBuffer(size * size, sizeof(float) * 2);
        velocityBuffer.SetData(new Vector2[size, size]);
        shader.SetBuffer(kernel, "velocityMap", velocityBuffer);

        ComputeBuffer sedimentCapacityBuffer = new ComputeBuffer(size * size, sizeof(float));
        sedimentCapacityBuffer.SetData(new float[size, size]);
        shader.SetBuffer(kernel, "sedimentCapacityMap", sedimentCapacityBuffer);

        ComputeBuffer sedimentBuffer = new ComputeBuffer(size * size, sizeof(float));
        sedimentBuffer.SetData(new float[size, size]);
        shader.SetBuffer(kernel, "sedimentMap", sedimentBuffer);

        ComputeBuffer debugBuffer = new ComputeBuffer(size * size, sizeof(float));
        debugBuffer.SetData(new float[size, size]);
        shader.SetBuffer(kernel, "debugMap", debugBuffer);

        float[,] terrainData = new float[size, size];
        float[,] waterData = new float[size, size];
        float[,] debugData = new float[size, size];

        for (int i = 0; i < iterations; i++) {
            for (int c = 0; c < cycles; c++) {
                for (int step = 0; step <= 5; step++) {
                    shader.SetInt("step", step);
                    shader.Dispatch(kernel, size / 8, size / 8, 1);
                }
            }

            terrainBuffer.GetData(terrainData);
            waterBuffer.GetData(waterData);
            debugBuffer.GetData(debugData);

            yield return new Matrix[] { new Matrix(terrainData), new Matrix(waterData), new Matrix(debugData) };
        }

        terrainBuffer.Release();
        rainBuffer.Release();
        waterBuffer.Release();
        fluxBuffer.Release();
        velocityBuffer.Release();
        sedimentCapacityBuffer.Release();
        sedimentBuffer.Release();
        debugBuffer.Release();
    }

    public static IEnumerable<Matrix> Thermal(Matrix terrainMap, Matrix soilMap, float talusAngle, float pipeArea, float pipeLength, float deltaTime, int cycles, int iterations) {
        ComputeShader shader = Resources.Load<ComputeShader>("Thermal");
        int kernel = shader.FindKernel("Thermal");
        int size = terrainMap.Size;

        shader.SetInt("size", size);
        shader.SetFloat("dt", deltaTime);
        shader.SetFloat("g", gravity);
        shader.SetFloat("pipeArea", pipeArea);
        shader.SetFloat("pipeLength", pipeLength);

        shader.SetFloat("talus", talusAngle);

        ComputeBuffer terrainBuffer = new ComputeBuffer(size * size, sizeof(float));
        terrainBuffer.SetData(terrainMap.Data);
        shader.SetBuffer(kernel, "heightMap", terrainBuffer);

        ComputeBuffer soilBuffer = new ComputeBuffer(size * size, sizeof(float));
        soilBuffer.SetData(soilMap.Data);
        shader.SetBuffer(kernel, "soilMap", soilBuffer);

        ComputeBuffer fluxBuffer = new ComputeBuffer(size * size, sizeof(float) * 4);
        fluxBuffer.SetData(new Vector4[size, size]);
        shader.SetBuffer(kernel, "fluxMap", fluxBuffer);

        float[,] data = new float[size, size];

        for (int i = 0; i < iterations; i++) {
            for (int c = 0; c < cycles; c++) {
                for (int step = 1; step <= 2; step++) {
                    shader.SetInt("step", step);
                    shader.Dispatch(kernel, size / 8, size / 8, 1);
                }
            }

            soilBuffer.GetData(data);

            yield return new Matrix(data);
        }

        terrainBuffer.Release();
        soilBuffer.Release();
        fluxBuffer.Release();
    }
}
