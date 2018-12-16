using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreateLevel : MonoBehaviour
{
    //The tiles have been modeled as 4x4 unity unit squares
    private const float TileSize = 4;

    [Header("Dynamic Environment Elements")] [SerializeField]
    private GameObject _outerWall;

    [SerializeField] private GameObject _innerWall;
    [SerializeField] private GameObject _exitTile;
    [SerializeField] private GameObject[] _floorTiles;

    [Header("Play Field Size")] [SerializeField]
    private int _xHalfExt = 2;

    [SerializeField] private int _zHalfExt = 2;

    [Header("Environment References")] [SerializeField]
    private GameObject _root;

    [SerializeField] private GameObject _floor;
    [SerializeField] private GameObject _environment;
    [SerializeField] private GameObject _ball;

    private int _xExt, _zExt;
    private int _start, _end;

    private Cell[,] _cells;
    private Stack<Cell> _backTrackerStack;

    private Cell _currentCell;

    private void Awake()
    {
        //Gather together all references you will need later on

        //Build the values for xExt and zExt from xHalfExt and zHalfExt
        _xExt = 2 * _xHalfExt + 1;
        _zExt = 2 * _zHalfExt + 1;

        _cells = new Cell[_xExt, _zExt];
        _backTrackerStack = new Stack<Cell>();

        //Build an offset for the dynamic play field from the BasePlatform e.g. the bigger halfExtent value in unity units
        var offset = Math.Max(_xHalfExt, _zHalfExt);
        //Calculate a scale factor for scaling the non-movable environment (and therefore the camera) and the BasePlatform 
        // the factors that the environment are scaled for right now are for x/zHalfExt =1, scale accordingly
        //i.e. the play field/environment should be as big as the dynamic field
        //Scale Environment
        _environment.transform.localScale *= offset;
        _floor.transform.localScale = new Vector3(_xExt * TileSize, 1, _zExt * TileSize);
        
        if (_root != null)
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

            //Place the PlayerBall above the play field
           //PlaceBallStart();
        }
    }

/*    private void Update()
    {


        //while (_cells.Cast<Cell>().Any(x => !x.Visited))
        //{
          
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            //If the current cell has any neighbours which have not been visited
            var neighbour = FindRandomNeighbours(_currentCell);

            if (neighbour != null)
            {
                // Push the current cell to the stack
                _backTrackerStack.Push(_currentCell);
                // Remove the wall between the current cell and the chosen cell
                RemoveWall(_currentCell, neighbour);
                // Make the chosen cell the current cell and mark it as visited
                _currentCell = neighbour;
                _currentCell.Visited = true;
            }
            else if (_backTrackerStack.Count > 0)
            {
                _currentCell = _backTrackerStack.Pop();
            }
        }
    }*/

    private void CreateInnerWalls()
    {
        for (var i = 0; i < _xExt; i++)
        {
            for (var j = 0; j < _zExt; j++)
            {
                var x = i - _xHalfExt;
                var z = j - _zHalfExt;

                var tile = CreateTile(x, z);
                var cell = tile.AddComponent<Cell>();
                cell.X = i;
                cell.Z = j;
                cell.Tile = tile;

                if (z < _zHalfExt)
                {
                    var wallH = CreateInnerWall(x * TileSize, z * TileSize + TileSize / 2f);
                    cell.TopWall = wallH;
                    wallH.transform.parent = tile.transform;
                }

                if (x < _xHalfExt)
                {
                    var wallV = CreateInnerWall(x * TileSize + TileSize / 2f, z * TileSize, true);
                    cell.RightWall = wallV;
                    wallV.transform.parent = tile.transform;
                }

                _cells[i, j] = cell;
            }
        }
    }

    private void CreateMaze()
    {
        // Make the initial cell the current cell and mark it as visited
        var currentCell = _cells[0, 0];
        currentCell.Visited = true;

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
        
        Destroy(currentCell.Tile);
        currentCell.Tile = CreateTile(currentCell.X - _xHalfExt, currentCell.Z - _zHalfExt, true);
    }

    private static void RemoveWall(Cell current, Cell neighbour)
    {
        if (current.X < neighbour.X)
            Destroy(current.RightWall);
        else if (current.X > neighbour.X)
            Destroy(neighbour.RightWall);
        else if (current.Z < neighbour.Z)
            Destroy(current.TopWall);
        else if (current.Z > neighbour.Z)
            Destroy(neighbour.TopWall);
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

    private GameObject CreateInnerWall(float x, float z, bool rotate = false)
    {
        var wall = Instantiate(_innerWall);
        wall.transform.Translate(x, 0, z);
        if (rotate) wall.transform.Rotate(0, 90, 0); // rotate after translate
        return wall;
    }

    private GameObject CreateTile(int x, int z, bool isExitTile = false)
    {
        var tile = Instantiate(isExitTile
            ? _exitTile
            : _floorTiles[(int) Mathf.Floor(Random.Range(0, _floorTiles.Length))]);

        tile.transform.Translate(x * TileSize, 0, z * TileSize);
        tile.transform.parent = _root.transform;
        return tile;
    }

    private void CreateOuterWalls()
    {
        for (var i = -_xHalfExt; i <= _xHalfExt; i++)
        {
            var outerWallL = Instantiate(_outerWall);
            var outerWallR = Instantiate(_outerWall);
            outerWallL.transform.Translate(i * TileSize, 0, _zExt / 2f * TileSize);
            outerWallR.transform.Translate(i * TileSize, 0, -_zExt / 2f * TileSize);
            outerWallL.transform.parent = _root.transform;
            outerWallR.transform.parent = _root.transform;
        }

        for (var i = -_zHalfExt; i <= _zHalfExt; i++)
        {
            var outerWallT = Instantiate(_outerWall);
            var outerWallB = Instantiate(_outerWall);
            outerWallT.transform.Translate(_xExt / 2f * TileSize, 0, i * TileSize);
            outerWallB.transform.Translate(-_xExt / 2f * TileSize, 0, i * TileSize);
            outerWallT.transform.Rotate(0, 90, 0);
            outerWallB.transform.Rotate(0, 90, 0);
            outerWallT.transform.parent = _root.transform;
            outerWallB.transform.parent = _root.transform;
        }
    }

    //You might need this more than once...
    private void PlaceBallStart()
    {
        //Reset Physics
        var ballRb = _ball.GetComponent<Rigidbody>();
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        //Place the ball
    }

    public void EndZoneTrigger(GameObject other)
    {
        //Check if ball first...
        //Player has fallen onto ground plane, reset
    }

    public void WinTrigger(GameObject other)
    {
        //Check if ball first...

        //Destroy this maze
        //Generate new maze
        //Player has fallen onto ground plane, reset
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