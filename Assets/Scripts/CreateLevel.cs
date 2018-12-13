using UnityEngine;
using System.Collections;

public class CreateLevel : MonoBehaviour
{
    //The tiles have been modeled as 4x4 unity unit squares
    private const float tileSize = 4;

    private GameObject root, floor, environment, ball;
    public int xHalfExt = 1;
    public int zHalfExt = 1;

    public GameObject outerWall;
    public GameObject innerWall;
    public GameObject exitTile;
    public GameObject[] floorTiles;

    private int xExt, zExt;
    private int start, end;

    // Use this for initialization
    void Awake()
    {
        //Gather together all refrences you will need later on


        //Build the values for xExt and zExt from xHalfExt and zHalfExt
        //Build an offset for the dyn playfield from the BasePlatform e.g. the bigger halfExtent value in unity units
        //Calculate a scale factor for scaling the non-movable environment (and therefore the camera) and the BasePlatform 
        // the factors that the environment are scaled for right now are for x/zHalfExt =1, scale accordingly
        //i.e. the playfield/environment should be as big as the dynamic field

        //Scale Environment

        //Scale  + position BasePlate


        if (root != null)
        {
            //Create the outer walls for given extXZ

            //create a maze
            //Build the maze from the given set of prefabs
            //Set the walls for the maze (place only one wall between two cells, not two!)

            //Place the PlayerBall above the playfiel
            placeBallStart();
        }
    }

    //You might need this more than once...
    void placeBallStart()
    {
        //Reset Physics
        //Place the ball
    }

    public void EndzoneTrigger(GameObject other)
    {
        //Check if ball first...
        //Player has fallen onto ground plane, reset
    }
    public void winTrigger(GameObject other)
    {
        //Check if ball first...

        //Destroy this maze
        //Generate new maze
        //Player has fallen onto ground plane, reset
    }
}
	

