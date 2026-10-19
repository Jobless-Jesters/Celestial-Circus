// This file was taken from ProGM's gist: https://gist.github.com/ProGM/226204b2a7f99998d84d755ffa1fb39a?permalink_comment_id=3990064

using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class DraggablePoint : PropertyAttribute {}

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviour), true)]
// Draw point in the preview
public class DraggablePointDrawer : Editor {

  readonly GUIStyle style = new GUIStyle();

  void OnEnable(){ // Initialize serialized field
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = Color.white;
  }

  public void OnSceneGUI () { // Defines how it looks in the preview
    var property = serializedObject.GetIterator ();
    while (property.Next (true)) {
      if (property.propertyType == SerializedPropertyType.Vector3) {
        // serializedObject is 
        var field = serializedObject.targetObject.GetType ().GetField (property.name); // Get point name
        if (field == null) {
          continue;
        }
        var draggablePoints = field.GetCustomAttributes (typeof(DraggablePoint), false);
        if (draggablePoints.Length > 0) {
          Handles.Label(property.vector3Value, property.name); // Make label for preview
          property.vector3Value = Handles.PositionHandle (property.vector3Value, Quaternion.identity); // Position of the draggable point
          serializedObject.ApplyModifiedProperties (); // Apply changes made
        }
      }
    }
  }
}
#endif