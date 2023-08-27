using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

 
[CustomEditor(typeof(PixelController))]
public class PixelControllerEditor : Editor
{
 
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();


            if (GUILayout.Button("Spawn Pixels"))
            {
                PixelController controller = target as PixelController;
                
                controller.SpawnPixels();
            }
          
        }
    
}
