using UnityEngine;
using System.Collections;

public class demo : MonoBehaviour {

    public CarBehavior car1;
    public CarBehavior car2;
    public CarBehavior car3;
    public CarBehavior car4;
    public CarBehavior car5;
    public CarBehavior car6;
    public CarBehavior car7;
    public CarBehavior car8;
    public CarBehavior car9;

	void Start () {
        //OFF();
	}
	
	void Update () {
        // if (Input.GetKey("1") | Input.GetKey("2") | Input.GetKey("3") | Input.GetKey("4") | Input.GetKey("5") | Input.GetKey("6") | Input.GetKey("7") | Input.GetKey("8") | Input.GetKey("9"))
        // {
        //     OFF();
        // }

        if (Input.GetKey("1")) { car1.enabled = true; }
        if (Input.GetKey("2")) { car2.enabled = true; }
        if (Input.GetKey("3")) { car3.enabled = true; }
        if (Input.GetKey("4")) { car4.enabled = true; }
        if (Input.GetKey("5")) { car5.enabled = true; }
        if (Input.GetKey("6")) { car6.enabled = true; }
        if (Input.GetKey("7")) { car7.enabled = true; }
        if (Input.GetKey("8")) { car8.enabled = true; }
        if (Input.GetKey("9")) { car9.enabled = true; }
	}
    void OFF()
    {
        car1.enabled = false;
        car2.enabled = false;
        car3.enabled = false;
        car4.enabled = false;
        car5.enabled = false;
        car6.enabled = false;
        car7.enabled = false;
        car8.enabled = false;
        car9.enabled = false;
    }
}


