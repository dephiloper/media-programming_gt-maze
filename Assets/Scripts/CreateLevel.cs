using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class CreateLevel : MonoBehaviour
{
    public static CreateLevel Instance;
    
    //The tiles have been modeled as 4x4 unity unit squares
    private const float TileSize = 4;

    [Header("Dynamic Environment Elements")] 
    [SerializeField]
    private GameObject outerWall;

    [SerializeField] 
    private GameObject innerWall;
    [SerializeField] 
    private GameObject exitTile;
    [SerializeField] 
    private GameObject[] floorTiles;

    [Header("Play Field Size")] 
    [SerializeField]
    private int xHalfExt = 2;
    [SerializeField] 
    private int zHalfExt = 2;

    [Header("Environment References")] 
    [SerializeField]
    private GameObject root;
    [SerializeField] 
    private GameObject floor;
    [SerializeField] 
    private GameObject environment;
    [SerializeField] 
    private GameObject ball;

    private int _xExt, _zExt;
    private int _start, _end;

    private Cell[,] _cells;
    private Stack<Cell> _backTrackerStack;

    private Cell _currentCell;
    private const float DefaultScaleValue = 0.5f;
    private const float IncreaseScaleFactor = 0.3f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (xHalfExt < 0 || zHalfExt < 0) 
            Debug.LogError("xHalfExt and zHalfExt must both have positive values.");
    }
    #endif

    private void Awake()
    {
        //Gather together all references you will need later on
        if (Instance == null)
            Instance = this;
        
        //Build the values for xExt and zExt from xHalfExt and zHalfExt
        _xExt = 2 * xHalfExt + 1;
        _zExt = 2 * zHalfExt + 1;

        _cells = new Cell[_xExt, _zExt];
        _backTrackerStack = new Stack<Cell>();

        //Build an offset for the dynamic play field from the BasePlatform e.g. the bigger halfExtent value in unity units
        var offset = Math.Max(xHalfExt, zHalfExt);
        //Calculate a scale factor for scaling the non-movable environment (and therefore the camera) and the BasePlatform 
        // the factors that the environment are scaled for right now are for x/zHalfExt =1, scale accordingly
        //i.e. the play field/environment should be as big as the dynamic field
        floor.transform.localScale = new Vector3(_xExt * TileSize, 1, _zExt * TileSize);
        floor.transform.Translate(Vector3.up * -offset);
        
        
        
        //Scale Environment 
        // if the offset is 0 or 1 we do not change the scale
        // otherwise we scale the environment stepwise with an increase of 0.3
        var scaleValue = offset < 2 ? DefaultScaleValue : DefaultScaleValue + IncreaseScaleFactor * (offset - 1f);
        environment.transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        
        if (root != null)
        {
            CreateOuterWalls();
            CreateInnerWalls();
            
            // Make the initial cell the current cell and mark it as visited
            _currentCell = _cells[0, 0];
            _currentCell.Visited = true;
            
            //create a maze
            //Build the maze from the given set of prefabs
            //Set the walls for the maze (place only one wall between two cells, not two!)
            CreateMaze();

            CreateExit();
            
            //Place the PlayerBall above the play field
           PlaceBallStart();
        }
    }

    private void CreateOuterWalls()
    {
        for (var i = -xHalfExt; i <= xHalfExt; i++)
        {
            var outerWallL = Instantiate(outerWall, root.transform, true);
            var outerWallR = Instantiate(outerWall, root.transform, true);
            outerWallL.transform.Translate(i * TileSize, 0, _zExt / 2f * TileSize);
            outerWallR.transform.Translate(i * TileSize, 0, -_zExt / 2f * TileSize);
        }

        for (var i = -zHalfExt; i <= zHalfExt; i++)
        {
            var outerWallT = Instantiate(outerWall, root.transform, true);
            var outerWallB = Instantiate(outerWall, root.transform, true);
            outerWallT.transform.Translate(_xExt / 2f * TileSize, 0, i * TileSize);
            outerWallB.transform.Translate(-_xExt / 2f * TileSize, 0, i * TileSize);
            outerWallT.transform.Rotate(0, 90, 0);
            outerWallB.transform.Rotate(0, 90, 0);
        }
    }

    private void CreateInnerWalls()
    {
        for (var i = 0; i < _xExt; i++)
        {
            for (var j = 0; j < _zExt; j++)
            {
                var x = i - xHalfExt;
                var z = j - zHalfExt;

                var tile = (i == 0 && j == 0) ? CreateTile(x, z, TileType.Entrance) : CreateTile(x, z, TileType.Random);
                var cell = tile.AddComponent<Cell>();
                cell.X = i;
                cell.Z = j;
                cell.Tile = tile;

                if (z < zHalfExt)
                {
                    var wallH = CreateInnerWall(x * TileSize, z * TileSize + TileSize / 2f);
                    cell.TopWall = wallH;
                    wallH.transform.parent = tile.transform;
                }

                if (x < xHalfExt)
                {
                    var wallV = CreateInnerWall(x * TileSize + TileSize / 2f, z * TileSize, true);
                    cell.RightWall = wallV;
                    wallV.transform.parent = tile.transform;
                }

                _cells[i, j] = cell;
            }
        }
    }
    
    private GameObject CreateInnerWall(float x, float z, bool rotate = false)
    {
        var wall = Instantiate(innerWall);
        wall.transform.Translate(x, 0, z);
        if (rotate) wall.transform.Rotate(0, 90, 0); // rotate after translate
        return wall;
    }

    private GameObject CreateTile(int x, int z, TileType tileType)
    {
        GameObject tile;
        
        switch (tileType)
        {
            case TileType.Random:
                tile = Instantiate(floorTiles[(int) Mathf.Floor(Random.Range(0, floorTiles.Length))]);
                break;
            case TileType.Entrance:
                tile = Instantiate(floorTiles[0]);
                break;
            case TileType.Exit:
                tile = Instantiate(exitTile);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(tileType), tileType,
                    "This message should never be printed out.");
        }

        tile.transform.Translate(x * TileSize, 0, z * TileSize);
        tile.transform.parent = root.transform;

        return tile;
    }
    
    private void CreateMaze()
    {
        // Make the initial cell the current cell and mark it as visited
        var currentCell = _cells[0, 0];
        currentCell.Visited = true;
        
        // while any of the cells is unvisited
        while (_cells.Cast<Cell>().Any(x => !x.Visited))
        {
            //If the current cell has any neighbours which have not been visited
            var neighbour = FindRandomNeighbours(currentCell);

            if (neighbour != null)
            {
                // Push the current cell to the stack
                _backTrackerStack.Push(currentCell);
                // Remove the wall between the current cell and the chosen cell
                RemoveWall(currentCell, neighbour);
                // Make the chosen cell the current cell and mark it as visited
                currentCell = neighbour;
                currentCell.Visited = true;
            }
            else if (_backTrackerStack.Count > 0)
            {
                currentCell = _backTrackerStack.Pop();
            }
        }
    }
    
    private void CreateExit()
    {
        var lastCell = _cells[_xExt - 1, _zExt - 1];
        Destroy(lastCell.Tile);
        lastCell.Tile = CreateTile(lastCell.X - xHalfExt, lastCell.Z - zHalfExt, TileType.Exit);
    }

    private static void RemoveWall(Cell current, Cell neighbour)
    {
        if (current.X < neighbour.X) Destroy(current.RightWall);
        else if (current.X > neighbour.X) Destroy(neighbour.RightWall);
        else if (current.Z < neighbour.Z) Destroy(current.TopWall);
        else if (current.Z > neighbour.Z) Destroy(neighbour.TopWall);
    }

    private Cell FindRandomNeighbours(Cell cell)
    {
        var neighbours = new List<Cell>();

        if (cell.X - 1 >= 0) neighbours.Add(_cells[cell.X - 1, cell.Z]);
        if (cell.X + 1 < _xExt) neighbours.Add(_cells[cell.X + 1, cell.Z]);
        if (cell.Z - 1 >= 0) neighbours.Add(_cells[cell.X, cell.Z - 1]);
        if (cell.Z + 1 < _zExt) neighbours.Add(_cells[cell.X, cell.Z + 1]);
        
        neighbours = neighbours.Where(x => !x.Visited).ToList();
        
        if (neighbours.Count == 0) return null;
        
        // Choose randomly one of the unvisited neighbours
        var rand = (int) Mathf.Floor(Random.Range(0, neighbours.Count));
        return neighbours[rand];
    }

    //You might need this more than once...
    private void PlaceBallStart()
    {
        //Reset Physics
        var ballRb = ball.GetComponent<Rigidbody>();
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        //Place the ball
        ball.transform.position = new Vector3(-xHalfExt * TileSize, 5, -zHalfExt * TileSize);
    }

    public void EndZoneTrigger(GameObject other)
    {
        //Check if ball first...
        if (!ball) return;
        root.GetComponent<BaseMovement>().ResetRotation();
        //Player has fallen onto ground plane, reset
        PlaceBallStart();
    }

    public void WinTrigger(GameObject other)
    {
        //Check if ball first...
        if (ball)
            //Destroy this maze
            //Generate new maze
            //Player has fallen onto ground plane, reset
            SceneManager.LoadScene(0);
    }    
}

internal class Cell : MonoBehaviour
{
    internal int X;
    internal int Z;
    
    internal bool Visited;

    internal GameObject TopWall;
    internal GameObject RightWall;
    internal GameObject Tile;
}

internal enum TileType
{
    Random,
    Entrance,
    Exit
}