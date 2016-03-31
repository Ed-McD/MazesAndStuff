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
    public int dBPathLength;
    public AnimationCurve dBCurve;
}

public class MazeMenu : EditorWindow
{
    private Vector2 scrollPos;
    public stats param = new stats() {useBias = false,
        wallMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"),
        floorMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat")
    };
    public MazeGeneration generator = new MazeGeneration();    
    GUIContent biasTooltip;
    GUIContent wallRemovalTT;
    GUIContent doorwayTT;
    GUIStyle headerStyle = new GUIStyle();
    AnimBool showBiasFields = new AnimBool();
    

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

    void OnEnable()
    {
        biasTooltip = new GUIContent("Bias Value", "Positive values bias towards horizontal, negative towards vertical");
        wallRemovalTT = new GUIContent("Remove Selected Walls", 
            "Remove walls of a base layout that are selected in the scene window. \nCells either side of the wall can't be connected to the rest of the maze");
        doorwayTT = new GUIContent("Designate Doorway",
            "Remove walls of a base layout that are selected in the scene window. \nCells either side of the wall are able to be connected to the maze");

        headerStyle = new GUIStyle();
        showBiasFields = new AnimBool();
        generator = new MazeGeneration();        
        param = new stats()
        {
            dBCurve = new AnimationCurve(),
            useBias = false,
            wallMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat"),
            floorMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat")
        };
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

#pragma warning disable 0618
            param.wallMat = (Material)EditorGUILayout.ObjectField("Wall Material", param.wallMat, typeof(Material));

            param.floorMat = (Material)EditorGUILayout.ObjectField("Floor Material", param.floorMat, typeof(Material));
#pragma warning restore 0618

            EditorGUILayout.BeginHorizontal();
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

            if (GUILayout.Button(wallRemovalTT))
            {
                generator.removeSelectedWalls();
            }
            if (GUILayout.Button(doorwayTT))
            {
                generator.createDoorway();
            }

            EditorGUILayout.Space();
                        
            EditorGUILayout.LabelField("Maze Specific Parmaters", headerStyle);

            param.generationMethod = (mazeType)EditorGUILayout.EnumPopup("Generation Method", param.generationMethod);
                       
            param.useBias = EditorGUILayout.Toggle("Use Bias", param.useBias);  

            if (EditorGUILayout.BeginFadeGroup(showBiasFields.faded))
            {
                param.bias = EditorGUILayout.Slider(biasTooltip, param.bias, -1.0f, 1.0f);
                param.dynamicBias = EditorGUILayout.Toggle("Dynamic Bias", param.dynamicBias);
                param.dBPathLength = EditorGUILayout.IntSlider("Dynamic Path Length", param.dBPathLength, 1, 5);
                param.dBCurve = EditorGUILayout.CurveField("Dyanmic Bias Curve", param.dBCurve);
            }
            EditorGUILayout.EndFadeGroup();

            if (GUILayout.Button("Generate With Rooms"))
            {
                generator.GenerateWithRooms(param);
            }

            if ( GUILayout.Button("Generate New"))
            {
                generator.Generate(param);
            }

            
           
        }
        EditorGUILayout.EndScrollView();        
    }
    
        
}
