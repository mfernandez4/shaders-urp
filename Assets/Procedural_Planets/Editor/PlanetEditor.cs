using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor
{
    Planet planet;
    
    
    Editor shapeEditor;
    Editor colorEditor;
    
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        using var check = new EditorGUI.ChangeCheckScope();
        if (check.changed)
        {
            planet.GeneratePlanet();
        }
        
        if (GUILayout.Button("Generate Planet"))
        {
            planet.GeneratePlanet();
        }

        DrawSettingsEditor(planet.settingsShape, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFoldout, ref shapeEditor);
        DrawSettingsEditor(planet.settingsColor, planet.OnColorSettingsUpdated , ref planet.colorSettingsFoldout, ref colorEditor);
    }
    
    void DrawSettingsEditor(Object settings, System.Action OnSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings == null) return;
        // Create a foldout for the settings
        foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
        
        using var check = new EditorGUI.ChangeCheckScope();
        
        // If the foldout is not open, return
        if (!foldout) return;
        CreateCachedEditor(settings, null, ref editor);
        editor.OnInspectorGUI();
        if (check.changed)
        {
            OnSettingsUpdated?.Invoke();
        }
    }
    
    private void OnEnable()
    {
        planet = (Planet) target;
    }
}
