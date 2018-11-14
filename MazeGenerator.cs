using System;
using System.Collections.Generic;
using MazeGeneratorCollection;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// Main script for creating and spawning the maze
/// </summary>
/// <remarks>
/// Ideally I'd break this script up into seperate ones but considering the scope of this project i decided not to.
/// </remarks>
public class MazeGenerator : MonoBehaviour {
    
    // protect the singleton so it's not "setable"
    private static MazeGenerator instance;
    public static MazeGenerator Instance { get { return instance; } }
    
    //Create new settings at Assets/Create/MazeGenerator/Game settings
    [Header("Settings in scriptable object")]
    public GameSettings settings;
    
    // prefabs
    private GameObject wall;
    private GameObject floor;

    // 2dimensional array of cell class to store celldata
    public Cell[,] cells;
    // storing the x and y in the cells array
    private CellPosition currentPosition;
    private bool buildingInProgress;
    private Color floorColor;
    [HideInInspector]
    public GameObject mazeHolder;

    // frontier cells used in prim's algorithm, to avoid duplicates i'm using Linq
    private List<CellPosition> frontier = new List<CellPosition>();
        
    #region init

    private void Awake()
    {
        // singleton pattern implementation
        // Check if another version of this static class is active in the scene
        if (instance != null && instance != this)
        {
            // If so then destroy it, THERE CAN BE ONLY ONE.
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        // save these prefabs locally
        wall = settings.wall;
        floor = settings.floor;
    }
    #endregion

    /*
        Pseudo code of algorithms
        -------------------------
        Recursive backtracking
        1. Close all cells
        2. Choose starting cell and open it. This is the current cell
        3. Pick a cell adjacent to the current cell that hasn’t been visited and open it. It becomes the current cell
        4. Repeat 2 until no adjacent wall can be selected
        5. The previous cell becomes the current cell. If this cell is the starting cell, then we are done. Else go to 2.

        Prim's algorithm
        1. Choose a cell at random and add it to the list of visited cells. This is the current cell.
        2. Mark all cells adjacent to the current cell
        3. Randomly select a marked cell and remove its connecting edge to a cell from the list of visited cells. This is now the current cell.
        4. Repeat 2 until no adjacent wall can be selected
        5. While there are marked cells remaining go to 2        
    */

    public void GenerateNewMaze()
    {
        cells = new Cell[settings.mazeWidth, settings.mazeLength];
        floor.transform.localScale = new Vector3(settings.cellSize, .01f, settings.cellSize);
        wall.transform.localScale = new Vector3(wall.transform.localScale.x, wall.transform.localScale.y, settings.cellSize);
        SpawnMaze(settings.mazeWidth, settings.mazeLength, settings.cellSize);
        
        switch (settings.algorithm)
        {
            case ActiveAlgorithm.RecursiveBacktracker:
                GenerateWithRecursiveBacktracker(settings.mazeWidth, settings.mazeLength);
                break;
            case ActiveAlgorithm.PrimsAlgorithm:
                GenerateWithPrim(settings.mazeWidth, settings.mazeLength);
                break;
            default:
                break;
        }        
    }

    /// <summary>
    /// Spawn a maze without any connections
    /// </summary>
    /// <param name="width"> Width of the maze </param>
    /// <param name="length"> Length of the maze </param>
    /// <param name="cellSize"></param>
    private void SpawnMaze(int width, int length, float cellSize)
    {
        // destroy maze if there's already one
        GameObject checkForActiveMaze = GameObject.Find("Maze");
        if (checkForActiveMaze != null)
        {
            Destroy(checkForActiveMaze);
        }
        mazeHolder = new GameObject("Maze");

        // move the camera to fit the maze
        MoveToFitMaze.Instance.Move(new Vector3(((width - 1) * cellSize) / 2, ((length + width) * cellSize) / 4 + 3, ((length - 1) * cellSize) / 2));

        /// generate maze without connections
        for (int mazeX = 0; mazeX < width; mazeX++)
        {
            for (int mazeY = 0; mazeY < length; mazeY++)
            {
                // spawn new cell with floor
                cells[mazeX, mazeY] = new Cell
                {
                    floor = Instantiate(floor, new Vector3(mazeX * cellSize, 0, mazeY * cellSize), Quaternion.identity, mazeHolder.transform)
                };
                // if cell is on the westend, add an western wall
                if (mazeX == 0)
                {
                    cells[0, mazeY].westWall = Instantiate(wall, new Vector3(mazeX * cellSize - (cellSize / 2), settings.wall.transform.localScale.y / 2, mazeY * cellSize), Quaternion.identity, mazeHolder.transform);
                }
                // if cell is on the north end, add an southern wall
                if (mazeY == 0)
                {
                    cells[mazeX, 0].northWall = Instantiate(wall, new Vector3(mazeX * cellSize, settings.wall.transform.localScale.y / 2, mazeY * cellSize - (cellSize / 2)), Quaternion.identity, mazeHolder.transform);
                    cells[mazeX, 0].northWall.transform.Rotate(Vector3.up, 90f);
                }

                cells[mazeX, mazeY].eastWall = Instantiate(wall, new Vector3(mazeX * cellSize + (cellSize / 2), settings.wall.transform.localScale.y / 2, mazeY * cellSize), Quaternion.identity, mazeHolder.transform);

                cells[mazeX, mazeY].southWall = Instantiate(wall, new Vector3(mazeX * cellSize, settings.wall.transform.localScale.y / 2, mazeY * cellSize + (cellSize / 2)), Quaternion.identity, mazeHolder.transform);
                cells[mazeX, mazeY].southWall.transform.Rotate(Vector3.up, 90f);
            }
        }
    }

    #region PrimAlgorithm

    /// <summary>
    /// Carve out a mazepath using Prim's algorithm
    /// </summary>
    /// <remarks>
    /// This function should be made asynchronous in future
    /// </remarks>
    /// <param name="width"></param>
    /// <param name="length"></param>
    private void GenerateWithPrim(int width, int length)
    {
        /* 
        An implementation of Prim's algorithm for generating mazes.
        This is a pretty fast algorithm, when implemented well, since it
        only needs random access to the list of frontier cells. It does
        require space proportional to the size of the maze, but even worse-
        case, it won't be but a fraction of the size of the maze itself.
        */

        // set random starting position to make maze more random
        currentPosition = new CellPosition { x = Random.Range(0, width), y = Random.Range(0, length) };
        frontier.Clear();
        buildingInProgress = true;

        while (buildingInProgress)
        {
            cells[currentPosition.x, currentPosition.y].visited = true;
            if (settings.randomColors)
            {
                floorColor = settings.mazeColors[Random.Range(0, settings.mazeColors.Length)];
                cells[currentPosition.x, currentPosition.y].floor.GetComponent<Renderer>().material.SetColor("_Color", floorColor);
            }
            // add non visited cells that surround the current cell to the frontier
            var frontierQuery = frontier.Union(GetSurroundingCells(currentPosition)).ToList();
            frontier = frontierQuery;
            frontierQuery.Remove(currentPosition);
            if (frontierQuery.Count < 1)
            {
                buildingInProgress = false;
            }
            else
            {
                CellPosition newPosition = frontierQuery[Random.Range(0, frontierQuery.Count)];
                ConnectCellToMaze(newPosition);
                currentPosition = newPosition;
            }
        }
    }

    private void ConnectCellToMaze(CellPosition position)
    {
        bool connected = false;
        while (!connected)
        {
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    // connect north
                    if (position.y > 0 && cells[position.x, position.y - 1].visited)
                    {
                        Destroy(cells[position.x, position.y - 1].southWall);
                        if (settings.randomColors)
                        {
                            cells[position.x, position.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[position.x, position.y - 1].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                case 1:
                    // connect east
                    if (position.x + 1 < settings.mazeWidth && cells[position.x + 1, position.y].visited)
                    {
                        Destroy(cells[position.x, position.y].eastWall);
                        if (settings.randomColors)
                        {
                            cells[position.x, position.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[position.x + 1, position.y].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                case 2:
                    // connect south
                    if (position.y + 1 < settings.mazeLength && cells[position.x, position.y + 1].visited)
                    {
                        Destroy(cells[position.x, position.y].southWall);
                        if (settings.randomColors)
                        {
                            cells[position.x, position.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[position.x, position.y + 1].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                case 3:
                    // connect west
                    if (position.x > 0 && cells[position.x - 1, position.y].visited)
                    {
                        Destroy(cells[position.x - 1, position.y].eastWall);
                        if (settings.randomColors)
                        {
                            cells[position.x, position.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[position.x - 1, position.y].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region RecursiveAlgorithm

    /// <summary>
    /// Carve out a mazepath using Recursive backtracking
    /// </summary>
    /// <remarks>
    /// This function should also be made asynchronous in future to keep the app running more smoothly
    /// </remarks>
    private void GenerateWithRecursiveBacktracker(int width, int length)
    {
        /*
        Recursive backtracking algorithm for maze generation. Requires that
        the entire maze be stored in memory, but is quite fast, easy to
        learn and implement, and (with a few tweaks) gives fairly good mazes.
        Can also be customized in a variety of ways.
        */

        // set random starting position to make maze more random
        currentPosition = new CellPosition { x = Random.Range(0, width), y = Random.Range(0, length) };
        BreakWalls(width, length);
    }

    private void BreakWalls(int width, int length)
    {
        cells[currentPosition.x, currentPosition.y].visited = true;
        bool availableNeighbours = CheckNeighbours(width, length, currentPosition);

        // give color to this area of the maze    
        floorColor = settings.mazeColors[Random.Range(0, settings.mazeColors.Length)];
        //Debug.Log("setting color to: " + floorColor.ToString());
        while (availableNeighbours)
        {
            if (settings.randomColors)
            {
                cells[currentPosition.x, currentPosition.y].floor.GetComponent<Renderer>().material.SetColor("_Color", floorColor);
            }
            // 0 = north, 1 = east, 2 = south, 3 west
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    // go north
                    if (CellAvailable(width, length, new Vector2(currentPosition.x, currentPosition.y - 1)))
                    {
                        Destroy(cells[currentPosition.x, currentPosition.y - 1].southWall);
                        currentPosition.y--;
                    }
                    else
                    {
                        availableNeighbours = CheckNeighbours(width, length, currentPosition);
                    }
                    break;
                case 1:
                    // go east
                    if (CellAvailable(width, length, new Vector2(currentPosition.x + 1, currentPosition.y)))
                    {
                        Destroy(cells[currentPosition.x, currentPosition.y].eastWall);
                        currentPosition.x++;
                    }
                    else
                    {
                        availableNeighbours = CheckNeighbours(width, length, currentPosition);
                    }
                    break;
                case 2:
                    // go south
                    if (CellAvailable(width, length, new Vector2(currentPosition.x, currentPosition.y + 1)))
                    {
                        Destroy(cells[currentPosition.x, currentPosition.y].southWall);
                        currentPosition.y++;
                    }
                    else
                    {
                        availableNeighbours = CheckNeighbours(width, length, currentPosition);
                    }
                    break;
                case 3:
                    // go west
                    if (CellAvailable(width, length, new Vector2(currentPosition.x - 1, currentPosition.y)))
                    {
                        Destroy(cells[currentPosition.x - 1, currentPosition.y].eastWall);
                        currentPosition.x--;
                    }
                    else
                    {
                        availableNeighbours = CheckNeighbours(width, length, currentPosition);
                    }
                    break;
                default:
                    break;
            }
            cells[currentPosition.x, currentPosition.y].visited = true;
        }
        if (!availableNeighbours)
        {
            SearchRemainingCells();
        }
    }

    private void SearchRemainingCells()
    {
        for (int x = 0; x < settings.mazeWidth; x++)
        {
            for (int y = 0; y < settings.mazeLength; y++)
            {
                if (!cells[x, y].visited && IsNextToMaze(x, y))
                {
                    currentPosition = new CellPosition { x = x, y = y };
                    // Debug.Log("Starting at: [ " + currentPosition.x.ToString() + ", " + currentPosition.y.ToString() + "]");                   
                    ConnectCellToMaze();
                    BreakWalls(settings.mazeWidth, settings.mazeLength);
                }
            }
        }
    }

    private void ConnectCellToMaze()
    {
        bool connected = false;
        while (!connected)
        {
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0:
                    // connect north
                    if (currentPosition.y > 0 && cells[currentPosition.x, currentPosition.y - 1].visited)
                    {
                        Destroy(cells[currentPosition.x, currentPosition.y - 1].southWall);
                        if (settings.randomColors)
                        {
                            cells[currentPosition.x, currentPosition.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[currentPosition.x, currentPosition.y - 1].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                case 1:
                    // connect east
                    if (currentPosition.x + 1 < settings.mazeWidth && cells[currentPosition.x + 1, currentPosition.y].visited)
                    {
                        Destroy(cells[currentPosition.x, currentPosition.y].eastWall);
                        if (settings.randomColors)
                        {
                            cells[currentPosition.x, currentPosition.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[currentPosition.x + 1, currentPosition.y].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                case 2:
                    // connect south
                    if (currentPosition.y + 1 < settings.mazeLength && cells[currentPosition.x, currentPosition.y + 1].visited)
                    {
                        Destroy(cells[currentPosition.x, currentPosition.y].southWall);
                        if (settings.randomColors)
                        {
                            cells[currentPosition.x, currentPosition.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[currentPosition.x, currentPosition.y + 1].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                case 3:
                    // connect west
                    if (currentPosition.x > 0 && cells[currentPosition.x - 1, currentPosition.y].visited)
                    {
                        Destroy(cells[currentPosition.x - 1, currentPosition.y].eastWall);
                        if (settings.randomColors)
                        {
                            cells[currentPosition.x, currentPosition.y].floor.GetComponent<Renderer>().material.SetColor("_Color", cells[currentPosition.x - 1, currentPosition.y].floor.GetComponent<Renderer>().material.color);
                        }
                        connected = true;
                    }
                    break;
                default:
                    break;
            }
        }
    }
    #endregion
    
    #region checking variables

    private List<CellPosition> GetSurroundingCells(CellPosition position)
    {
        List<CellPosition> returnCells = new List<CellPosition>();
        // check north, east, south and west and if a cell has not been visited add it to the list
        if (CellAvailable(settings.mazeWidth, settings.mazeLength, new Vector2(currentPosition.x, currentPosition.y - 1)))
        {
            returnCells.Add(new CellPosition { x = currentPosition.x, y = currentPosition.y - 1 });
        }
        if (CellAvailable(settings.mazeWidth, settings.mazeLength, new Vector2(currentPosition.x + 1, currentPosition.y)))
        {
            returnCells.Add(new CellPosition { x = currentPosition.x + 1, y = currentPosition.y });
        }
        if (CellAvailable(settings.mazeWidth, settings.mazeLength, new Vector2(currentPosition.x, currentPosition.y + 1)))
        {
            returnCells.Add(new CellPosition { x = currentPosition.x, y = currentPosition.y + 1 });
        }
        if (CellAvailable(settings.mazeWidth, settings.mazeLength, new Vector2(currentPosition.x - 1, currentPosition.y)))
        {
            returnCells.Add(new CellPosition { x = currentPosition.x - 1, y = currentPosition.y });
        }
        return returnCells;
    }
    
    private bool CellAvailable(int width, int length, Vector2 checkPosition)
    {
        bool available = false;
        if (checkPosition.x >= 0 && checkPosition.x < width && checkPosition.y >= 0 && checkPosition.y < length)
        {
            if (!cells[(int)checkPosition.x, (int)checkPosition.y].visited)
            {
                available = true;
            }
        }
        return available;
    }

    /// <summary>
    /// Check if there's an empty/unvisited neighbour cell
    /// </summary>
    /// <param name="width"> Width of the current maze </param>
    /// <param name="length"> Length of the current maze </param>
    /// <param name="checkPosition"> cellposition to check around</param>
    /// <returns></returns>
    private bool CheckNeighbours(int width, int length, CellPosition checkPosition)
    {
        bool available = false;
        if (checkPosition.x >= 0 && checkPosition.x < width && checkPosition.y >= 0 && checkPosition.y < length)
        {
            // check east
            if (checkPosition.x < width - 1 && !cells[checkPosition.x + 1, checkPosition.y].visited)
            {
                available = true;
            }
            // check west
            if (checkPosition.x > 0 && !cells[checkPosition.x - 1, checkPosition.y].visited)
            {
                available = true;
            }
            // check north
            if (checkPosition.y < length - 1 && !cells[checkPosition.x, checkPosition.y + 1].visited)
            {
                available = true;
            }
            // check south
            if (checkPosition.y > 0 && !cells[checkPosition.x, checkPosition.y - 1].visited)
            {
                available = true;
            }
        }
        return available;
    }

    /// <summary>
    /// Check if the x and y position have a cell next to a visited cell
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns> If the x,y position has a visited neighbour </returns>
    private bool IsNextToMaze(int x, int y)
    {
        bool nextToMaze = false;
        // check north
        if (y < settings.mazeLength - 1 && cells[x, y + 1].visited)
        {
            nextToMaze = true;
        }
        // check east
        if (x < settings.mazeWidth - 1 && cells[x + 1, y].visited)
        {
            nextToMaze = true;
        }        
        // check south
        if (y > 0 && cells[x, y - 1].visited)
        {
            nextToMaze = true;
        }
        // check west
        if (x > 0 && cells[x - 1, y].visited)
        {
            nextToMaze = true;
        }        
        return nextToMaze;
    }
    #endregion    
}