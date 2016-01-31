using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum direction
{
    LEFT, RIGHT, UP, DOWN,
}
public struct wallData
{
    public bool isHorizontal;
    public Vector2 position;
}

public class mazeCell
{
    public int index;
    public int ID;    
    public bool inMaze;
    public bool wallUp, wallDown, wallLeft, wallRight;
    public intVector2 gridPos;
    public List<cellLabel> adjacentCells = new List<cellLabel>();
    public List<cellLabel> inNeighbours = new List<cellLabel>();

}

public struct cellLabel
{
    public mazeCell cell;
    public direction dir;
}

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


    stats _stats;
    List<mazeCell> maze = new List<mazeCell>();
    List<wallData> wallDataHolder = new List<wallData>();

    public void Generate(stats _params)
    {
        Debug.Log("Generating...");
        _stats = _params;
        createCells();

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

    void createCells()
    {
        maze.Clear();
        int cellCount = 0;
        for (int i = 1; i <= _stats.mazeWidth; i++)
        {
            for (int j = 1; j <= _stats.mazeDepth; j++)
            {
                maze.Add(new mazeCell() { index = i, ID = cellCount, gridPos = { x = j, y = i }, inMaze = false,
                    wallUp = true, wallDown = true, wallLeft = true, wallRight = true });
                cellCount++;
            }
        }
        foreach (mazeCell c in maze)
        {
            getNeighbours(c);
        }  
    }

    void getNeighbours(mazeCell _cell)
    {
        if (_cell.gridPos.x > 1)
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x - 1, _cell.gridPos.y), dir = direction.LEFT });
        }
        if (_cell.gridPos.x < _stats.mazeWidth )
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x + 1, _cell.gridPos.y), dir = direction.RIGHT });
        }
        if (_cell.gridPos.y > 1)
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x, _cell.gridPos.y - 1), dir = direction.DOWN });
        }
        if (_cell.gridPos.y < _stats.mazeDepth)
        {
            _cell.adjacentCells.Add(new cellLabel() { cell = getCellAt(_cell.gridPos.x, _cell.gridPos.y + 1), dir = direction.UP });
        }
    }

    mazeCell getCellAt(int _x, int _y)
    {

        int whatis = ((_y - 1) * _stats.mazeDepth) + _x - 1;
        return (maze[((_y - 1) * _stats.mazeDepth) + _x - 1]);
    }

    public void clearWallsFromWorld()
    {
        GameObject.DestroyImmediate(fullMaze.gameObject);
    }

    public void drawMaze()
    {
        wallDataHolder.Clear();
        wallWorldHolder = new GameObject();
              
        fullMaze = new GameObject();

        wallData tempWallData;
        GameObject wallObject;
       

        foreach (mazeCell c in maze)
        {
            if (c.wallLeft)
            {
                tempWallData = new wallData() { position = new Vector2(c.gridPos.x - 0.5f, c.gridPos.y), isHorizontal = true };
                if (!wallDataHolder.Contains(tempWallData))
                {
                    wallDataHolder.Add(tempWallData);
                }
            }
            if (c.wallRight)
            {
                tempWallData = new wallData() { position = new Vector2(c.gridPos.x + 0.5f, c.gridPos.y), isHorizontal = true };
                if (!wallDataHolder.Contains(tempWallData))
                {
                    wallDataHolder.Add(tempWallData);
                }
            }
            if (c.wallUp)
            {
                tempWallData = new wallData() { position = new Vector2(c.gridPos.x, c.gridPos.y + 0.5f), isHorizontal = false };
                if (!wallDataHolder.Contains(tempWallData))
                {
                    wallDataHolder.Add(tempWallData);
                }
            }
            if (c.wallDown)
            {
                tempWallData = new wallData() { position = new Vector2(c.gridPos.x, c.gridPos.y - 0.5f), isHorizontal = false };
                if (!wallDataHolder.Contains(tempWallData))
                {
                    wallDataHolder.Add(tempWallData);
                }
            }
        }

        foreach (wallData wd in wallDataHolder)
        {
            wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallObject.GetComponent<Renderer>().material = _stats.wallMat;
            wallObject.name = ("Wall " + wd.position.x + ", " + wd.position.y);
            wallObject.transform.position = new Vector3(wd.position.x, _stats.wallHeight /2, wd.position.y);

            if (wd.isHorizontal)
            {
                wallObject.transform.localScale = new Vector3(_stats.wallThickness, _stats.wallHeight, 1+ _stats.wallThickness);
            }
            else
            {
                wallObject.transform.localScale = new Vector3(1+ _stats.wallThickness, _stats.wallHeight, _stats.wallThickness);
            }
            wallObject.transform.parent = wallWorldHolder.transform;

            wallWorldHolder.name = "Walls";
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
            IDLIST.Add(c.ID);
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
                List<cellLabel> verticals = new List<cellLabel>();

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
                    for (int i = 0; i < currCell.adjacentCells.Count; i++)
                    {
                        cellLabel tempCell = currCell.adjacentCells[i];
                        int index;
                        if (horizontals.Contains(tempCell))
                        {
                            if (Random.Range(-1.0f, 1.0f) < _stats.bias)
                            {
                                index = hzShuffCount;
                                currCell.adjacentCells[i] = currCell.adjacentCells[index];
                                currCell.adjacentCells[index] = tempCell;
                            }

                            hzShuffCount++;
                        }
                        else
                        {
                            if (Random.Range(-1.0f, 1.0f) > _stats.bias)
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
            List<cellLabel> verticals = new List<cellLabel>();

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
                for (int i = 0; i < currCell.adjacentCells.Count; i++)
                {
                    cellLabel tempCell = currCell.adjacentCells[i];
                    int index;
                    if (horizontals.Contains(tempCell))
                    {
                        if (Random.Range(-1.0f, 1.0f) < _stats.bias)
                        {
                            index = hzShuffCount;
                            currCell.adjacentCells[i] = currCell.adjacentCells[index];
                            currCell.adjacentCells[index] = tempCell;
                        }

                        hzShuffCount++;
                    }
                    else
                    {
                        if (Random.Range(-1.0f, 1.0f) > _stats.bias)
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
        mazeCell startingCell;
        mazeCell currCell;
        currCell = maze[0];
        startingCell = maze[0];

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
            List<cellLabel> verticals = new List<cellLabel>();

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
                for (int i = 0; i < currCell.adjacentCells.Count; i++)
                {
                    cellLabel tempCell = currCell.adjacentCells[i];
                    int index;
                    if (horizontals.Contains(tempCell))
                    {
                        if (Random.Range(-1.0f, 1.0f) < _stats.bias)
                        {
                            index = hzShuffCount;
                            currCell.adjacentCells[i] = currCell.adjacentCells[index];
                            currCell.adjacentCells[index] = tempCell;
                        }                        
                        
                        hzShuffCount++;
                    }
                    else
                    {
                        if (Random.Range(-1.0f, 1.0f) > _stats.bias)
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
