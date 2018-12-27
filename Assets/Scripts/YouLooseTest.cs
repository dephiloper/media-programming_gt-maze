using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class YouLooseTest : MonoBehaviour
{
    private CreateLevel _gameCtrl;

    // Use this for initialization
    private void Start()
    {
        //Get a reference
        _gameCtrl = CreateLevel.Instance;
    }

    private void OnTriggerExit(Collider other)
    {
        //call a method of gameCtrl
        _gameCtrl.EndZoneTrigger(other.gameObject);
    }
}
