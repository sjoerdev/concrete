using System.Numerics;

namespace Concrete;

public static class Metrics
{
    public static bool dataIsReady = false;
    
    public static int framesToCheck = 512;

    public static float[] lastFrameRates => frameRates.Skip(frameRates.Count - framesToCheck).ToArray();
    public static float[] lastFrameTimes => frameTimes.Skip(frameTimes.Count - framesToCheck).ToArray();

    public static float averageFrameRate;
    public static float averageFrameTime;

    private static float timer;

    private static List<float> frameRates = [];
    private static List<float> frameTimes = [];

    public static void Update(float deltaTime)
    {
        frameTimes.Add(deltaTime * 1000f);
        frameRates.Add(1 / deltaTime);

        timer += deltaTime;
        if (timer > 1)
        {
            averageFrameRate = lastFrameRates.Average();
            averageFrameTime = lastFrameTimes.Average();
            timer = 0;
        }

        if (frameTimes.Count > framesToCheck && frameRates.Count > framesToCheck) dataIsReady = true;
    }
}