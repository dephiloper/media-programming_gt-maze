using UnityEngine;

public class BaseMovement : MonoBehaviour {
    [SerializeField] private GameObject hPedal;
    [SerializeField] private GameObject vPedal;
    [SerializeField] private float pedalFactor = -10f;
    [SerializeField] private float rotFactor = 30f;
    
    private void Update()
    {
        RotatePlatform();
        
        // reset rotation with key 0 for debug purpose
        if (Input.GetKeyDown(KeyCode.Alpha0)) 
            ResetRotation();

        if (Input.GetKey(KeyCode.E))
            transform.Rotate(0, -rotFactor * Time.deltaTime, 0);
        else if (Input.GetKey(KeyCode.Q))
            transform.Rotate(0, rotFactor * Time.deltaTime, 0);
    }

    public void ResetRotation() =>
        transform.rotation = Quaternion.Euler(0, 0, 0);

    private void RotatePlatform()
    {
        //Get Input from Input.GetAxis
        //Stretch it from 0-1 to 0 - maxRotFactor
        var vAxis = Input.GetAxis("Vertical") * rotFactor * Time.deltaTime;

        //REM: Maybe you have to invert one axis to get things right visually
        var hAxis = Input.GetAxis("Horizontal") * rotFactor * Time.deltaTime;

        //Apply a part of the rotation to this (and children) to rotate the play field
        //Use Quaternion.Slerp and Quaternion.Euler for doing it
        transform.Rotate(-vAxis, 0, 0);
        transform.Rotate(0, 0, hAxis);

        //Apply an exaggerated amount of rotation to the pedals to visualize the players input
        //Make the rotation look right
        hPedal.transform.Rotate(0, hAxis * pedalFactor, 0);
        vPedal.transform.Rotate(0, vAxis * pedalFactor, 0);
    }
}
