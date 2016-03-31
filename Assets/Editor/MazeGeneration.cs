using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//Enum defining directions for ease.
public enum direction
{
    LEFT, RIGHT, UP, DOWN,
}

//position of wall and direction to rotate it.
public struct wallData
{
    public bool isHorizontal;
    public Vector2 position;
    
}

//The base cell. Needs a list of cell labels, which contain cells, so this must be a class.
public class mazeCell
{
    public int index;
    public int ID;    
    public bool inMaze;
    public bool wallUp, wallDown, wallLeft, wallRight;
    public intVector2 gridPos;
    public List<cellLabel> adjacentCells = new List<cellLabel>();
    public List<cellLabel> inNeighbours = new List<cellLabel>();
    public GameObject goWallUp, goWallDown, goWallLeft, goWallRight;
    public bool canConnect;

}

//Used to label a neighbouring cell with a direction
public struct cellLabel
{
    public mazeCell cell;
    public direction dir;
}

//struct used for holding grid positions
public struct intVector2
{
    public int x;
    public int y;
}

[ExecuteInEditMode]
public class MazeGeneration {

    GameObject wallWorldHolder;
    GameObject floorWorldHolder; 
    GameObject fullMaze;

    float biasToUse;
    stats _stats;
    List<mazeCell> maze = new List<mazeCell>();
    List<wallData> wallDataHolder = new List<wallData>();


    //Construct Basic Layout
    public void constructBase(stats _params)
    {
        _stats = _params;        
        createCells();
        drawMaze();
    }

    public void GenerateWithRooms(stats _params)
    {
        int tempD = _stats.mazeDepth, tempW = _stats.mazeWidth;
        _stats = _params;
        _stats.mazeDepth = tempD; _stats.mazeWidth = tempW;
        switch (_stats.generationMethod)
        {
            case mazeType.PRIMS:
                {
                    generatePrims();
                    break;
                }
            case mazeType.KRUSKALS:
                {
                    generateKruskals();
                    break;
                }
            case mazeType.RANDOM:
                {
                    if (Random.Range(0, 3) % 2 == 0)
                    {
                        generatePrims();
                    }
                    else
                    {
                        generateKruskals();
                    }
                    break;
                }
            case mazeType.BACKTRACKER:
                {
                    generateBackTracker();
                    break;
                }

        }
        clearWallsFromWorld();
        drawMaze();

    }

    //Main function
    public void Generate(stats _params)
    {
        Debug.Log("Generating...");
        _stats = _params;
        createCells();

        //Chooses algorithm function
        switch (_stats.generationMethod)
        {
            case mazeType.PRIMS:
                {
                    generatePrims();
                    break;
                }
            case mazeType.KRUSKALS:
                {
                    generateKruskals();
                    break;
                }
            case mazeType.RANDOM:
                {
                    if (Random.Range(0,3) %2 ==0 )
                    {
                        generatePrims();
                    }
                    else
                    {
                        generateKruskals();
                    }
                    break;
                }
            case mazeType.BACKTRACKER:
                {
                    generateBackTracker();
                    break;
                }

        }

        drawMaze();

    }

    //Initialise a maze of cells
    void createCells()
    {

        //Clear out the last maze
        maze.Clear();
        int cellCount = 0;
        for (int i = 1; i <= _stats.mazeWidth; i++)
        {
            for (int j = 1; j <= _stats.mazeDepth; j++)
            {
                maze.Add(new mazeCell() { index = i, ID = cellCount, gridPos = { x = j, y = i }, inMaze = false,
                    wallUp = true, wallDown = true, wallLeft = true, wallRight = true, canConnect = true});
                cellCount++;
            }
        }
        foreach (mazeCell c in maze)
        {
            getNeighbours(c);
        }  
    }

