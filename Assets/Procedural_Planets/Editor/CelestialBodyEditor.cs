using UnityEngine;
using UnityEditor;
using UnityEngine.Profiling;


[CustomEditor(typeof(CelestialBody))]
public class CelestialBodyEditor : Editor
{
    
    
    CelestialBody celestialBody;
    
    
    private Editor _shapeEditor;
    private Editor _colorEditor;

    
    private bool _shapeFoldout;
    private bool _colorFoldout;


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        using var check = new EditorGUI.ChangeCheckScope();
        if (check.changed)
        {
            Regenerate();
        }

        if (GUILayout.Button("Generate Celestial Body"))
        {
            Profiler.BeginSample("Generate Celestial Body");
            Debug.Log("Generate Celestial Body");
            Regenerate();
            Profiler.EndSample();
        }

        if (GUILayout.Button("Randomize Shading"))
        {
            Debug.Log("Randomize Shading");
        }

        if (GUILayout.Button("Randomize Shape"))
        {
            Debug.Log("Randomize Shape");
        }

        if (GUILayout.Button("Randomize Both"))
        {
            Debug.Log("Randomize Both");
        }


        // Draw shape/shading object editors
        if (
            celestialBody.bodySettings == null || 
            celestialBody.bodySettings.shape == null ||
            celestialBody.bodySettings.shading == null
        ) return;

    DrawSettingsEditor (celestialBody.bodySettings.shape, ref _shapeFoldout, ref _shapeEditor);
        DrawSettingsEditor (celestialBody.bodySettings.shading, ref _colorFoldout, ref _colorEditor);
    }

    
    private void Regenerate()
    {
        celestialBody.OnShapeSettingChanged();
        celestialBody.OnShadingSettingChanged();
        EditorApplication.QueuePlayerLoopUpdate();
    }

    
    void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings == null) return;
        
        foldout = EditorGUILayout.InspectorTitlebar( foldout, settings );
        
        if (!foldout) return;
        CreateCachedEditor(settings, null, ref editor);
        editor.OnInspectorGUI();
    }

    
    private void OnEnable()
    {
        _shapeFoldout = EditorPrefs.GetBool(nameof(_shapeFoldout), false);
        _colorFoldout = EditorPrefs.GetBool(nameof(_colorFoldout), false);
        celestialBody = (CelestialBody) target;
    }

    
    void SaveState()
    {
        EditorPrefs.SetBool(nameof(_shapeFoldout), _shapeFoldout);
        EditorPrefs.SetBool(nameof(_colorFoldout), _colorFoldout);
    }
}
