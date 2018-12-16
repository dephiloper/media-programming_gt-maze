using UnityEngine;

public class BaseMovement : MonoBehaviour {
    [SerializeField] private GameObject _hPedal;
    [SerializeField] private GameObject _vPedal;
    [SerializeField] private float _pedalFactor = -10f;
    [SerializeField] private float _rotFactor = 30f;
    
    private void Update()
    {
        RotatePlatform();
        
        // reset rotation with key 0 for debug purpose
        if (Input.GetKeyDown(KeyCode.Alpha0)) 
            transform.rotation = Quaternion.Euler(0, 0 ,0);

        if (Input.GetKey(KeyCode.E))
            transform.Rotate(0, -_rotFactor * Time.deltaTime, 0);
        else if (Input.GetKey(KeyCode.Q))
            transform.Rotate(0, _rotFactor * Time.deltaTime, 0);
    }

    private void RotatePlatform()
    {
        //Get Input from Input.GetAxis
        //Stretch it from 0-1 to 0 - maxRotFactor
        var vAxis = Input.GetAxis("Vertical") * _rotFactor * Time.deltaTime;

        //REM: Maybe you have to invert one axis to get things right visually
        var hAxis = Input.GetAxis("Horizontal") * _rotFactor * Time.deltaTime;

        //Apply a part of the rotation to this (and children) to rotate the play field
        //Use Quaternion.Slerp and Quaternion.Euler for doing it
        transform.Rotate(-vAxis, 0, 0);
        transform.Rotate(0, 0, hAxis);

        //Apply an exaggerated amount of rotation to the pedals to visualize the players input
        //Make the rotation look right
        _hPedal.transform.Rotate(0, hAxis * _pedalFactor, 0);
        _vPedal.transform.Rotate(0, vAxis * _pedalFactor, 0);
    }
}
