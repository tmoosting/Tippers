using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

// CPS change per second: how often per second the Pixels move towards their goal
// Tickers per Pattern: how many GameTicks are needed to call a pattern move
// Ticks slider: change time between patterns
// PatternChangeIntermission: seconds of pause in between
// Pixels calculate how much time they have to move from current to goalstate, and do that much
 

public class PixelController : MonoBehaviour
{


    public static PixelController Instance;

    [Header("Assigns")] 
    public ControlUI controlUI;
    public GameObject pixelPrefab;
    public GameObject blackPlane;
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

    [Header("Shifting")] 
    public bool PixelAutoChanges = false;
    public float PixelMovementPerSecond = 10;
    public float TicksPerPattern = 5; // how many game ticks before a pattern change call is made
    public float PatternChangeIntermission = 2; // time in seconds to delay when calling new pattern-set
    
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

    // fold ticks
    public float TicksPerSecond = 1; // manual adjusted game speed 
    private float timeBetweenTicks;
    private float nextTickTime; 

    // traffic ticks
    private float trafficTimer = 0.0f;
    private float trafficTriggerTime = 1.0f; // in seconds
    
    
    public bool ShiftPattern;
    public bool ShiftSideways;
    public Dictionary<int, bool> savePresence = new Dictionary<int, bool>(); 
    public Pixel[,] pixelArray; // 2D array of pixels  
    
    private  List<Pixel> pixelList = new List<Pixel>(); 

    private SpawnPlane spawnPlane;

    private int trafficPassed = 0;



  [HideInInspector] public bool tipped = false;
  [HideInInspector] public int trafficRate = 0;
  [HideInInspector]   public int currentPattern = 99;

  
  private void Awake()
  {
      Instance = this;
      VerifyLoadFiles();

  }

  private void Start()
  {
      shiftTicksCounter = 0;
      currentPattern = 99;
      timeBetweenTicks = 1f / TicksPerSecond;
      nextTickTime = Time.time;
      spawnPlane = FindObjectOfType<SpawnPlane>();
      spawnPlane.GetComponent<MeshRenderer>().enabled = false;  
       
      SpawnPixels();
      FlipSideways();
  } 
  
  private float frameCounter = 0; 
  void Update()
  {
      // Trigger Pixels x times per second (ChangesPerSecond)
      if (PixelAutoChanges)
      {
          frameCounter += Time.deltaTime * PixelMovementPerSecond; 
          if (frameCounter >= 1)
          {
              foreach (var pixel in pixelList)
                  pixel.ChangeTowardsGoal();
              frameCounter--; 
          }
      }
 
      
      // Trigger Field State checks every Tick
      if (Time.time >= nextTickTime)
      {
          ShiftTick();
          nextTickTime = Time.time + timeBetweenTicks;
      }
      
      trafficTimer += Time.deltaTime;
      if(trafficTimer >= trafficTriggerTime)
      {
          trafficTimer = 0.0f;
          CountTraffic();
      }
      #region KeyInput
      if (Input.GetKeyUp(KeyCode.P))
          ToggleColors();
      if (Input.GetKeyUp(KeyCode.T))
          ToggleWindow();
      if (Input.GetKeyUp(KeyCode.M))
          FlipBoard(); 
      if (Input.GetKeyUp(KeyCode.N))
          blackPlane.SetActive(!blackPlane.activeSelf);
      if (Input.GetKeyUp(KeyCode.O))
          FlipForward();
      if (Input.GetKeyUp(KeyCode.I))
          FlipBackward();
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
      #endregion
  
  }
  private void FlipForward()
  {
      int height = pixelArray.GetLength(1);
      int width = pixelArray.GetLength(0);
 
      for (int y = 0; y < height; y++)
      {
          for (int x = width-1; x >= 0; x--)
          {
              Pixel currentPixel = pixelArray[x, y];
              
              currentPixel.AnimateFoldState(5, timeBetweenTicks);
          }
      } 
  }
  private void FlipBackward()
  {
      int height = pixelArray.GetLength(1);
      int width = pixelArray.GetLength(0);
 
      for (int y = 0; y < height; y++)
      {
          for (int x = width-1; x >= 0; x--)
          {
              Pixel currentPixel = pixelArray[x, y];
              
              currentPixel.AnimateFoldState(0, timeBetweenTicks);
          }
      } 
  }




  private int shiftTicksCounter = 0;
  private void ShiftTick()
  {

      if (ShiftSideways)
          ShiftPixelsSideways();
      //todo if sideways do sideways, but without coroutine

      if (ShiftPattern)
      {
          shiftTicksCounter++;
          if (shiftTicksCounter >= TicksPerPattern)
          {
              MakePatternChange();
              shiftTicksCounter = 0;
          }
      } 
  }

  public void MakePatternChange()
  {
      //Debug.Log("CHANGE FROM " + currentPattern);
      if (currentPattern == 99)  // At starting pattern
          CallPattern(0);
      else  if (currentPattern == 4) // at final pattern
          CallPattern(0);
      else if (savePresence[currentPattern + 1] == true) // next pattern available
          CallPattern(currentPattern + 1);
      else
          CallPattern(0);
  }

  private void CallPattern(int i)
  {
      currentPattern = i;
      StartCoroutine(SetPatternWithDelay(i));
      
  }

  private IEnumerator SetPatternWithDelay(int i)
  {
    //  Debug.Log("Pause 0 ");
      yield return new WaitForSeconds(PatternChangeIntermission);
  //    Debug.Log("Pause 1 ");
      SetPixelDataFromFile(i);
  }

  private void SetPixelDataFromFile(int i)
  {
      string path = "Assets/Saves/save" + i + ".json"; 
      string json = File.ReadAllText(path);  
      PixelDataList wrapper = JsonUtility.FromJson<PixelDataList>(json);
      List<PixelData> pixelDataList = wrapper.list; 
      int width = pixelArray.GetLength(0);
      int height = pixelArray.GetLength(1);
 
      for (int x = 0; x < width; x++)
      {
          for (int y = 0; y < height; y++)
          {
              PixelData pixelData = pixelDataList[x * height + y];
              pixelArray[x, y].SetTargetTimeState(pixelData.CurrentTime);
          }
      } 
  }
  private void ShiftPixelsSideways()
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
  
  private void VerifyLoadFiles()
  {
      savePresence = new Dictionary<int, bool>();
      savePresence.Add(0,  File.Exists("Assets/Saves/save0.json"));
      savePresence.Add(1,  File.Exists("Assets/Saves/save1.json"));
      savePresence.Add(2,  File.Exists("Assets/Saves/save2.json"));
      savePresence.Add(3,  File.Exists("Assets/Saves/save3.json"));
      savePresence.Add(4,  File.Exists("Assets/Saves/save4.json")); 
  }

  private void FlipSideways()
  {
      transform.Rotate(new Vector3(90,0,90));
  }

  private void ToggleWindow()
  {  
      controlUI.ShowWindow(!controlUI.showWindow);
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
      controlUI.shiftSideToggle.value = false;
      
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
              Pixel pixel = pixelObject.GetComponent<Pixel>();
              pixel.Initialize();
              pixelArray[i, j] = pixel;
              pixelList.Add(pixel);
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
        savePresence[i] = true;
        controlUI.RefreshWindow();
    }


    public void LoadFile(int i)
    {
        currentPattern = i;
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
                pixelArray[x, y].SetStateFromLoad ( pixelData.CurrentTime);
            }
        } 
        ToggleColors(); 
    }



}
