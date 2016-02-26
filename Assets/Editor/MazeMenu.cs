using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.AnimatedValues;

public enum mazeType
{
    BACKTRACKER,PRIMS,KRUSKALS,RANDOM,
}

public struct stats
{
    
    public int mazeWidth, mazeDepth;
    public float wallThickness, wallHeight;
    public mazeType generationMethod;
    public Material wallMat,floorMat;
    public bool useBias;
    public bool dynamicBias;
    public float bias; 
}

public class MazeMenu : EditorWindow
{
    public stats param = new stats() {
        wallMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"),
        floorMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat")
    };
    public MazeGeneration generator = new MazeGeneration();
    private Vector2 scrollPos;
    GUIContent tooltip = new GUIContent("Bias Value", "Positive values bias towards horizontal, negative towards ertical");
    GUIStyle headerStyle = new GUIStyle();

    AnimBool showBiasFields;
    

    
    

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
        showBiasFields.target = param.useBias;
        showBiasFields.valueChanged.AddListener(Repaint);
        headerStyle.fontStyle = FontStyle.Bold;
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        {
            EditorGUILayout.LabelField("Basic Layout Parameters", headerStyle);

            param.mazeWidth= EditorGUILayout.IntSlider("Maze Depth", param.mazeWidth, 5, 50);

            param.mazeDepth= EditorGUILayout.IntSlider("Maze Width", param.mazeDepth, 5, 50);

            param.wallHeight = EditorGUILayout.Slider("Wall Height", param.wallHeight, 0.5f, 3);

            param.wallThickness = EditorGUILayout.Slider("Wall Thickness", param.wallThickness, 0.01f, 0.25f);

            param.wallMat = (Material)EditorGUILayout.ObjectField("Wall Material", param.wallMat, typeof(Material));

            param.floorMat = (Material)EditorGUILayout.ObjectField("Floor Material", param.floorMat, typeof(Material));

            Rect r = EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Construct Base Layout"))
                {
                    generator.constructBase(param);
                }
                if (GUILayout.Button("Clear Last Maze"))
                {
                    generator.clearWallsFromWorld();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
                        
            EditorGUILayout.LabelField("Maze Specific Parmaters", headerStyle);

            param.generationMethod = (mazeType)EditorGUILayout.EnumPopup("Generation Method", param.generationMethod);
                       
            param.useBias = EditorGUILayout.Toggle("Use Bias", param.useBias);  

            if (EditorGUILayout.BeginFadeGroup(showBiasFields.faded))
            {
                param.dynamicBias = EditorGUILayout.Toggle("Dynamic Bias", param.dynamicBias);
                param.bias = EditorGUILayout.Slider(tooltip, param.bias, -1.0f, 1.0f);
            }
            EditorGUILayout.EndFadeGroup();             

            if ( GUILayout.Button("Generate"))
            {
                generator.Generate(param);
            }

            
           
        }
        EditorGUILayout.EndScrollView();        
    }
    
        
}
