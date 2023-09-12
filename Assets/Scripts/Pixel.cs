using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
using UnityEngine.Formats.Alembic.Importer;
 

public class Pixel : MonoBehaviour
{
    public Renderer renderer;
    public AlembicStreamPlayer alembic;

    [HideInInspector] public bool isHovered = false;
    [HideInInspector] public bool applyColor = false;

    public float targetTimeState;
    public float currentSpeed;
    
    
    
    private bool reversing = false;
    private bool forceMovement = false;
    private bool colorApplied = false;
    
    private void Update()
    {
        /*if (forceMovement)
          HandleForcedMovement();
        else
            HandleDefaultMovement();

        CheckForReverse(); */
    }


    public void Initialize()
    {
        targetTimeState = alembic.CurrentTime;

    }  
    public void SetStateFromLoad(float givenTime)
    {
        alembic.CurrentTime = givenTime;
        targetTimeState = givenTime; 
    }
    public void SetTargetTimeState(float targetTime)
    {
        targetTimeState = targetTime;
    }
    public void ChangeTowardsGoal( )
    {
        if (Math.Abs(targetTimeState - alembic.CurrentTime) < 0.001f)
            return;
        float totalChangeDist = Mathf.Abs(targetTimeState - alembic.CurrentTime);
        float totalSeconds = PixelController.Instance.TicksPerPattern / PixelController.Instance.TicksPerSecond;
        float totalChanges = PixelController.Instance.PixelMovementPerSecond * totalSeconds;
        float smallChange = totalChangeDist / totalChanges;
        if (targetTimeState > alembic.CurrentTime)
            alembic.CurrentTime += smallChange;
        else
            alembic.CurrentTime -= smallChange;
        
//        Debug.Log("pixel has totalChangeDist" +totalChangeDist+ " totalSeconds" +totalSeconds+ " totalChanges" +totalChanges+ " smallChange"+smallChange);
        ApplyColor();
        
   //     alembic.CurrentTime = Mathf.Lerp(alembic.CurrentTime, alembic.CurrentTime +smallChange , 1/PixelController.Instance.PixelMovementPerSecond);

        //lerp towards targetTimeState -  :
        // calculate total amount of seconds available using PixelController.Instance.TicksPerPattern and PixelController.Instance.TicksPerSecond
        // use PixelController.Instance.PixelMovementPerSecond to calculate how many total movements it has
        // divide total distance by that 

    }

    public bool IsFolded()
    {
        float alembicTime = alembic.CurrentTime;

        bool returnValue = false;
        if (PixelController.Instance.tipped == true) // tipped has already been updated before this check
        {
            if (Mathf.Abs(alembicTime - PixelController.Instance.foldState0) <
                Mathf.Abs(alembicTime - PixelController.Instance.foldState1))
                returnValue =  false;
            else
                returnValue =  true;
        }
        else
        {
            if (Mathf.Abs(alembicTime - PixelController.Instance.foldState5) <
                Mathf.Abs(alembicTime - PixelController.Instance.foldState4))
                returnValue =  false;
            else
                returnValue =  true;
            
        }
     //   Debug.Log(returnValue +"pixel has time: " + alembicTime + " abs1: " + Mathf.Abs(alembicTime - PixelController.Instance.foldState0)  + " abs2: " +Mathf.Abs(alembicTime - PixelController.Instance.foldState1) );

        return returnValue;
    }
    
    
    
    
    private Coroutine animateCoroutine;
    public void AnimateFoldState(float targetTippingState, float duration)
    { 
        if (animateCoroutine != null)
            StopCoroutine(animateCoroutine);

        animateCoroutine = StartCoroutine(AnimateFoldStateCoroutine(targetTippingState, duration));
    }

private IEnumerator AnimateFoldStateCoroutine(float targetState, float duration)
{
    float startState = alembic.CurrentTime;
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
        float t = Mathf.Clamp01(elapsedTime / duration);
       alembic.CurrentTime = Mathf.Lerp(startState, targetState, t);
        elapsedTime += Time.deltaTime;
        ApplyColor(); 
        yield return null;
    } 
    alembic.CurrentTime = targetState;
}
   

  

    public void SetFoldState(float state)
    {
//        Debug.Log("Set to " + state);
        alembic.CurrentTime = state;
    }

  

    public float GetFoldState()
    {
        return alembic.CurrentTime;
    }

    public void ApplyColor( )
    {  
        if (applyColor)
        {
            Material materialToApply;

            float currentTime = alembic.CurrentTime;
        
            if (currentTime <= 0.5)
                materialToApply = PixelController.Instance.stateMaterial0;
            else if (currentTime <= 1.5)
                materialToApply = PixelController.Instance.stateMaterial1;
            else if (currentTime <= 2.5)
                materialToApply = PixelController.Instance.stateMaterial2;
            else if (currentTime <= 3.5)
                materialToApply = PixelController.Instance.stateMaterial3;
            else if (currentTime <= 4.5)
                materialToApply = PixelController.Instance.stateMaterial4;
            else
                materialToApply = PixelController.Instance.stateMaterial5;

            if (renderer.material != materialToApply)
                 renderer.material = materialToApply;
        }
        else
        { 
            renderer.material = PixelController.Instance.stateMaterialDefault;
        }
    }
    
    private void OnMouseEnter()
    {
        isHovered = true;
    }

    private void OnMouseExit()
    {
        isHovered = false;
    }

    private void OnMouseDown()
    {
        forceMovement = true;

    }
    private void OnMouseUp()
    {
        forceMovement = false;
    }
    
    
    /*
     
      private void CheckForReverse()
    {
        if (alembic.CurrentTime >= alembic.Duration || alembic.CurrentTime <= 0)
            reversing = !reversing;
    }
     public void Progress(float amount)
    {
        alembic.CurrentTime += amount;
    }

    public void Regress(float amount)
    {
        alembic.CurrentTime -= amount;

    }
    
    
    
    private void HandleForcedMovement()
    {
        float increment = Time.deltaTime * PixelController.Instance.forcedSpeed;
        
        if (reversing)
            Regress(increment);
        else
            Progress(increment);

    } 
    private void HandleDefaultMovement()
    {
        float increment = Time.deltaTime * PixelController.Instance.defaultSpeed;
        
        if (reversing)
            Regress(increment);
        else
            Progress(increment);
    }*/


    
}