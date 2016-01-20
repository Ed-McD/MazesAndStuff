using UnityEngine;
using System.Collections;
using UnityEditor;



public struct stats
{
    
    public int mazeWidth, mazeDepth;
    public float wallThickness, wallHeight;
    bool Prim;
}

public class MazeMenu : EditorWindow
{
    public stats param = new stats();
    public MazeGeneration generator = new MazeGeneration();
    private Vector2 scrollPos;

    [MenuItem("Custom Tools/Maze Generator")]
    private static void OpenWindow()
    {
        EditorWindow.GetWindow<MazeMenu>(typeof(MazeMenu));
    }

    [MenuItem("Tools/Maze Generator", true)]
    private static bool showEditorValidator()
    {
        return true;
    }

   
    void OnGUI()
    {
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        {
            
            param.mazeWidth= EditorGUILayout.IntSlider("Maze Width", param.mazeWidth, 0, 50);

            param.mazeDepth= EditorGUILayout.IntSlider("Maze Depth", param.mazeDepth, 0, 50);
            if ( GUILayout.Button("Generate"))
            {
                generator.Generate(param);
            }
        }
        EditorGUILayout.EndScrollView();        
    }
    
        
}
