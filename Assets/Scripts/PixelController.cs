using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// 3 states
// Tipping, PatternShift (plus how it changes subshift), SideShift
// patroon maakt hem stabieler
// progress door patronen heen, en blijf hange, make soem flutter, add sideward shift, either during or alternate


// pattern within blank state A
// on blank sate B, all the way to even 5 all of them

public class PixelController : MonoBehaviour
{


    public static PixelController Instance;

    [Header("Assigns")] 
    public ControlUI controlUI;
    public GameObject pixelPrefab;
    public Transform pixelParent;
    public Material stateMaterialDefault;
    public Material stateMaterial0;
    public Material stateMaterial1;
    public Material stateMaterial2;
    public Material stateMaterial3;
    public Material stateMaterial4;
    public Material stateMaterial5;


    [Header("Spawn Settings")]
    public float pixelsPerBoundsUnit = 1;
    public float spaceBetweenPixels = 0.7f;

    [Header("Tipping Settings")] 
    public float foldState0;
    public float foldState1;
    public float foldState2;
    public float foldState3;
    public float foldState4;
    public float foldState5;
    public int trafficPerTip = 100;
    public float defaultSpeed = 0;
    public float forcedSpeed = .5f;
    public float tippingPoint = 2.5f; //  between 0 and 5


    public bool Shift;
    
    List<Pixel> pixelList = new List<Pixel>(); 
    public Pixel[,] pixelArray; // 2D array of pixels 
    private SpawnPlane spawnPlane;

    private int trafficPassed = 0;


  [HideInInspector] public bool tipped = false;
  [HideInInspector] public int trafficRate = 0;
  
  private void Awake()
  {
      Instance = this;
  }

  private void Start()
  {
      timeBetweenTicks = 1f / TicksPerSecond;
      nextTickTime = Time.time;
      spawnPlane = FindObjectOfType<SpawnPlane>();
      spawnPlane.GetComponent<MeshRenderer>().enabled = false;
        SpawnPixels();
  }

  // fold ticks
  public float TicksPerSecond = 1;
  private float timeBetweenTicks;
  private float nextTickTime; 

  // traffic ticks
  private float trafficTimer = 0.0f;
  private float trafficTriggerTime = 1.0f; // in seconds
  
  private void Update()
  {
      trafficTimer += Time.deltaTime;
      if(trafficTimer >= trafficTriggerTime)
      {
          trafficTimer = 0.0f;
          CountTraffic();
      }
      if (Input.GetKeyUp(KeyCode.P))
          ToggleColors();
      
      if (Input.GetKeyUp(KeyCode.M))
          FlipBoard();
      if (Input.GetKey(KeyCode.Alpha0))
          TipHoveredPixels(foldState0);
      if (Input.GetKey(KeyCode.Alpha1))
          TipHoveredPixels(foldState1);
      if (Input.GetKey(KeyCode.Alpha2))
          TipHoveredPixels(foldState2);
      if (Input.GetKey(KeyCode.Alpha3))
          TipHoveredPixels(foldState3);
      if (Input.GetKey(KeyCode.Alpha4))
          TipHoveredPixels(foldState4);
      if (Input.GetKey(KeyCode.Alpha5))
          TipHoveredPixels(foldState5);
      
      if (Shift)
          if (Time.time >= nextTickTime)
          {
              ShiftTick();
              nextTickTime = Time.time + timeBetweenTicks;
          }
  }

  private void CountTraffic()
  {
      trafficPassed += trafficRate;
      if (trafficPassed >= trafficPerTip)
      {
          trafficPassed = 0;
          Tip();
      }
      controlUI.  trafficLabel.text = trafficPassed+ "/" + PixelController.Instance.trafficPerTip;
  }

  public void ResetTipState()
  {
      tipped = false;
      controlUI.tipButton.text = "TIP"; 
  }
  
  // called from Button
  public void Tip()
  {
        SetTipped(!tipped);
  } 
  private void SetTipped(bool b)
  {
      tipped = b;
      controlUI.shiftToggle.value = false;
      
      foreach (var pixel in pixelList)
      {
          if (tipped)
          {
              if (pixel.IsFolded())
                  pixel.AnimateFoldState(foldState4, timeBetweenTicks);
              else
                  pixel.AnimateFoldState(foldState5, timeBetweenTicks); 
          }
          else
          {
              if (pixel.IsFolded())
                  pixel.AnimateFoldState(foldState1, timeBetweenTicks);
              else
                  pixel.AnimateFoldState(foldState0, timeBetweenTicks); 
          }
 
      }

      if ( tipped)
          controlUI.tipButton.text = "UNTIP";
      else
          controlUI.tipButton.text = "TIP";
  }
  private bool colorsEnabled = false;
  public void ToggleColors(bool flipState = true)
  {
      if (flipState)
       colorsEnabled = !colorsEnabled;
      foreach (var pixel in pixelList)
      {
          pixel.applyColor = colorsEnabled;
          pixel.ApplyColor();
      }
  }

  private void TipHoveredPixels(float i)
  {
      Debug.Log("TIP" + i);
      foreach (var pixel in pixelList)
          if (pixel.isHovered)
              pixel.SetFoldState(i);
  }

