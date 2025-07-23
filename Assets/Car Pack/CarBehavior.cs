using UnityEngine;
using System.Collections;

public class CarBehavior : MonoBehaviour {
    public bool OnlyKeyboard_setFalseToTouchControl;// set FALSE to use touchscreen
    public bool moveCamera;//your camera must compare tag "MainCamera"
    public bool tankControl;//rotation
    public bool dont_show_touch_Buttons;//if TouchControl is activated

    public float power1;//characteristics start
    public float power2;//characteristics acceleration
    public float power3;//characteristics high speed

    public float min_power;//characteristics reverse
    public float maxsteer;
    public float destabilization;//uncontrolled drift

    public float speedometer;

    private Transform wheel_L;
    private Transform wheel_R;
    private ParticleSystem smokeLB;
    private ParticleSystem smokeRB;
    private ParticleSystem sledLB;
    private ParticleSystem sledRB;
    private ParticleSystem sledLF;
    private ParticleSystem sledRF;

    ParticleSystem.EmissionModule smokeRBe;
    ParticleSystem.EmissionModule smokeLBe;
    ParticleSystem.EmissionModule sledLBe;
    ParticleSystem.EmissionModule sledRBe;
    ParticleSystem.EmissionModule sledLFe;
    ParticleSystem.EmissionModule sledRFe;

    private bool touch_str;//чтобы езда прямо срабатывала всегда, а не 1 раз (for myself)
    private Rigidbody2D Car_Body;
    private GameObject Camera;
    private GameObject help;
    private float speed=0;
    public float gas_pedal;
    private bool touch_gas;
    private bool touch_stop;
    private float cursteer;
    public float curdestab;
    private bool invesion;
	void Start () {
        invesion = false;
        Camera = GameObject.FindWithTag("MainCamera");
        Car_Body = gameObject.GetComponent<Rigidbody2D>();

        if (!tankControl)
        {
            wheel_L = transform.Find("wheel L");
            wheel_R = transform.Find("wheel R");
            smokeRB = transform.Find("smokeRB").GetComponent<ParticleSystem>();
            smokeLB = transform.Find("smokeLB").GetComponent<ParticleSystem>();
            sledLB = transform.Find("sledLB").GetComponent<ParticleSystem>();
            sledRB = transform.Find("sledRB").GetComponent<ParticleSystem>();
            sledLF = transform.Find("sledLF").GetComponent<ParticleSystem>();
            sledRF = transform.Find("sledRF").GetComponent<ParticleSystem>();

            smokeRBe = smokeRB.emission;
            smokeLBe = smokeLB.emission;
            sledLBe = sledLB.emission;
            sledRBe = sledRB.emission;
            sledLFe = sledLF.emission;
            sledRFe = sledRF.emission;
        }
	}

    void OnGUI() {
        if (!dont_show_touch_Buttons & !OnlyKeyboard_setFalseToTouchControl) {
            int x = Screen.width;
            int y = Screen.height;
            if (GUI.Button(new Rect(x * 0f, y * 0.7f, x * 0.2f, y * 0.3f), "Down")){}
            if (GUI.Button(new Rect(x * 0f, y * 0.4f, x * 0.2f, y * 0.3f), "UP")) { }
            if (GUI.Button(new Rect(x * 0.6f, y * 0.7f, x * 0.2f, y * 0.3f), "<")) { }
            if (GUI.Button(new Rect(x * 0.8f, y * 0.7f, x * 0.2f, y * 0.3f), ">")) { }
        }
    }
	void Update () {
        if (!tankControl){
            if (wheel_L != null & wheel_R != null){
                wheel_L.localRotation = Quaternion.Euler(new Vector3(0, 0, cursteer / maxsteer * 25));
                wheel_R.localRotation = Quaternion.Euler(new Vector3(0, 0, cursteer / maxsteer * 25));
            }

            if (curdestab > 0){
                sledLBe.rateOverDistance = 5; sledRBe.rateOverDistance = 5;
                smokeLBe.rateOverDistance = 2; smokeRBe.rateOverDistance = 2;
            }else{
                sledLBe.rateOverDistance = 0; sledRBe.rateOverDistance = 0;
                smokeLBe.rateOverDistance = 0; smokeRBe.rateOverDistance = 0;
            }
            sledLFe.rateOverDistance = 0; sledRFe.rateOverDistance = 0;
        }

        if (moveCamera) Camera.transform.position = new Vector3(transform.position.x, transform.position.y, -8 - Mathf.Abs(speed) * 0.15f);
        if (OnlyKeyboard_setFalseToTouchControl)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Gas();
            }

            else if (!Input.GetKey(KeyCode.S))
            {
                Neytralka();
            }

            if (Input.GetKey(KeyCode.S))
            {
                Tormoz();
            }


            if (Input.GetKey(KeyCode.A) & !Input.GetKey(KeyCode.D))
            {
                Left();
            }

            else if (Input.GetKey(KeyCode.D))
            {
                Right();
            }

