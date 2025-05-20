using UnityEditor;
using UnityEngine;
using Resources.Scripts.GameManagers;

[CustomEditor(typeof(CharacterScaleManager))]
public class CharacterScaleManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8);
        if (GUILayout.Button("Apply Scales To Scene"))
        {
            var mgr = (CharacterScaleManager)target;
            mgr.ApplyScalesToScene();
        }
    }
}