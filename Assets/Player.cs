using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class Player : MonoBehaviour {

    // Camera
    public Transform cameraTransform;
    public float cameraSpeed = 0.05f;

    // Player
    public int score = 0;
    public float speed = 3.0f;
    public float rotationSpeed = 8.0f;
    public float bulletSpeed = 4.0f;
    public bool isPlaying = true;
    Vector3 initialPosition;
    Vector3 cameraInitialPosition;

    // AsteroidsManagement
    AsteroidsManagement am;
    public GameObject asteroidManagementObject;
    public GameObject asteroid;
    public GameObject bullet;
    public Camera playerCamera;

    // UI
    public Text scoreLabel;
    public Text gameoverScoreLabel;
    public GameObject gameplayUI;
    public GameObject gameoverUI;

    float rotationZ = 0;

    float time = 0;

    Vector2 cameraGameOverTarget;

	// Use this for initialization
	void Start () {
        cameraInitialPosition = cameraTransform.position;
        initialPosition = transform.position;
        cameraGameOverTarget = Random.insideUnitCircle;

        StartGame();

        vec = new Vector3[] { new Vector3(0, 0, 0) };
    }


    public Vector3[] vec;
    void Run(float delTime) {
        vec[0].x += delTime;
    }

    void Update() {
        float d = Time.deltaTime;
        new Thread(delegate () {
            Run(d);
        }).Start();
    }
    

    void FixedUpdate() {
        if (isPlaying)
        {
            time += Time.deltaTime;
            if (time > 0.5f)
            {
                am.Shoot(transform.position, transform.rotation);
                time = 0;
            }

            ChangeCamPos();
            PlayerMovement();
        }
    }

    public void IncreaseScore(int increaseBy = 1) {
        score += increaseBy;
        scoreLabel.text = "score: " + score;
    }

    public void GameOver() {
        isPlaying = false;
        gameplayUI.SetActive(false);
        gameoverScoreLabel.text = scoreLabel.text;
        gameoverUI.SetActive(true);
    }

    // Called from OnClick() and Start()
    public void StartGame() {
        if(am != null)
            am.Clear();
        Destroy(asteroidManagementObject.GetComponent<AsteroidsManagement>());
        am = asteroidManagementObject.AddComponent<AsteroidsManagement>();
        am.player = this;
        am.asteroid = asteroid;
        am.bullet = bullet;
        am.playerCamera = playerCamera;
        am.playerPosition = transform;

        transform.position = initialPosition;
        cameraTransform.position = cameraInitialPosition;
        gameoverUI.SetActive(false);
        gameplayUI.SetActive(true);
        score = 0;
        IncreaseScore(0);
        isPlaying = true;
    }

    void ChangeCamPos() {
        // Position of camera
        Vector3 c = cameraTransform.position;
        // Position of player
        Vector3 p = gameObject.transform.position;
        // Set new camera position
        cameraTransform.position = new Vector3(Mathf.Lerp(c.x, p.x, cameraSpeed), Mathf.Lerp(c.y, p.y, cameraSpeed), c.z);
    }

    void PlayerMovement() {

        // Moove player
        if (Input.GetKey(KeyCode.W))
            transform.Translate(Vector3.up * Time.deltaTime * speed, Space.Self);
        if (Input.GetKey(KeyCode.S))
            transform.Translate(-Vector3.up * Time.deltaTime * speed, Space.Self);

        // Rotate player
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            rotationZ = Mathf.Lerp(rotationZ, rotationSpeed, 0.7f);
        else
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                rotationZ = Mathf.Lerp(rotationZ, -rotationSpeed, 0.7f);
            else
                rotationZ = Mathf.Lerp(rotationZ, 0, 0.7f);
        gameObject.transform.Rotate(0, 0, rotationZ, Space.Self);
    }
}
