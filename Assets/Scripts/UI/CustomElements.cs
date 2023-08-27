using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.UIElements;

public class CustomElements : MonoBehaviour
{
    
  
}



  
/// <summary>
/// TextField with no border and color background
/// </summary>
public class BlueTextField : TextField
{
    private ControlUI controlUI;
    public BlueTextField( ControlUI controlUI )
    {
        this.controlUI = controlUI;
        VisualElement textBox = this.Q("unity-text-input");
        textBox.style.backgroundColor = controlUI.textFieldColor; 
        textBox.style.borderBottomWidth = 0f;
        textBox.style.borderTopWidth = 0f;
        textBox.style.borderRightWidth = 0f;
        textBox.style.borderLeftWidth = 0f;
    } 
}
    
    
public class FrontLabel : VisualElement
{
    private ControlUI controlUI;
    public Label label;
         
    public FrontLabel(ControlUI controlUI, bool anchorRight ,string textValue, VisualElement secondElement)
    {
        this.controlUI = controlUI;

        secondElement.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        if (anchorRight)
            style.justifyContent = Justify.SpaceBetween;
        else
            style.justifyContent = Justify.FlexStart;

        style.alignItems = Align.Center;
    
        label = new Label(textValue);
        label.style.unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold);
        this.Add(label); 
        EditorCoroutineUtility.StartCoroutineOwnerless(SetMarginAfterDelay(secondElement));
        
        this.Add(secondElement);
    }
    private IEnumerator SetMarginAfterDelay(VisualElement secondElement)
    {
        yield return new EditorWaitForSeconds(0.01f); 
        float margin = 80 - label.resolvedStyle.width;
        secondElement.style.marginLeft =  margin;
        secondElement.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
    }
    
}