  private void FlipBoard()
  {
       transform.Rotate(new Vector3(180,0,0));
  }

  public void UpdateTicksPerSecond(float newTicksPerSecond)
  {
      TicksPerSecond = newTicksPerSecond;
      timeBetweenTicks = 1f / TicksPerSecond;
  }
  private void ShiftTick()
  { 
      int height = pixelArray.GetLength(1);
      int width = pixelArray.GetLength(0);

      // Store the right-most column of pixels to use it for wrapping
      Pixel[] rightMostColumn = new Pixel[height];
      for (int y = 0; y < height; y++)
      {
          rightMostColumn[y] = pixelArray[width - 1, y];
      }

      // Shift all pixels to the right
      for (int y = 0; y < height; y++)
      {
          for (int x = width - 1; x >= 1; x--)
          {
              Pixel currentPixel = pixelArray[x, y];
              Pixel leftPixel = pixelArray[x - 1, y];

              // Set the tipping state of the current pixel to the tipping state of the left pixel
          //    currentPixel.SetTippingState(leftPixel.GetTippingState());
              
           currentPixel.AnimateFoldState(leftPixel.GetFoldState(), timeBetweenTicks);
          }
      }

      // Set the left-most column to the values of the right-most column (wrap)
      for (int y = 0; y < height; y++)
      {
          pixelArray[0, y].AnimateFoldState(rightMostColumn[y].GetFoldState(), timeBetweenTicks);
      }
  }


  public void SpawnPixels()
  {
      spawnPlane = FindObjectOfType<SpawnPlane>();

      foreach (var pixel in GetPixels())
          if (pixel != null)
              DestroyImmediate(pixel.gameObject);

      
      // get spawnPlane size
      Vector3 planeSize = spawnPlane.GetComponent<Renderer>().bounds.size;

      // calculate number of pixels to spawn in x (length)
      int pixelsToSpawnX = Mathf.CeilToInt(planeSize.x * pixelsPerBoundsUnit);
      // calculate new scale for the pixels
      float newPixelScale = (planeSize.x - spaceBetweenPixels * (pixelsToSpawnX - 1)) / pixelsToSpawnX;
      // calculate number of pixels to spawn in z (width) based on newPixelScale
      int pixelsToSpawnZ = Mathf.CeilToInt(planeSize.z / (newPixelScale + spaceBetweenPixels));


      pixelList = new List<Pixel>();
      pixelArray = new Pixel[pixelsToSpawnX, pixelsToSpawnZ];

      
      // spawn and position pixels
      for (int i = 0; i < pixelsToSpawnX; i++)
      {
          for (int j = 0; j < pixelsToSpawnZ; j++)
          {
              Vector3 position = spawnPlane.transform.position;
              position.x += i * (newPixelScale + spaceBetweenPixels) - planeSize.x / 2 + newPixelScale / 2;
              position.z += j * (newPixelScale + spaceBetweenPixels) - planeSize.z / 2 + newPixelScale / 2;
              GameObject pixelObject = Instantiate(pixelPrefab, position, Quaternion.identity, pixelParent);
              pixelObject.transform.localScale = new Vector3(newPixelScale, newPixelScale, newPixelScale);
              pixelArray[i, j] = pixelObject.GetComponent<Pixel>();
              pixelList.Add(pixelObject.GetComponent<Pixel>());
          }
      }
  }



    
    public List<Pixel> GetPixels()
    {
        return pixelParent.GetComponentsInChildren<Pixel>(true).ToList();
    }
    public Pixel GetPixel(int x, int z)
    {
        return pixelArray[x, z];
    }

    public void SaveToFile(int i)
    {
        string path = "Assets/Saves/save" + i + ".json";

        int width = pixelArray.GetLength(0);
        int height = pixelArray.GetLength(1);

        // Create a list to hold the pixel data
        List<PixelData> pixelDataList = new List<PixelData>();

        // Populate the list with the current state of each pixel
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PixelData pixelData = new PixelData();
                pixelData.CurrentTime = pixelArray[x, y].alembic.CurrentTime;
                pixelDataList.Add(pixelData);
            }
        }

        // Convert the list to a JSON string
        PixelDataList wrapper = new PixelDataList();
        wrapper.list = pixelDataList;
        string json = JsonUtility.ToJson(wrapper);

        // Write the JSON string to a file
        File.WriteAllText(path, json);
 
    }

    public void LoadFile(int i)
    {
        string path = "Assets/Saves/save" + i + ".json";

        // Read the JSON string from the file
        string json = File.ReadAllText(path);

        // Convert the JSON string to a list of pixel data
        PixelDataList wrapper = JsonUtility.FromJson<PixelDataList>(json);
        List<PixelData> pixelDataList = wrapper.list;

        int width = pixelArray.GetLength(0);
        int height = pixelArray.GetLength(1);

        // Update the state of each pixel
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                PixelData pixelData = pixelDataList[x * height + y];
                pixelArray[x, y].alembic.CurrentTime = pixelData.CurrentTime;
            }
        }

        ToggleColors();

    }



}
