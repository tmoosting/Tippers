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
    
    private bool reversing = false;
    private bool forceMovement = false;
    private bool colorApplied = false;
    
    private void Update()
    {
        if (forceMovement)
          HandleForcedMovement();
        else
            HandleDefaultMovement();

        CheckForReverse();

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
        
    }
    private Coroutine animateCoroutine;

    public void AnimateTippingState(float targetTippingState, float duration)
    {
        if (animateCoroutine != null)
        {
            StopCoroutine(animateCoroutine);
        }

        animateCoroutine = StartCoroutine(AnimateTippingStateCoroutine(targetTippingState, duration));
    }

private IEnumerator AnimateTippingStateCoroutine(float targetState, float duration)
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

    public void SetTippingState(float state)
    {
//        Debug.Log("Set to " + state);
        alembic.CurrentTime = state;
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

    public float GetTippingState()
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

}