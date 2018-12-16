using UnityEngine;
using System.Collections;

public class YouWinTest : MonoBehaviour {
    private CreateLevel _gameCtrl;

    // Use this for initialization
    private void Start()
    {
        //Get a reference
    }

    private void OnTriggerEnter(Collider other)
    {
        //call a method of gameCtrl indicating a possible win situation
        //Is it good to test for Player here?
    }
}
