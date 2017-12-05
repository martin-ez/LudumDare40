using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Bus : MonoBehaviour
{
    [Header("Controls")]
    public Text speedText;
    public Text multiplierText;
    public Image damageIndicator;
    public Image healthBar;
    public Image tacometer;
    public Image gearIndicator;

    [Header("Stats")]
    public Image bg;
    public Text status;
    public Text noPassengers;
    public Text clock;
    public Text[] labels;
    public GameObject[] passengerGroups;

    [Header("End/Start Screen")]
    public Image canvasbg;
    public Text levelText;
    public Text stopsText;
    public Text lengthText;
    public Text countdown;
    public Text options;

    [Header("Level Info")]
    public int level;
    public int noStops;
    public float length;

    [Header("Damage Settings")]
    [Range(0, 1)]
    public float minDamage = 0.1f;
    [Range(0, 1)]
    public float maxDamage = 0.5f;
    public float lowDamageVelocity = 100;
    public float bigDamageVelocity = 500;
    public float damageIndicatorDuration;

    private Rigidbody rb;
    private RearWheelDrive drive;
    private float health = 1f;
    private bool invulnerable = false;
    private float time;
    private int passengers;
    private Color statusColor;
    private StatusMessages statusMes;

    private int difficulty = 0;

    private AudioManager audioManager;
    public GameObject explosion;

    public enum BusState
    {
        Start,
        Drive,
        Pickup,
        Finish
    }
    private BusState state = BusState.Start;

    void Start()
    {
        time = 0;
        passengers = 0;
        statusColor = new Color(73, 238, 21, 1);
        noPassengers.text = (passengers < 10 ? "0" : "") + passengers;
        multiplierText.text = "x1";
        rb = GetComponent<Rigidbody>();
        drive = GetComponent<RearWheelDrive>();
        statusMes = GetComponent<StatusMessages>();
        audioManager = FindObjectOfType<AudioManager>();

        foreach (GameObject group in passengerGroups)
        {
            group.SetActive(false);
        }

        drive.Toggle();
        StartCoroutine(LevelStart());
    }

    void LateUpdate()
    {
        if (state == BusState.Drive) time += Time.fixedDeltaTime;
        int seconds = (int)(time % 60);
        int minutes = (int)((time - seconds) / 60);
        clock.text = (minutes < 10 ? "0" : "") + minutes + ":" + (seconds < 10 ? "0" : "") + seconds;

        float speedlabel = (int)drive.Speed();
        speedText.text = (speedlabel < 0 ? "-" : "") + (Mathf.Abs(speedlabel) < 10 ? "0" : "") + Mathf.Abs(speedlabel);

        float speed = rb.velocity.sqrMagnitude;
        float speedInt = Mathf.InverseLerp(0, drive.maxVelocity, speed);
        tacometer.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(-6, -174, speedInt));
    }

    public BusState GetState()
    {
        return state;
    }

    public void EnterStation()
    {
        state = BusState.Pickup;
        drive.Toggle();
        StartCoroutine(StatsOpen());
        audioManager.PlaySound(AudioManager.Sound.Stop);
    }

    public void ExitStation()
    {
        state = BusState.Drive;
        drive.Toggle();
        StartCoroutine(StatsClose());
        difficulty++;
        SetDifficulty(difficulty);
    }

    public void AddPassenger()
    {
        passengers++;
        status.text = statusMes.GetMessage(passengers);
        noPassengers.text = (passengers < 10 ? "0" : "") + passengers;
    }

    public void FinishLevel()
    {
        drive.Toggle();
        state = BusState.Finish;
        StartCoroutine(LevelEnd(time, passengers));
    }

    public float GetVelocity()
    {
        return rb.velocity.sqrMagnitude;
    }

    public Vector3 GetVelocityDirection()
    {
        return rb.velocity.normalized;
    }

    void SetDifficulty(int level)
    {
        if (level < 4)
        {
            passengerGroups[level].SetActive(true);
            passengerGroups[level].GetComponent<CharacterFlip>().Dance();
            audioManager.SetVolume(level + 1);
            StartCoroutine(GearAnimation());
            multiplierText.text = "x" + (level + 1);

            switch (level)
            {
                case 1:
                    drive.maxTorque = 800;
                    break;
                case 2:
                    drive.maxTorque = 1100;
                    break;
                case 3:
                    drive.maxTorque = 1500;
                    break;
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (!invulnerable && state == BusState.Drive)
        {
            float damage = 0;
            if (col.relativeVelocity.magnitude > bigDamageVelocity)
            {
                damage = maxDamage;
            }
            else if (col.relativeVelocity.magnitude > lowDamageVelocity)
            {
                float dmgInt = Mathf.InverseLerp(lowDamageVelocity, bigDamageVelocity, col.relativeVelocity.magnitude);
                damage = Mathf.Lerp(minDamage, maxDamage, dmgInt);
            }

            if (damage != 0)
            {
                TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (state == BusState.Drive && !invulnerable)
        {
            audioManager.PlaySound(AudioManager.Sound.Crash);
            invulnerable = true;
            damageIndicator.fillAmount = health;
            health -= damage;
            if (health < 0)
            {
                health = 0;
                Die();
            }
            healthBar.fillAmount = health;
            StartCoroutine(DamageRoutine());
        }
    }

    private IEnumerator LevelStart()
    {
        float center = Screen.width / 2;
        float rigth = Screen.width * 2;
        levelText.text = "Level " + level;
        levelText.rectTransform.position = new Vector3(rigth, levelText.rectTransform.position.y, levelText.rectTransform.position.z);
        stopsText.text = noStops + " stops";
        stopsText.rectTransform.position = new Vector3(rigth, stopsText.rectTransform.position.y, stopsText.rectTransform.position.z);
        lengthText.text = length + " km";
        lengthText.rectTransform.position = new Vector3(rigth, lengthText.rectTransform.position.y, lengthText.rectTransform.position.z);
        options.rectTransform.position = new Vector3(rigth, options.rectTransform.position.y, options.rectTransform.position.z);
        canvasbg.fillAmount = 0;
        countdown.gameObject.SetActive(false);
        yield return new WaitForSeconds(1);

        //BG Appear
        float t = 0;
        float time = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.25f, time);
            canvasbg.fillAmount = t;
            yield return null;
        }
        canvasbg.fillAmount = 1;

        Text text = levelText;
        for (int i = 0; i < 3; i++)
        {
            if (i == 1) text = stopsText;
            if (i == 2) text = lengthText;
            //Text Appear
            t = 0;
            time = 0;
            while (t < 1)
            {
                time += Time.deltaTime;
                t = Mathf.InverseLerp(0, 0.75f, time);
                text.rectTransform.position = new Vector3(Mathf.Lerp(rigth, center, t), text.rectTransform.position.y, text.rectTransform.position.z);
                yield return null;
            }
            text.rectTransform.position = new Vector3(center, text.rectTransform.position.y, text.rectTransform.position.z);
            yield return new WaitForSeconds(0.25f);
        }
        yield return new WaitForSeconds(1.75f);
        //Disapear
        t = 0;
        time = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.25f, time);
            levelText.rectTransform.position = new Vector3(Mathf.Lerp(center, -rigth, t), levelText.rectTransform.position.y, levelText.rectTransform.position.z);
            stopsText.rectTransform.position = new Vector3(Mathf.Lerp(center, -rigth, t), stopsText.rectTransform.position.y, stopsText.rectTransform.position.z);
            lengthText.rectTransform.position = new Vector3(Mathf.Lerp(center, -rigth, t), lengthText.rectTransform.position.y, lengthText.rectTransform.position.z);
            canvasbg.fillAmount = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        //Countdown
        countdown.gameObject.SetActive(true);
        countdown.text = "3";
        for (int i = 0; i < 4; i++)
        {
            countdown.fontSize = 0;
            countdown.text = "" + (3 - i);
            if (i == 3) countdown.text = "GO!";
            t = 0;
            time = 0;
            while (t < 1)
            {
                time += Time.deltaTime;
                t = Mathf.InverseLerp(0, 0.48f, time);
                countdown.fontSize = (int)Mathf.Lerp(0f, 300, t);
                yield return null;
            }
            yield return new WaitForSeconds(0.25f);
        }
        state = BusState.Drive;
        drive.Toggle();
        yield return new WaitForSeconds(0.75f);
        //Disapear
        t = 0;
        time = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.25f, time);
            countdown.rectTransform.position = new Vector3(Mathf.Lerp(center, -rigth, t), countdown.rectTransform.position.y, countdown.rectTransform.position.z);
            yield return null;
        }
    }

    private IEnumerator LevelEnd(float timeUsed, float passengersTravel)
    {
        float center = Screen.width / 2;
        float rigth = Screen.width * 2;
        levelText.text = "Level Completed!";
        levelText.rectTransform.position = new Vector3(rigth, levelText.rectTransform.position.y, levelText.rectTransform.position.z);

        int seconds = (int)(timeUsed % 60);
        int minutes = (int)((timeUsed - seconds) / 60);
        string displayTime = (minutes < 10 ? "0" : "") + minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
        stopsText.text = "Time: " + displayTime;
        stopsText.rectTransform.position = new Vector3(rigth, stopsText.rectTransform.position.y, stopsText.rectTransform.position.z);
        lengthText.text = "Passengers: " + passengersTravel;
        lengthText.rectTransform.position = new Vector3(rigth, lengthText.rectTransform.position.y, lengthText.rectTransform.position.z);
        options.rectTransform.position = new Vector3(rigth, options.rectTransform.position.y, options.rectTransform.position.z);
        canvasbg.fillAmount = 0;
        countdown.gameObject.SetActive(false);

        //BG Appear
        float t = 0;
        float time = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.25f, time);
            canvasbg.fillAmount = t;
            yield return null;
        }
        canvasbg.fillAmount = 1;

        Text text = levelText;
        for (int i = 0; i < 4; i++)
        {
            if (i == 1) text = stopsText;
            if (i == 2) text = lengthText;
            if (i == 3) text = options;
            //Text Appear
            t = 0;
            time = 0;
            while (t < 1)
            {
                time += Time.deltaTime;
                t = Mathf.InverseLerp(0, 0.75f, time);
                text.rectTransform.position = new Vector3(Mathf.Lerp(rigth, center, t), text.rectTransform.position.y, text.rectTransform.position.z);
                yield return null;
            }
            text.rectTransform.position = new Vector3(center, text.rectTransform.position.y, text.rectTransform.position.z);
            yield return new WaitForSeconds(0.25f);
        }

        while (true)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            yield return null;
        }
    }

    private IEnumerator DamageRoutine()
    {
        float start = damageIndicator.fillAmount;
        float end = health;
        yield return new WaitForSeconds(0.35f);
        float t = 0;
        float time = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, damageIndicatorDuration, time);
            damageIndicator.fillAmount = Mathf.Lerp(start, end, t);
            yield return null;
        }
        damageIndicator.fillAmount = end;
        invulnerable = false;
    }

    private IEnumerator StatsOpen()
    {
        //BG 
        float time = 0;
        float t = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.75f, time);
            bg.fillAmount = Mathf.Lerp(0.3f, 1f, t);

            yield return null;
        }
        bg.fillAmount = 1f;


        //Labels
        time = 0;
        t = 0;
        Color c = new Color(255, 255, 255);
        Color statusC = statusColor;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.35f, time);

            statusC.a = Mathf.Lerp(0f, 1f, t);
            c.a = Mathf.Lerp(0f, 1f, t);

            status.color = statusC;
            noPassengers.color = c;
            foreach (Text l in labels)
            {
                l.color = c;
            }
            yield return null;
        }
        statusC.a = 1f;
        c.a = 1f;

        status.color = statusC;
        noPassengers.color = c;
        foreach (Text l in labels)
        {
            l.color = c;
        }
    }

    private IEnumerator StatsClose()
    {
        //Labels
        float time = 0;
        float t = 0;
        Color c = new Color(255, 255, 255);
        Color statusC = statusColor;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.35f, time);
            statusC.a = Mathf.Lerp(1f, 0f, t);
            c.a = Mathf.Lerp(1f, 0f, t);
            status.color = statusC;
            noPassengers.color = c;
            foreach (Text l in labels)
            {
                l.color = c;
            }
            yield return null;
        }

        statusC.a = 0f;
        c.a = 0f;
        status.color = statusC;
        noPassengers.color = c;
        foreach (Text l in labels)
        {
            l.color = c;
        }

        //BG 
        time = 0;
        t = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.75f, time);
            bg.fillAmount = Mathf.Lerp(1f, 0.3f, t);
            yield return null;
        }
        bg.fillAmount = 0.3f;
    }

    private IEnumerator GearAnimation()
    {
        float time = 0;
        float t = 0;
        float start = gearIndicator.fillAmount;
        float end = start + 0.25f;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.35f, time);
            gearIndicator.fillAmount = Mathf.Lerp(start, end, t);
            yield return null;
        }

        gearIndicator.fillAmount = end;
    }

    private void Die()
    {
        state = BusState.Finish;
        StopAllCoroutines();
        GameObject exp = Instantiate(explosion);
        exp.transform.position = transform.position;
        Destroy(exp, 1);
        Destroy(transform.Find("Model").gameObject);
        Destroy(transform.Find("Passengers").gameObject);
        StartCoroutine(DeathScreen());
    }

    private IEnumerator DeathScreen()
    {
        float center = Screen.width / 2;
        float rigth = Screen.width * 2;
        levelText.text = "Level Failed!";
        levelText.rectTransform.position = new Vector3(rigth, levelText.rectTransform.position.y, levelText.rectTransform.position.z);
        options.rectTransform.position = new Vector3(rigth, options.rectTransform.position.y, options.rectTransform.position.z);
        canvasbg.fillAmount = 0;
        countdown.gameObject.SetActive(false);

        //BG Appear
        float t = 0;
        float time = 0;
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.InverseLerp(0, 0.25f, time);
            canvasbg.fillAmount = t;
            yield return null;
        }
        canvasbg.fillAmount = 1;

        Text text = levelText;
        for (int i = 0; i < 2; i++)
        {
            if (i == 1) text = options;
            //Text Appear
            t = 0;
            time = 0;
            while (t < 1)
            {
                time += Time.deltaTime;
                t = Mathf.InverseLerp(0, 0.75f, time);
                text.rectTransform.position = new Vector3(Mathf.Lerp(rigth, center, t), text.rectTransform.position.y, text.rectTransform.position.z);
                yield return null;
            }
            text.rectTransform.position = new Vector3(center, text.rectTransform.position.y, text.rectTransform.position.z);
            yield return new WaitForSeconds(0.25f);
        }

        while (true)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            yield return null;
        }
    }
}
