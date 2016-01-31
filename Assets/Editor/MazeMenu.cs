using UnityEngine;
using System.Collections;
using UnityEditor;

public enum mazeType
{
    BACKTRACKER,PRIMS,KRUSKALS,RANDOM,
}

public struct stats
{
    
    public int mazeWidth, mazeDepth;
    public float wallThickness, wallHeight;
    public mazeType generationMethod;
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
            
            param.mazeWidth= EditorGUILayout.IntSlider("Maze Width", param.mazeWidth, 5, 50);

            param.mazeDepth= EditorGUILayout.IntSlider("Maze Depth", param.mazeDepth, 5, 50);

            param.wallHeight = EditorGUILayout.Slider("Wall Height", param.wallHeight, 0.5f, 3);

            param.wallThickness = EditorGUILayout.Slider("Wall Thickness", param.wallThickness, 0.01f, 0.25f);

            param.generationMethod = (mazeType)EditorGUILayout.EnumPopup("Generation Method", param.generationMethod);
            
            if ( GUILayout.Button("Generate"))
            {
                generator.Generate(param);
            }
            if (GUILayout.Button("ClearWall"))
            {
                generator.clearWallsFromWorld();
            }
            //if (GUILayout.Button("MAZE IT UP!!"))
            //{
            //    generator.generatePrims();
            //}
            //if(GUILayout.Button("SHOW THAT MAZE!!"))
            //{
            //    generator.drawMaze();
            //}
            //if (GUILayout.Button("testWalls"))
            //{
            //    generator.everyOther();
            //}
        }
        EditorGUILayout.EndScrollView();        
    }
    
        
}
