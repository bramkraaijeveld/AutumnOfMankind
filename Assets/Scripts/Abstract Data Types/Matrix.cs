using UnityEngine;

public class Matrix {

    private float[,] data;
    private int size;

    public float[,] Data { get { return data; } }
    public int Size { get { return size; } }

    public Matrix(float[,] data) {
        if (data.GetLength(0) != data.GetLength(1)) throw new System.Exception("Matrix Construction Error: Matrix must be square");
        this.data = data;
        size = this.data.GetLength(0);
    }

    public Matrix(float[] data, int size) {
        if (data.Length != size * size) throw new System.Exception("Matrix Construction Error: Input Array must have length [size * size]");
        this.data = new float[size, size];
        System.Buffer.BlockCopy(data, 0, this.data, 0, this.data.Length * 4);
        this.size = size;
    }

    //-- MATRIX METHDOS --//
    public Matrix Slice(int x, int y, int size) {
        float[,] m = new float[size, size];
        for (int i = 0; i < size; i++) System.Buffer.BlockCopy(data, ((x + i) * this.size + y) * 4, m, i * size * 4, size * 4);
        return new Matrix(m);
    }

    public Matrix[,] Partition(int n, bool inclusive = false) {
        Matrix[,] matrices = new Matrix[n, n];

        if (inclusive) {
            if ((size - 1) % n != 0) throw new System.Exception("Matrix Partitioning Error: Matrix (Size-1) Must be divisible by n");
            int s = (size - 1) / n;
            for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) matrices[i, j] = Slice(i * s, j * s, s + 1);
        }
        else /* If Exclusive */ {
            if (size % n != 0) throw new System.Exception("Matrix Partitioning Error: Matrix Size Must be divisible by n");
            int s = size / n;
            for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) matrices[i, j] = Slice(i * s, j * s, s);
        }
        return matrices;
    }

    public Matrix Scale(float factor) {
        int s = (int)(size * factor);
        float[,] m = new float[s, s];
        for (int x = 0; x < s; x++) for (int y = 0; y < s; y++)
                m[x, y] = this[x / (factor * size), y / (factor * size)];
        return new Matrix(m);
    }

    public Matrix Copy() {
        float[,] m = new float[size, size];
        for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) m[x, y] = data[x, y];
        return new Matrix(m);
    }

    public Matrix Normalise() {
        return this / Sum();
    }

    public Matrix Convolute(Matrix kernel, bool GPU = true) {
        if (kernel.Size % 2 != 1) throw new System.Exception("Matrix Convolution Error: Kernel Size must be an odd number");

        int kernelSize = (kernel.size - 1) / 2;
        float[,] m = new float[size, size];

        if (GPU) {
            ComputeShader shader = Resources.Load<ComputeShader>("Convolution");
            int k = shader.FindKernel("Convolution");

            ComputeBuffer inputBuffer = new ComputeBuffer(size * size, sizeof(float));
            inputBuffer.SetData(data);
            shader.SetBuffer(k, "input", inputBuffer);

            ComputeBuffer kernelBuffer = new ComputeBuffer(kernel.size * kernel.size, sizeof(float));
            kernelBuffer.SetData(kernel.data);
            shader.SetBuffer(k, "kernel", kernelBuffer);

            ComputeBuffer outputBuffer = new ComputeBuffer(size * size, sizeof(float));
            shader.SetBuffer(k, "output", outputBuffer);

            shader.SetInt("matrixSize", size);
            shader.SetInt("kernelSize", kernelSize);

            shader.Dispatch(k, size / 8, size / 8, 1);

            outputBuffer.GetData(m);

            inputBuffer.Release();
            kernelBuffer.Release();
            outputBuffer.Release();
        }
        else /* IF NOT GPU */ {

            for (int x = 0; x < size; x++) {
                for (int y = 0; y < size; y++) {

                    float sum = 0;
                    for (int i = -kernelSize; i <= kernelSize; i++) {
                        for (int j = -kernelSize; j <= kernelSize; j++) {
                            if (x + i >= 0 && x + i < size && y + j >= 0 && y + j < size) {
                                sum += data[x + i, y + j] * kernel[i + kernelSize, j + kernelSize];
                            }
                        }
                    }

                    m[x, y] = sum;
                }
            }
        }

        return new Matrix(m);
    }

    //-- MATRIX DATA --//
    public float Sum() {
        float sum = 0;
        for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) sum += data[x, y];
        return sum;
    }

    public float Average() {
        return Sum() / data.Length;
    }

    //-- MATRIX DEFINITIONS --//
    public static Matrix Zeros(int size) {
        return new Matrix(new float[size, size]);
    }

    public static Matrix Ones(int size) {
        float[,] m = new float[size, size];
        for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) m[x, y] = 1;
        return new Matrix(m);
    }

    public static Matrix Linear(int size) {
        float[,] m = new float[size, size];
        for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) m[x, y] = (x * size + y) / (float)(size * size);
        return new Matrix(m);
    }

    public static Matrix Gaussian(int size, Vector2 spread) {
        float[,] m = new float[size, size];
        for (int x = 0; x < size; x++) for (int y = 0; y < size; y++) m[x, y] = Mathf.Exp(-(Mathf.Pow(x - (size / 2.0f), 2) / (2 * spread.x * size) + Mathf.Pow(y - (size / 2.0f), 2) / (2 * spread.y * size)));
        return new Matrix(m);
    }

    public static Matrix Voronoi(int seed, int size, int nPoints) {
        System.Random r = new System.Random(seed);
        Vector2[] points = new Vector2[nPoints];
        for (int i = 0; i < nPoints; i++) points[i] = new Vector2(r.Next(0, size), r.Next(0, size));

        ComputeShader shader = Resources.Load<ComputeShader>("Voronoi");
        int kernel = shader.FindKernel("Voronoi");

        shader.SetInt("size", size);
        shader.SetInt("nPoints", nPoints);

        ComputeBuffer pointBuffer = new ComputeBuffer(nPoints, sizeof(float) * 2);
        pointBuffer.SetData(points);
        shader.SetBuffer(kernel, "points", pointBuffer);

        ComputeBuffer outputBuffer = new ComputeBuffer(size * size, sizeof(float));
        shader.SetBuffer(kernel, "output", outputBuffer);

        shader.Dispatch(kernel, size / 8, size / 8, 1);

        float[,] m = new float[size, size];
        outputBuffer.GetData(m);

        pointBuffer.Release();
        outputBuffer.Release();

        return new Matrix(m);
    }

    public static Matrix Perlin(int size, float height, float persistence, float scale, Vector4 offset) {
        int[] permutation = { 151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180, 151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180 };

        ComputeShader perlin = Resources.Load<ComputeShader>("Perlin");
        int kernel = perlin.FindKernel("Perlin");

        //Copy Permutation Table to Shader
        ComputeBuffer permBuf = new ComputeBuffer(permutation.Length, sizeof(int));
        permBuf.SetData(permutation);
        perlin.SetBuffer(kernel, "p", permBuf);

        //Create Output buffer
        ComputeBuffer output = new ComputeBuffer(size * size, sizeof(float));
        perlin.SetBuffer(kernel, "output", output);

        //Set Parameters
        perlin.SetInt("size", size);
        perlin.SetFloat("height", height);
        perlin.SetFloat("persistence", persistence);
        perlin.SetFloat("scale", scale);
        perlin.SetVector("offset", offset);

        //Run Shader
        perlin.Dispatch(kernel, size / 32, size / 32, 1);

        //Get Buffer Data
        float[,] rawData = new float[size, size];
        output.GetData(rawData);

        //Release Buffers
        output.Release();
        permBuf.Release();

        return new Matrix(rawData);
    }

    /// <summary>Generates Diamond Square Matrix</summary>
    /// <param name="seed">Seed for Pseudo-Random Number Generation</param>
    /// <param name="iterations">Matrix size will be 2^iterations + 1</param>
    /// <param name="height">Matrix Scaling</param>
    /// <param name="persistance">Persistance of Pseudo-Random Number Amplitude</param>
    /// <returns>Diamond Square Matrix</returns>
    public static Matrix DiamondSquare(int seed, int iterations, float height, float persistance) {
        int size = (int)Mathf.Pow(2, iterations) + 1;
        float[,] data = new float[size, size];

        System.Random r = new System.Random(seed);

        float amplitude = height / 20000f;

        data[0, 0] = r.Next(-10000, 10000) * amplitude;
        data[0, size - 1] = r.Next(-10000, 10000) * amplitude;
        data[size - 1, 0] = r.Next(-10000, 10000) * amplitude;
        data[size - 1, size - 1] = r.Next(-10000, 10000) * amplitude;

        for (int s = size - 1; s > 0; s /= 2) {
            amplitude *= persistance;
            for (int x = 0; x < size - 1; x += s) {
                for (int y = 0; y < size - 1; y += s) {
                    // Diamond Step
                    data[x + s / 2, y + s / 2] = (data[x, y] + data[x + s, y] + data[x, y + s] + data[x + s, y + s]) / 4 + r.Next(-10000, 10000) * amplitude;

                    // Square Steps
                    data[x + s / 2, y] = (data[x, y] + data[x + s, y] + data[x + s / 2, y + s / 2] + data[x + s / 2, Mathf.Abs((y - s / 2) % size)]) / 4 + r.Next(-10000, 10000) * amplitude;
                    data[x + s / 2, y + s] = (data[x, y + s] + data[x + s, y + s] + data[x + s / 2, y + s / 2] + data[x + s / 2, (y + 3 * s / 2) % size]) / 4 + r.Next(-10000, 10000) * amplitude;
                    data[x, y + s / 2] = (data[x, y] + data[x, y + s] + data[x + s / 2, y + s / 2] + data[Mathf.Abs((x - s / 2) % size), y + s / 2]) / 4 + r.Next(-10000, 10000) * amplitude;
                    data[x + s, y + s / 2] = (data[x + s, y] + data[x + s, y + s] + data[x + s / 2, y + s / 2] + data[(x + 3 * s / 2) % size, y + s / 2]) / 4 + r.Next(-10000, 10000) * amplitude;
                }
            }
        }

        return new Matrix(data);
    }

    //-- MATRIX CONVERSIONS --//
    public static Texture2D Texture(Matrix r = null, Matrix g = null, Matrix b = null, Matrix a = null) {
        int size = 0;
        if (r != null) size = r.size;
        if (g != null) size = g.size;
        if (b != null) size = b.size;
        if (a != null) size = a.size;

        float min = float.MaxValue, max = float.MinValue;
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                if (r != null && r[x, y] > max) max = r[x, y];
                if (g != null && g[x, y] > max) max = g[x, y];
                if (b != null && b[x, y] > max) max = b[x, y];
                if (a != null && a[x, y] > max) max = a[x, y];

                if (r != null && r[x, y] < min) min = r[x, y];
                if (g != null && g[x, y] < min) min = g[x, y];
                if (b != null && b[x, y] < min) min = b[x, y];
                if (a != null && a[x, y] < min) min = a[x, y];
            }
        }

        Color32[] colors = new Color32[size * size];
        for (int i = 0; i < colors.Length; i++) {
            byte rv = r != null ? (byte)(int)((r[i % size, i / size] - min) / (max - min) * 255) : (byte)0;
            byte gv = g != null ? (byte)(int)((g[i % size, i / size] - min) / (max - min) * 255) : (byte)0;
            byte bv = b != null ? (byte)(int)((b[i % size, i / size] - min) / (max - min) * 255) : (byte)0;
            byte av = a != null ? (byte)(int)((a[i % size, i / size] - min) / (max - min) * 255) : (byte)255;

            colors[i] = new Color32(rv, gv, bv, av);
        }

        Texture2D texture = new Texture2D(size, size);
        texture.SetPixels32(colors);
        texture.Apply();

        return texture;
    }

    public Texture2D ToTexture() {

        float min = float.MaxValue, max = float.MinValue;
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                if (data[x, y] > max) max = data[x, y];
                if (data[x, y] < min) min = data[x, y];
            }
        }

        Color32[] colors = new Color32[data.Length];
        for (int i = 0; i < colors.Length; i++) {
            byte value = (byte)(int)((data[i % size, i / size] - min) / (max - min) * 255);
            colors[i] = new Color32(value, value, value, 255);
        }

        Texture2D texture = new Texture2D(size, size);
        texture.SetPixels32(colors);
        texture.Apply();

        return texture;
    }

    public override string ToString() {
        string matrixString = "Matrix" + size + "x" + size;

        if (data.Length < 512) {
            matrixString += "\n[";
            for (int y = size - 1; y >= 0; y--) {
                matrixString += "[";
                for (int x = 0; x < size; x++) matrixString += data[x, y] + ", ";
                matrixString += "]\n";
            }
            matrixString += "]";
        }

        return matrixString;
    }

    public float[] ToArray() {
        float[] array = new float[data.Length];
        System.Buffer.BlockCopy(data, 0, array, 0, data.Length * 4);
        return array;
    }

    public void FromArray(float[] array) {
        if (data.Length != array.Length) throw new System.Exception("Matrix FromArray Error: Array length must be equal to matrix size squared");
        System.Buffer.BlockCopy(array, 0, data, 0, data.Length * 4);
    }

    //-- OPERATOR OVERLOADS --//
    public float this[int x, int y] { get { return data[Mathf.Min(Mathf.Max(x, 0), size - 1), Mathf.Min(Mathf.Max(y, 0), size - 1)]; } set { data[Mathf.Min(Mathf.Max(x, 0), size - 1), Mathf.Min(Mathf.Max(y, 0), size - 1)] = value; } }

    // Bilinear Interpolation
    // float u & v = [0..1]
    public float this[float u, float v] {
        get {
            float x = u * (size - 1), y = v * (size - 1);
            int x0 = Mathf.Clamp((int)x, 0, size - 1),
                x1 = Mathf.Clamp((int)(x + 1), 0, size - 1),
                y0 = Mathf.Clamp((int)y, 0, size - 1),
                y1 = Mathf.Clamp((int)(y + 1), 0, size - 1);

            x %= 1; y %= 1;

            return data[x0, y0] * (1 - x) * (1 - y) +
                   data[x1, y0] * x * (1 - y) +
                   data[x0, y1] * (1 - x) * y +
                   data[x1, y1] * x * y;
        }
    }

    // Matrix - Matrix Operations //
    public static Matrix operator +(Matrix m1, Matrix m2) {
        if (m1.size != m2.size) throw new System.Exception("Matrix Addition Error: Matrices must be of same size");

        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] + m2.data[x, y];
        return new Matrix(m);
    }

    public static Matrix operator -(Matrix m1, Matrix m2) {
        if (m1.size != m2.size) throw new System.Exception("Matrix Addition Error: Matrices must be of same size");

        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] - m2.data[x, y];
        return new Matrix(m);
    }

    public static Matrix operator *(Matrix m1, Matrix m2) {
        if (m1.size != m2.size) throw new System.Exception("Matrix Addition Error: Matrices must be of same size");

        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] * m2.data[x, y];
        return new Matrix(m);
    }

    public static Matrix operator /(Matrix m1, Matrix m2) {
        if (m1.size != m2.size) throw new System.Exception("Matrix Addition Error: Matrices must be of same size");

        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] / m2.data[x, y];
        return new Matrix(m);
    }

    // Matrix - Float Operations //
    public static Matrix operator +(Matrix m1, float f) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] + f;
        return new Matrix(m);
    }

    public static Matrix operator -(Matrix m1, float f) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] - f;
        return new Matrix(m);
    }

    public static Matrix operator *(Matrix m1, float f) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] * f;
        return new Matrix(m);
    }

    public static Matrix operator /(Matrix m1, float f) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] / f;
        return new Matrix(m);
    }

    // Float - Matrix Operations //
    public static Matrix operator +(float f, Matrix m1) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] + f;
        return new Matrix(m);
    }

    public static Matrix operator -(float f, Matrix m1) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] - f;
        return new Matrix(m);
    }

    public static Matrix operator *(float f, Matrix m1) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] * f;
        return new Matrix(m);
    }

    public static Matrix operator /(float f, Matrix m1) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = m1.data[x, y] / f;
        return new Matrix(m);
    }

    public static Matrix operator -(Matrix m1) {
        float[,] m = new float[m1.size, m1.size];
        for (int x = 0; x < m1.size; x++) for (int y = 0; y < m1.size; y++) m[x, y] = -m1.data[x, y];
        return new Matrix(m);
    }
}
