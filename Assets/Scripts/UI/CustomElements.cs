using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
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
    
     