    //Define the cells neighbours
    void getNeighbours(mazeCell _cell)
    {
        if (_cell.gridPos.x > 1)
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x - 1, _cell.gridPos.y), dir = direction.LEFT });
        }
        if (_cell.gridPos.x < _stats.mazeDepth )
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x + 1, _cell.gridPos.y), dir = direction.RIGHT });
        }
        if (_cell.gridPos.y > 1)
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x, _cell.gridPos.y - 1), dir = direction.DOWN });
        }
        if (_cell.gridPos.y < _stats.mazeWidth)
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x, _cell.gridPos.y + 1), dir = direction.UP });
        }
    }

    //Return cell in maz list from grid position co-ords
    mazeCell getCellAt(int _x, int _y)
    {

        //int whatis = ((_y - 1) * _stats.mazeDepth) + _x - 1; //For debugging.
        //Debug.Log(whatis);
        return (maze[((_y - 1) * _stats.mazeDepth) + _x - 1]); //Convert given grid position to list position.
    }

    //Remove the last maze created from the scene
    public void clearWallsFromWorld()
    {
        if (fullMaze != null)
        {
            GameObject.DestroyImmediate(fullMaze.gameObject);
        }
        
    }

    private void createWall(wallData wd, mazeCell _cell, direction _cellDir)
    {
        if (!wallDataHolder.Contains(wd))
        {
            GameObject wallObject;
            wallDataHolder.Add(wd);

            wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallObject.GetComponent<Renderer>().material = _stats.wallMat;
            wallObject.name = ("Wall " + wd.position.x + ", " + wd.position.y);
            wallObject.transform.position = new Vector3(wd.position.x, _stats.wallHeight / 2, wd.position.y);

            if (wd.isHorizontal)
            {
                wallObject.transform.localScale = new Vector3(_stats.wallThickness, _stats.wallHeight, 1 + _stats.wallThickness);
            }
            else
            {
                wallObject.transform.localScale = new Vector3(1 + _stats.wallThickness, _stats.wallHeight, _stats.wallThickness);
            }
            wallObject.transform.parent = wallWorldHolder.transform;       
            wallWorldHolder.name = "Walls";
            
            switch(_cellDir)
            {
                case direction.LEFT:
                    _cell.goWallRight = wallObject;
                    if (returnCellInDir(_cell, direction.RIGHT)!= _cell)
                    {
                        returnCellInDir(_cell, direction.RIGHT).goWallLeft = wallObject;
                    }

                    break;
                case direction.RIGHT:
                    _cell.goWallLeft = wallObject;
                    
                    if (returnCellInDir(_cell, direction.LEFT) != _cell)
                    {
                        returnCellInDir(_cell, direction.LEFT).goWallRight = wallObject;
                    }

                    break;
                case direction.UP:
                    _cell.goWallDown = wallObject;
                    
                    if (returnCellInDir(_cell, direction.DOWN) != _cell)
                    {
                        returnCellInDir(_cell, direction.DOWN).goWallUp = wallObject;
                    }

                    break;
                case direction.DOWN:
                    _cell.goWallUp = wallObject;
                    
                    if (returnCellInDir(_cell, direction.UP) != _cell)
                    {
                        returnCellInDir(_cell, direction.UP).goWallDown = wallObject;
                    }

                    break;

            }            
        }        
    }

    //Create objects to make a visual representation of the maze.
    public void drawMaze()
    {
        wallDataHolder.Clear();
        wallWorldHolder = new GameObject();
              
        fullMaze = new GameObject();
        wallData wd;    

        foreach (mazeCell c in maze)
        {
            
            if (c.wallLeft && !c.goWallLeft)
            {
                wd = new wallData() { position = new Vector2(c.gridPos.x - 0.5f, c.gridPos.y), isHorizontal = true };
                createWall(wd, c, direction.RIGHT);       

            }
            if (c.wallRight && !c.goWallRight)
            {
                wd = new wallData() {position = new Vector2(c.gridPos.x + 0.5f, c.gridPos.y), isHorizontal = true };
                createWall(wd, c,direction.LEFT);
               
            }
            if (c.wallUp && !c.goWallUp)
            {
                wd = new wallData() { position = new Vector2(c.gridPos.x, c.gridPos.y + 0.5f), isHorizontal = false };
                createWall(wd, c,direction.DOWN);
                
            }
            if (c.wallDown && !c.goWallDown)
            {
                wd = new wallData() { position = new Vector2(c.gridPos.x, c.gridPos.y - 0.5f), isHorizontal = false };
                createWall(wd, c,direction.UP);
                
            }         
        }


        floorWorldHolder = GameObject.CreatePrimitive(PrimitiveType.Quad);        
        floorWorldHolder.GetComponent<Renderer>().material = _stats.floorMat;
        floorWorldHolder.transform.position = new Vector3((_stats.mazeDepth/ 2.0f) +0.5f, 0, (_stats.mazeWidth / 2.0f)+0.5f);
        floorWorldHolder.transform.localScale = new Vector3(_stats.mazeDepth, _stats.mazeWidth, 1);
        floorWorldHolder.transform.Rotate(new Vector3 (90,0,0));
        floorWorldHolder.name = "Floor";

        wallWorldHolder.transform.parent = fullMaze.transform;
        floorWorldHolder.transform.parent = fullMaze.transform;
        fullMaze.name = "Maze";
        fullMaze.transform.localScale *= 10;

    }

    public void createDoorway()
    {
        List<GameObject> selectedWalls = new List<GameObject>();
        foreach (GameObject go in UnityEditor.Selection.gameObjects)
        {
            if (go.name.Contains("Wall"))
            {
                selectedWalls.Add(go);
            }
        }
        foreach (mazeCell cell in maze)
        {
            for (int i = 0; i < selectedWalls.Count; i++)
            {
                if (selectedWalls[i] == cell.goWallLeft)
                {
                    designateDoorways(cell, direction.LEFT);
                }
                else if (selectedWalls[i] == cell.goWallRight)
                {
                    designateDoorways(cell, direction.RIGHT);
                }
                else if (selectedWalls[i] == cell.goWallUp)
                {
                    designateDoorways(cell, direction.UP);
                }
                else if (selectedWalls[i] == cell.goWallDown)
                {
                    designateDoorways(cell, direction.DOWN);
                }
            }
        }
    }

    private void designateDoorways(mazeCell _cell, direction _dir)
    {
        bool temp = true;
        foreach (cellLabel cL in _cell.adjacentCells)
        {
            if (cL.dir == _dir)
            {
                switch (cL.dir)
                {
                    case direction.LEFT:

                        cL.cell.wallRight = false;
                        GameObject.DestroyImmediate(cL.cell.goWallRight);
                        cL.cell.canConnect = temp;
                        _cell.wallLeft = false;
                        GameObject.DestroyImmediate(_cell.goWallLeft);
                        _cell.canConnect = temp;
                        _cell.inMaze = false;
                        cL.cell.inMaze = false;

                        break;
                    case direction.RIGHT:

                        cL.cell.wallLeft = false;
                        GameObject.DestroyImmediate(cL.cell.goWallLeft);
                        cL.cell.canConnect = temp;
                        _cell.wallRight = false;
                        GameObject.DestroyImmediate(_cell.goWallRight);
                        _cell.canConnect = temp;
                        _cell.inMaze = false;
                        cL.cell.inMaze = false;


                        break;
                    case direction.UP:

                        cL.cell.wallDown = false;
                        GameObject.DestroyImmediate(cL.cell.goWallDown);
                        cL.cell.canConnect = temp;
                        _cell.wallUp = false;
                        GameObject.DestroyImmediate(_cell.goWallUp);
                        _cell.canConnect = temp;
                        _cell.inMaze = false;
                        cL.cell.inMaze = false;

                        break;
                    case direction.DOWN:

                        cL.cell.wallUp = false;
                        GameObject.DestroyImmediate(cL.cell.goWallUp);
                        cL.cell.canConnect = temp;
                        _cell.wallDown = false;
                        GameObject.DestroyImmediate(_cell.goWallDown);
                        _cell.canConnect = temp;
                        _cell.inMaze = false;
                        cL.cell.inMaze = false;


                        break;
                }
            }
        }
    }

    // W I P
    public void removeSelectedWalls()
    {
        List<GameObject> selectedWalls = new List<GameObject>();
        foreach(GameObject go in  UnityEditor.Selection.gameObjects)
        {
            if(go.name.Contains("Wall"))
            {
                selectedWalls.Add(go);
            }
        }        
        foreach( mazeCell cell in maze)
        {
            for (int i = 0; i < selectedWalls.Count; i ++)
            {
                if (selectedWalls[i] == cell.goWallLeft)
                {
                    neighbourRemoveObject(cell, direction.LEFT);
                }
                else if(selectedWalls[i] == cell.goWallRight)
                {
                    neighbourRemoveObject(cell, direction.RIGHT);
                }
                else if (selectedWalls[i] == cell.goWallUp)
                {
                    neighbourRemoveObject(cell, direction.UP);
                }
                else if (selectedWalls[i] == cell.goWallDown)
                {
                    neighbourRemoveObject(cell, direction.DOWN);
                }
            }
        }
    }

    public void neighbourRemoveObject(mazeCell _cell, direction _dir)
    {
        bool temp = false;
        foreach (cellLabel cL in _cell.adjacentCells)
        {
            if (cL.dir == _dir)
            {
                switch (cL.dir)
                {
                    case direction.LEFT:

                        cL.cell.wallRight = false;                        
                        GameObject.DestroyImmediate(cL.cell.goWallRight);
                        cL.cell.canConnect = temp;
                        _cell.wallLeft = false;
                        GameObject.DestroyImmediate(_cell.goWallLeft);
                        _cell.canConnect = temp;
                        _cell.inMaze = true;
                        cL.cell.inMaze = true;

                        break;
                    case direction.RIGHT:

                        cL.cell.wallLeft = false;
                        GameObject.DestroyImmediate(cL.cell.goWallLeft);
                        cL.cell.canConnect = temp;
                        _cell.wallRight = false;
                        GameObject.DestroyImmediate(_cell.goWallRight);
                        _cell.canConnect = temp;
                        _cell.inMaze = true;
                        cL.cell.inMaze = true;
                        

                        break;
                    case direction.UP:

                        cL.cell.wallDown = false;
                        GameObject.DestroyImmediate(cL.cell.goWallDown);
                        cL.cell.canConnect = temp;
                        _cell.wallUp = false;
                        GameObject.DestroyImmediate(_cell.goWallUp);
                        _cell.canConnect = temp;
                        _cell.inMaze = true;
                        cL.cell.inMaze = true;

                        break;
                    case direction.DOWN:

                        cL.cell.wallUp = false;
                        GameObject.DestroyImmediate(cL.cell.goWallUp);
                        cL.cell.canConnect = temp;
                        _cell.wallDown = false;
                        GameObject.DestroyImmediate(_cell.goWallDown);
                        _cell.canConnect = temp;
                        _cell.inMaze = true;
                        cL.cell.inMaze = true;


                        break;
                }
            }
        }
    }

    //Function that removes the walls between the cell passed in, and the neighbour in the direction passed in.
    public void neighbourRemove(mazeCell _cell ,direction _dir)
    {
        foreach(cellLabel cL in _cell.adjacentCells)
        {
            if (cL.dir == _dir)
            {
                switch (cL.dir)
                {
                    case direction.LEFT:

                        _cell.wallLeft = false;
                        cL.cell.wallRight = false;

                        break;
                    case direction.RIGHT:
                        cL.cell.wallLeft = false;
                        _cell.wallRight = false;

                        break;
                    case direction.UP:
                        cL.cell.wallDown = false;
                        _cell.wallUp = false;
                        
                        break;
                    case direction.DOWN:
                        cL.cell.wallUp = false;
                        _cell.wallDown = false;

                        break;
                }
            }
        }
    }

    //Debug function for testing maze drawing worked correctly.
    public void everyOther()
    {
        foreach (mazeCell c in maze)
        {
            if(c.gridPos.x % 2 != 0)
            {
                neighbourRemove(c, direction.LEFT);
                neighbourRemove(c, direction.RIGHT);            

            }
            else
            {
               // c.wallUp = false;
               // c.wallDown = false;
            }
        }
    }

    //Get cell in directions
    mazeCell returnCellInDir(mazeCell _cell, direction _dir)
    {
        foreach (cellLabel _cl in _cell.adjacentCells)
        {
            if(_cl.dir == _dir)
            {
                return _cl.cell;
            }
        }
        return _cell;
    }

    // DYNAMIC BIAS  // NEEDS MORE WORK!!
    float calculateDynamicBias(mazeCell _cell)
    {
        
        float _bias = 0;
        mazeCell temp;
        int hCount = 0;
        int vCount = 0;
        float hDecimal, vDecimal;
        

        #region Get Count
        foreach (cellLabel cL in _cell.adjacentCells)
        {
            switch (cL.dir)
            {
                case direction.LEFT:
                    if (cL.cell.wallLeft == false)
                    {
                        hCount++;
                        for (int i = 1; i < _stats.dBPathLength; i++)
                        {
                            temp = returnCellInDir(cL.cell, direction.LEFT);
                            if (temp != cL.cell && !temp.wallLeft)
                            {
                                hCount++;
                            }
                        }                                        
                    }
                   

                    break;
                case direction.RIGHT:
                    if (cL.cell.wallRight == false)
                    {
                        hCount++;
                        for (int i = 1; i < _stats.dBPathLength; i++)
                        {
                            temp = returnCellInDir(cL.cell, direction.RIGHT);
                            if (temp != cL.cell && !temp.wallRight)
                            {
                                hCount++;
                            }
                        }
                    }

                    break;
                case direction.UP:
                    if (cL.cell.wallUp == false)
                    {
                        vCount++;
                        for (int i = 1; i < _stats.dBPathLength; i++)
                        {
                            temp = returnCellInDir(cL.cell, direction.UP);
                            if (temp != cL.cell && !temp.wallUp)
                            {
                                vCount++;
                            }
                        }
                    }

                    break;
                case direction.DOWN:
                    if (cL.cell.wallDown == false)
                    {
                        vCount++;
                        for (int i = 1; i < _stats.dBPathLength; i++)
                        {
                            temp = returnCellInDir(cL.cell, direction.DOWN);
                            if (temp != cL.cell && !temp.wallDown)
                            {
                                vCount++;
                            }
                        }
                    }
                    break;
            }

        }
        #endregion

        hDecimal = (float)hCount / (float)_stats.dBPathLength;
        vDecimal = (float)vCount / (float)_stats.dBPathLength;
        _bias = (hDecimal - vDecimal)/2.0f;
        _bias = evaluateCurve(_bias);

        //Debug.Log("hCount = " + hCount + ", hdecimal = " + hDecimal + "\n vdecimal = " + vDecimal);
        Debug.Log(_bias);
        return _bias;
    }

    float evaluateCurve( float _value)
    {
        return (_stats.dBCurve.Evaluate(_value)); 
    }

    //Kruskals algorithm 
    void generateKruskals()
    {
        List<int> IDLIST = new List<int>(); 

        //shuffle the maze
        for (int i = 0; i < maze.Count; i++)
        {
            mazeCell tempCell = maze[i];
            int randomIndex = Random.Range(i, maze.Count);
            maze[i] = maze[randomIndex];
            maze[randomIndex] = tempCell;
        }

        foreach (mazeCell c in maze)
        {
            if (!IDLIST.Contains(c.ID))
            {
                IDLIST.Add(c.ID);
            }
        }

        while (IDLIST.Count > 1)
        {
            foreach (mazeCell currCell in maze)
            {
                //Shuffle the adjacent cell list
                for (int i = 0; i < currCell.adjacentCells.Count; i++)
                {
                    cellLabel tempCell = currCell.adjacentCells[i];
                    int randomIndex = Random.Range(i, currCell.adjacentCells.Count);
                    currCell.adjacentCells[i] = currCell.adjacentCells[randomIndex];
                    currCell.adjacentCells[randomIndex] = tempCell;

                }

                // RESHUFFLE WITH BIAS
                #region 

                List<cellLabel> horizontals = new List<cellLabel>();
                //List<cellLabel> verticals = new List<cellLabel>();

                if (_stats.useBias)
                {
                    foreach (cellLabel cL in currCell.adjacentCells)
                    {
                        if (cL.dir == direction.LEFT || cL.dir == direction.RIGHT)
                        {
                            horizontals.Add(cL);
                        }
                        if (cL.dir == direction.UP || cL.dir == direction.DOWN)
                        {

                        }
                    }
                    int hzShuffCount = 0;
                    int vtShuffCount = 0;
                    biasToUse = _stats.bias;

                    if (_stats.dynamicBias)
                    {
                        biasToUse = calculateDynamicBias(currCell);
                    }

                    for (int i = 0; i < currCell.adjacentCells.Count; i++)
                    {
                        cellLabel tempCell = currCell.adjacentCells[i];
                        int index;
                        if (horizontals.Contains(tempCell))
                        {
                            if (Random.Range(-1.0f, 1.0f) < biasToUse)
                            {
                                index = hzShuffCount;
                                currCell.adjacentCells[i] = currCell.adjacentCells[index];
                                currCell.adjacentCells[index] = tempCell;
                            }

                            hzShuffCount++;
                        }
                        else
                        {
                            if (Random.Range(-1.0f, 1.0f) > biasToUse)
                            {
                                index = vtShuffCount;
                                currCell.adjacentCells[i] = currCell.adjacentCells[index];
                                currCell.adjacentCells[index] = tempCell;
                            }

                            vtShuffCount++;
                        }

                    }
                }
                #endregion

                cellLabel labeledInCell = currCell.adjacentCells[0];

                //Cycle through shuffled list for first neighbour already in maze
                for (int i = 0; i < currCell.adjacentCells.Count; i++)
                {
                    labeledInCell = currCell.adjacentCells[i];
                    if (labeledInCell.cell.ID != currCell.ID)
                    {
                        i = currCell.adjacentCells.Count;
                    }
                }

                if (labeledInCell.cell.ID == currCell.ID)
                {
                    continue;
                }

                currCell.inMaze = true;
                neighbourRemove(currCell, labeledInCell.dir);
                int tempInt = labeledInCell.cell.ID;
                foreach(int i in IDLIST)
                {
                    if(i == tempInt)
                    {
                        foreach(mazeCell c in maze)
                        {
                            if(c.ID == tempInt)
                            {
                                c.ID = currCell.ID;
                            }
                        }
                        IDLIST.Remove(i);
                        break;
                    }
                }
            }
        }        
    }

    //Prims Algorithm
    void generatePrims()
    {

        List<mazeCell> inCells = new List<mazeCell>();
        List<mazeCell> frontierCells = new List<mazeCell>();
        List<mazeCell> outCells = new List<mazeCell>();
        mazeCell currCell;
        cellLabel labeledInCell;

        foreach (mazeCell c in maze)
        {
            outCells.Add(c);
        }


        //Pick a starting cell...
        currCell = outCells[Random.Range(0, outCells.Count)];

        //...add it to the maze...
        currCell.inMaze = true;
        inCells.Add(currCell);
        outCells.Remove(currCell);

        //...and make its neighbours frontier cells
        foreach (cellLabel c in currCell.adjacentCells)
        {
            if (!frontierCells.Contains(c.cell))
            {
                frontierCells.Add(c.cell);
                outCells.Remove(c.cell);

            }
        }


        while (frontierCells.Count > 0)
        {
            currCell = frontierCells[Random.Range(0, frontierCells.Count)];
            labeledInCell = currCell.adjacentCells[0];

            //Shuffle the adjacent cell list
            for (int i = 0; i < currCell.adjacentCells.Count; i++)
            {
                cellLabel tempCell = currCell.adjacentCells[i];
                int randomIndex = Random.Range(i, currCell.adjacentCells.Count);
                currCell.adjacentCells[i] = currCell.adjacentCells[randomIndex];
                currCell.adjacentCells[randomIndex] = tempCell;

            }

            // RESHUFFLE WITH BIAS
            #region 

            List<cellLabel> horizontals = new List<cellLabel>();
            //List<cellLabel> verticals = new List<cellLabel>();

            if (_stats.useBias)
            {
                foreach (cellLabel cL in currCell.adjacentCells)
                {
                    if (cL.dir == direction.LEFT || cL.dir == direction.RIGHT)
                    {
                        horizontals.Add(cL);
                    }
                    if (cL.dir == direction.UP || cL.dir == direction.DOWN)
                    {

                    }
                }
                int hzShuffCount = 0;
                int vtShuffCount = 0;
                biasToUse = _stats.bias;

                if (_stats.dynamicBias)
                {
                    biasToUse = calculateDynamicBias(currCell);
                }


                for (int i = 0; i < currCell.adjacentCells.Count; i++)
                {
                    cellLabel tempCell = currCell.adjacentCells[i];
                    int index;
                    if (horizontals.Contains(tempCell))
                    {
                        if (Random.Range(-1.0f, 1.0f) < biasToUse)
                        {
                            index = hzShuffCount;
                            currCell.adjacentCells[i] = currCell.adjacentCells[index];
                            currCell.adjacentCells[index] = tempCell;
                        }

                        hzShuffCount++;
                    }
                    else
                    {
                        if (Random.Range(-1.0f, 1.0f) >biasToUse)
                        {
                            index = vtShuffCount;
                            currCell.adjacentCells[i] = currCell.adjacentCells[index];
                            currCell.adjacentCells[index] = tempCell;
                        }

                        vtShuffCount++;
                    }

                }
            }
            #endregion

            //Cycle through shuffled list for first neighbour already in maze
            for (int i = 0; i < currCell.adjacentCells.Count; i++)
            {
                labeledInCell = currCell.adjacentCells[i];
                if (labeledInCell.cell.inMaze)
                {
                    i = currCell.adjacentCells.Count;
                }
            }


            currCell.inMaze = true;
            inCells.Add(currCell);
            frontierCells.Remove(currCell);

            neighbourRemove(currCell, labeledInCell.dir);            

            foreach (cellLabel c in currCell.adjacentCells)
            {
                if (!(frontierCells.Contains(c.cell)) && outCells.Contains(c.cell))
                {
                    frontierCells.Add(c.cell);
                    outCells.Remove(c.cell);
                }
            }



        }

        maze = inCells;
        Debug.Log("MAZE MADE");

    }

    //Recursive Backtracker Algorithm
    void generateBackTracker()
    {
        //Shuffle the maze
        for (int i = 0; i < maze.Count; i++)
        {
            mazeCell tempCell = maze[i];
            int randomIndex = Random.Range(i, maze.Count);
            maze[i] = maze[randomIndex];
            maze[randomIndex] = tempCell;
        }


        List<mazeCell> path = new List<mazeCell>();

        cellLabel labeledInCell;
        //mazeCell startingCell;
        mazeCell currCell;
        currCell = maze[0];
        //startingCell = maze[0];

        path.Add(currCell);

        while (path.Count > 0)
        {
            //Shuffle the adjacent cell list
            for (int i = 0; i < currCell.adjacentCells.Count; i++)
            {
                cellLabel tempCell = currCell.adjacentCells[i];
                int randomIndex;                
                randomIndex = Random.Range(i, currCell.adjacentCells.Count);
                currCell.adjacentCells[i] = currCell.adjacentCells[randomIndex];
                currCell.adjacentCells[randomIndex] = tempCell;

            }


            // RESHUFFLE WITH BIAS
            #region 

            List<cellLabel> horizontals = new List<cellLabel>();
            //List<cellLabel> verticals = new List<cellLabel>();

            if (_stats.useBias)
            {
                foreach (cellLabel cL in currCell.adjacentCells)
                {
                    if (cL.dir == direction.LEFT || cL.dir == direction.RIGHT)
                    {
                        horizontals.Add(cL);
                    }
                    if(cL.dir ==direction.UP || cL.dir == direction.DOWN)
                    {

                    }
                }
                int hzShuffCount = 0;
                int vtShuffCount = 0;
                biasToUse = _stats.bias;
                 
                if(_stats.dynamicBias)
                {
                    biasToUse = calculateDynamicBias(currCell);
                }

                for (int i = 0; i < currCell.adjacentCells.Count; i++)
                {
                    
                    cellLabel tempCell = currCell.adjacentCells[i];
                    int index;
                    if (horizontals.Contains(tempCell))
                    {
                        if (Random.Range(-1.0f, 1.0f) < biasToUse)
                        {
                            index = hzShuffCount;
                            currCell.adjacentCells[i] = currCell.adjacentCells[index];
                            currCell.adjacentCells[index] = tempCell;
                        }                        
                        
                        hzShuffCount++;
                    }
                    else
                    {
                        if (Random.Range(-1.0f, 1.0f) > biasToUse)
                        {
                            index = vtShuffCount;
                            currCell.adjacentCells[i] = currCell.adjacentCells[index];
                            currCell.adjacentCells[index] = tempCell;
                        }

                        vtShuffCount++;
                    }
                    
                }
            }
            #endregion


            //This line is just to stop VS having a fit about something being possible unassigned.
            labeledInCell = currCell.adjacentCells[0];    

            //Cycle through shuffled list for first neighbour already in maze
            for (int i = 0; i < currCell.adjacentCells.Count; i++)
            {
                labeledInCell = currCell.adjacentCells[i];
                if (!labeledInCell.cell.inMaze)
                {
                    i = currCell.adjacentCells.Count;
                }
            }
            
            //If the adjacent cell assigned isn't in the maze yet, add it and make it the current cell
            if(!labeledInCell.cell.inMaze) 
            {
                neighbourRemove(currCell, labeledInCell.dir);

                currCell = labeledInCell.cell;
                currCell.inMaze = true;
                path.Add(currCell);


            }
            else
            {
                path.RemoveAt(path.Count - 1);
                if (path.Count > 0)
                {
                    currCell = path[path.Count - 1];
                }
            }
        }
    }
}
