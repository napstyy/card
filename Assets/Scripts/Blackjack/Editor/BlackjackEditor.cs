// MyBehaviourEditor.cs
using UnityEngine;
using UnityEditor;
using CardGame;

[CustomEditor(typeof(BlackjackController))]
public class BlackjackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add a button to the Inspector
        BlackjackController myScript = (BlackjackController)target;
        if (GUILayout.Button("Get Random Rules"))
        {
            myScript.GetRandomRules(); // Call the function
        }
    }
}