            else
            {
                Straight(); 
            }

        }
        else
        {
            
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Vector2 pos = new Vector2(Input.GetTouch(i).position.x / Screen.width, Input.GetTouch(i).position.y / Screen.height);
                if (pos.x < 0.2f & pos.y > 0.3f & pos.y < 0.6f) { touch_gas = true; touch_stop = false; }
                else if (pos.x < 0.2f & pos.y < 0.3f) { touch_gas = false; touch_stop = true; }
                if (pos.x < 0.8f & pos.x > 0.6f & pos.y < 0.3f) { Left(); touch_str = false; if (Input.GetTouch(i).phase == TouchPhase.Ended) { touch_str = true; } }
                else if (pos.x > 0.8f & pos.y < 0.3f) { Right(); touch_str = false; if (Input.GetTouch(i).phase == TouchPhase.Ended) { touch_str = true; } }
                else if (Input.GetTouch(i).phase == TouchPhase.Ended) { touch_gas = false; touch_stop = false; }

            }
            if (Input.touchCount == 0) { touch_gas = false; touch_stop = false; touch_str = true; }

            if (touch_gas) { Gas(); }
            else if (touch_stop) { Tormoz(); }
            else { Neytralka(); }
            if (touch_str) { Straight(); }
        }

        if (gas_pedal >= 0) { invesion = false; }
        if (gas_pedal < 0) { invesion = true; }

    }
    void FixedUpdate() {
            if (invesion)
            {
                speed = -Car_Body.linearVelocity.magnitude;
                if (tankControl) Car_Body.AddTorque(-cursteer*Car_Body.mass*20);
                else Car_Body.AddTorque((cursteer*Car_Body.mass * speed) / (1 + speed * speed * 0.001f));
            }
            else
            {
                speed = Car_Body.linearVelocity.magnitude;
                if (tankControl) Car_Body.AddTorque(cursteer*Car_Body.mass*20);
                else
                {
                    Car_Body.AddTorque((cursteer*Car_Body.mass * speed) / (1 + speed * speed * 0.001f) + speed * Random.Range(-curdestab, +curdestab)*Car_Body.mass);
                    //Car_Body.AddRelativeForce(new Vector2(-cursteer * speed * 0.5f, 0));
                }
            }
            speedometer = Mathf.Round(speed * 5f);

            Car_Body.AddRelativeForce(new Vector2(0, gas_pedal*Car_Body.mass));
    }

    void Gas() 
    {
        if (gas_pedal < 0) { Tormozhenie(); }
        else { Maksimalka(); }
    }
    void Tormoz()
    {
        if (gas_pedal > 0) { Tormozhenie_Nazad(); }
        else if (gas_pedal > min_power) { Razgon_Nazad(); }
        else { Maksimalka_Nazad(); }
    }

    public void Left()
    {
        if (cursteer < -1) cursteer += Time.deltaTime * 50;

        if (cursteer < maxsteer) { cursteer += Time.deltaTime * 20; }
        else
        {
            cursteer = maxsteer;
            if(!tankControl)
            if (speed > 25) { sledLBe.rateOverDistance = 5; sledRBe.rateOverDistance = 5; sledLFe.rateOverDistance = 5; sledRFe.rateOverDistance = 5; }//заносы на скорости при полном повороте //drifts at speed at full turn
            else { sledLFe.rateOverDistance = 0; sledRFe.rateOverDistance = 0; }
        }
    }
    public void Right()
    {
        if (cursteer > 1) cursteer -= Time.deltaTime * 50;

        if (cursteer > -maxsteer) { cursteer -= Time.deltaTime * 20; }
        else
        {
            cursteer = -maxsteer;
            if(!tankControl)
            if (speed > 25) { sledLBe.rateOverDistance = 5; sledRBe.rateOverDistance = 5; sledLFe.rateOverDistance = 5; sledRFe.rateOverDistance = 5; }//заносы на скорости при полном повороте //drifts at speed at full turn
            else { sledLFe.rateOverDistance = 0; sledRFe.rateOverDistance = 0; }
        }
    }
    public void Straight() {
        //cursteer = 0;
        if (cursteer < -1) cursteer += Time.deltaTime * 40; 
        else if (cursteer > 1) cursteer -= Time.deltaTime * 40; 
        else cursteer = 0;
    }
    void Tormozhenie() {
        gas_pedal += Time.deltaTime * 100;
    }

    void Maksimalka()
    {
        gas_pedal = 10 * power1 + speed * 5 * power2 - speed * speed * (0.008f/power3);

        if (curdestab > 0 & speed > 10)
            curdestab -= Time.deltaTime * destabilization*2;//восстановление стабилизации //stabilization recovery
        else curdestab = 0;

        if (power1 > 1 & speed < power1 * 5) { curdestab = destabilization; }
    }
    void Neytralka()
    {
        if (gas_pedal > 3) { gas_pedal -= Time.deltaTime * 20;}
        else if (gas_pedal < -3) { gas_pedal += Time.deltaTime * 30;}
        else { gas_pedal = 0;}

        if (curdestab > 0)
            curdestab -= Time.deltaTime * destabilization;//восстановление стабилизации //stabilization recovery
        else curdestab = 0;
    }
    void Tormozhenie_Nazad()
    {
        gas_pedal -= Time.deltaTime * 120;

        //потеря управления при торможении
        //braking control loss
        if (speed > 15)
        {
            if (curdestab < destabilization)
                curdestab += Time.deltaTime * destabilization * 0.6f;
            else curdestab = destabilization;
        }
    }
    void Razgon_Nazad()
    {
        gas_pedal -= Time.deltaTime * 30;
        curdestab = 0;
    }
    void Maksimalka_Nazad()
    {
        gas_pedal = min_power;
        curdestab = 0;
    }
}