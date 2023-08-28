using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternGenerator : MonoBehaviour
{
    public int width;
    public int height;
    public float feedRate = 0.055f;
    public float killRate = 0.062f;
    public float diffusionRateA = 1.0f;
    public float diffusionRateB = 0.5f;
    public int iterations = 10;

    private float[,] a;
    private float[,] b;
    
    public static PatternGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CreatePattern()
    {
        width = PixelController.Instance.pixelArray.GetLength(0);
        height = PixelController.Instance.pixelArray.GetLength(1);
        
        
        Initialize();
        
        for(int i = 0; i < iterations; i++)
        {
            ReactionDiffusionStep();
        }

        UpdatePixels();
        PixelController.Instance.ToggleColors(false);

    }

    private void Initialize()
    {
        a = new float[width, height];
        b = new float[width, height];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                a[x, y] = 1;
                b[x, y] = (Random.Range(0, 100) < 20) ? 1 : 0;

            }
        }
    }

    private void ReactionDiffusionStep()
    {
        float[,] nextA = new float[width, height];
        float[,] nextB = new float[width, height];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                float aCenter = a[x, y];
                float bCenter = b[x, y];

                float laplaceA = 0;
                float laplaceB = 0;

                // Compute Laplacian
                for(int dx = -1; dx <= 1; dx++)
                {
                    for(int dy = -1; dy <= 1; dy++)
                    {
                        int nx = Mathf.Clamp(x + dx, 0, width - 1);
                        int ny = Mathf.Clamp(y + dy, 0, height - 1);
                    
                        float weight = 0.05f;
                        if(dx == 0 && dy == 0) weight = -1.0f;
                        else if(dx == 0 || dy == 0) weight = 0.2f;

                        laplaceA += weight * a[nx, ny];
                        laplaceB += weight * b[nx, ny];
                    }
                }

                // Compute next a and b
                nextA[x, y] = aCenter + (diffusionRateA * laplaceA - aCenter * bCenter * bCenter + feedRate * (1 - aCenter));
                nextB[x, y] = bCenter + (diffusionRateB * laplaceB + aCenter * bCenter * bCenter - (killRate + feedRate) * bCenter);

                // Clamp to valid range
                nextA[x, y] = Mathf.Clamp(nextA[x, y], 0, 1);
                nextB[x, y] = Mathf.Clamp(nextB[x, y], 0, 1);
            }
        }

        a = nextA;
        b = nextB;
    }


    private void UpdatePixels()
    {
        PixelController controller = PixelController.Instance;
        controller.ResetTipState();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float a_value = a[x, y];
                float b_value = b[x, y];
        
                // Check the difference between 'a' and 'b'
                float diff = a_value - b_value;

                // Map the difference to a tipping state value between 0 and 5
                int tippingStateInt = Mathf.RoundToInt(Mathf.Lerp(0, 5, diff));

                float tippingState;
                switch (tippingStateInt)
                {
                    case 0: tippingState = controller.foldState0; break;
                    case 1: tippingState = controller.foldState1; break;
                    case 2: tippingState = controller.foldState2; break;
                    case 3: tippingState = controller.foldState3; break;
                    case 4: tippingState = controller.foldState4; break;
                    case 5: tippingState = controller.foldState5; break;
                    default: tippingState = controller.foldState0; break;
                }
                /*switch (tippingStateInt)
                {
                    case 0: tippingState = controller.foldState0; break;
                    case 1: tippingState = controller.foldState0; break;
                    case 2: tippingState = controller.foldState0; break;
                    case 3: tippingState = controller.foldState1; break;
                    case 4: tippingState = controller.foldState1; break;
                    case 5: tippingState = controller.foldState1; break;
                    default: tippingState = controller.foldState0; break;
                }*/

                controller.GetPixel(x, y).SetFoldState(tippingState);
            }
        }
    }
}
