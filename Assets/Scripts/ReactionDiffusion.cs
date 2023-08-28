using UnityEngine;

public class ReactionDiffusion : MonoBehaviour
{
    public int width = 128;
    public int height = 128;
    public float feedRate = 0.055f;
    public float killRate = 0.062f;
    public float diffusionRateA = 1.0f;
    public float diffusionRateB = 0.5f;
    public float reactionRate = 1.0f;

    private float[,] a;
    private float[,] b;

    private bool started = false;
    public  void StartDiffusion()
    {
          width = PixelController.Instance.pixelArray.GetLength(0);
          height = PixelController.Instance.pixelArray.GetLength(1);

        a = new float[width, height];
        b = new float[width, height];

        delayCounter = 0;
        started = true;
    }

    private int delayCounter = 0;
    private int delayFactor = 15;
    void FixedUpdate()
    {
        if (started == false)
            return;


        if (delayCounter < delayFactor)
        {
            delayCounter++; 
            return;
        }
        else
        {
            delayCounter = 0;
        }
        
        Debug.Log("width" +width );
        Debug.Log("height" + height);
        float[,] newA = new float[width, height];
        float[,] newB = new float[width, height];

        // simulate reaction-diffusion
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                float laplacianA =
                    a[x - 1, y - 1] + a[x - 1, y] + a[x - 1, y + 1] +
                    a[x, y - 1] - 8 * a[x, y] + a[x, y + 1] +
                    a[x + 1, y - 1] + a[x + 1, y] + a[x + 1, y + 1];

                float laplacianB =
                    b[x - 1, y - 1] + b[x - 1, y] + b[x - 1, y + 1] +
                    b[x, y - 1] - 8 * b[x, y] + b[x, y + 1] +
                    b[x + 1, y - 1] + b[x + 1, y] + b[x + 1, y + 1];

                float reaction = a[x, y] * b[x, y] * b[x, y];

                newA[x, y] = a[x, y] + (diffusionRateA * laplacianA - reaction + feedRate * (1 - a[x, y])) * reactionRate;
                newB[x, y] = b[x, y] + (diffusionRateB * laplacianB + reaction - (killRate + feedRate) * b[x, y]) * reactionRate;

                newA[x, y] = Mathf.Clamp(newA[x, y], 0.0f, 1.0f);
                newB[x, y] = Mathf.Clamp(newB[x, y], 0.0f, 1.0f);
            }
        }

        a = newA;
        b = newB;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // ...

                float colorValue = a[x, y] - b[x, y];
                colorValue = Mathf.Clamp(colorValue, -1.0f, 1.0f);

                int tippingState = Mathf.RoundToInt(Mathf.Lerp(0, 4, (colorValue + 1) / 2));
                PixelController.Instance.GetPixel(x, y).SetFoldState(tippingState);
            }
        }

        
    }
}
