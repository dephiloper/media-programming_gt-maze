using UnityEngine;
using System.Collections;

public class YouWinTest : MonoBehaviour {
    CreateLevel gameCtrl;

    // Use this for initialization
    void Start()
    {
        //Get a reference
    }

    void OnTriggerEnter(Collider other)
    {
        //call a method of gameCtrl indicating a possible win situation
        //Is ist good to test for Player here?
    }
}
