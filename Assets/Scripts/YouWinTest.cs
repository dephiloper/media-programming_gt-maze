using UnityEngine;

public class YouWinTest : MonoBehaviour 
{
    private CreateLevel _gameCtrl;

    // Use this for initialization
    private void Start()
    {
        //Get a 
        _gameCtrl = CreateLevel.Instance;
    }

    private void OnTriggerExit(Collider other)
    {
        //call a method of gameCtrl indicating a possible win situation
        //Is it good to test for Player here?
        _gameCtrl.WinTrigger(other.gameObject);
    }
}
