using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public StateMachine stateMachine { get; private set; }

    public float initialSpeed;
    public Quaternion initialAngle;
    public float maxAngle { get; private set; } = 80f;
    public float minAngle { get; private set; } = -80f;
    public float maxSpeed; // �ʱ� �ӷ� X, �÷��� �� �ִ� �ӷ�
    public float frontSpeed; // ������

    public Rigidbody rb { get; private set; } // �浹 ó����S
    public bool launchSuccess = false;
    public bool airshipColiide = false;

    public GameObject speedBar { get; private set; }
    public GameObject angleBar { get; private set; }

    public Vector3 velocity = new Vector3 (0, 0, 0);
    public float distance = 0f;
    public float altitude = 1f;
    public float altitudeRatio = 10f; // ���ؾ� ��
    public float fuelTimer = 5f;

    public ParticleSystem electronicParticle; // Nimbus collide
    public ParticleSystem explosionParticle; // Airship collide
    public ParticleSystem fireParticle; // Airship collide

    public LinkedList<AirflowSystem> airflows = new LinkedList<AirflowSystem>();

    private List<Spawner> spawners = new List<Spawner>();
    public bool shieldOn;
    public GameObject shield;

    public float boosterSpeed;
    public bool boosterOn;

    // ����
    public int coinCount;
    public float maxSpeedReached;
    public float maxAltitudeReached;

    public float propellerSpeed { get; private set; }
    private Transform propeller;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        speedBar = GameObject.FindGameObjectWithTag("SpeedBar");
        angleBar = transform.GetChild(0).GetChild(1).gameObject;
        GameManager.instance.SetBoardLength(maxSpeed);
        shield = transform.GetChild(transform.childCount - 2).gameObject;
        propeller = transform.GetChild(2).GetChild(2).transform;

        var mapObjectCount = GameManager.instance.GetComponents<MapObjectSpawner>().Length;
        spawners.Add(GameManager.instance.GetComponent<AirflowSpwaner>());
        spawners[0].enabled = false;
        for(int i = 1; i< mapObjectCount+1; ++i)
        {
            spawners.Add(GameManager.instance.GetComponents<MapObjectSpawner>()[i - 1]);
            spawners[i].enabled = false;
        }
        spawners.Add(GameManager.instance.GetComponent<CoinSpawner>());
        spawners[spawners.Count - 1].enabled = false;
    }

    private void Start()
    {
        InitStateMachine();
    }

    private void FixedUpdate()
    {
        stateMachine?.FixedUpdateState();
        transform.position += velocity * Time.deltaTime;
        RotatePropeller();
    }

    private void Update()
    {
        stateMachine?.UpdateState();

        if(shieldOn)
        {
            shield.SetActive(true);
            Invoke("ShieldRemove", 5f);
        }
        else
        {
            shield.SetActive(false);
        }

        if(boosterOn)
        {
            Booster(boosterSpeed);
            Invoke("BoosterRemove", 5f);
        }

        if(velocity.x > maxSpeedReached)
        {
            maxSpeedReached = velocity.x;
        }
        if(altitude > maxAltitudeReached)
        {
            maxAltitudeReached = altitude;
        }

        // �ΰ��� ���� UI ������Ʈ
        altitude = transform.position.y * altitudeRatio;
        UIManager.instance.UpdateDistanceText(distance);
        UIManager.instance.UpdateVelocityText(velocity.x);
        UIManager.instance.UpdateAltitudeText(altitude);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Jump"))
        {
            // ���� ���� (Ready -> Gliding)
            stateMachine.AddState(StateName.Gliding, new StateGliding(this));
            StateGliding stateGliding = (StateGliding)stateMachine.GetState(StateName.Gliding);
            angleBar.SetActive(false);
            stateMachine?.ChangeState(StateName.Gliding);
            foreach(var s in spawners)
            {
                s.enabled = true;
            }
        }

        if (other.CompareTag("Floor"))
        {
            // ���� ���� ( Gliding -> Landing )
            stateMachine.AddState(StateName.Landing, new StateLanding(this));
            stateMachine?.ChangeState(StateName.Landing);
            foreach (var s in spawners)
            {
                s.spawnStop = true;
            }
        }

        if (other.CompareTag("Airflow"))
        {
            if(!airflows.Contains(other.GetComponent<AirflowSystem>()))
            {
                airflows.AddLast(other.GetComponent<AirflowSystem>());
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Airflow"))
        {
            if (airflows.Contains(other.GetComponent<AirflowSystem>()))
            {
                airflows.Remove(other.GetComponent<AirflowSystem>());
            }
        }
    }

    public void InitStateMachine()
    {
        // original code
        stateMachine = new StateMachine(StateName.Ready, new StateReady(this));
    }

    public void ShieldRemove()
    {
        shieldOn = false;
    }

    public void Booster(float boosterSpeed)
    {
        frontSpeed += boosterSpeed;
    }

    public void BoosterRemove()
    {
        boosterOn = false;
    }

    public void RotatePropeller()
    {
        propeller.Rotate(0, 0, propellerSpeed * Time.deltaTime);
    }

    public void ChangePropellerSpeed(float speed)
    {
        propellerSpeed = speed;
    }
}
