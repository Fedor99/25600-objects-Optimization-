using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class AsteroidsManagement : MonoBehaviour {

    // Reference to Player class
    public Player player;

    const int initialNumberOfAsteroids = 25600;
    int sqrtNumberOfAsteroids;

    // Prefab of arteroid
    public GameObject asteroid;
    // Prefab of bullet
    public GameObject bullet;
    float bulletSpeed;
    // Camera to get it`s view frustum
    public Camera playerCamera;
    Vector3 playerCameraVectorPosition;

    public Transform playerPosition;
    Vector3 playerVectorPosition;

    // Asteroid data
    Vector3[] asteroidPosition;
    Vector2[] asteroidVector;
    int[][][] section;
    GameObject[] asteroids;
    List<int> destroyAsteroidIndexes;
    // Defines how long asteroid is disabled
    float[] asteroidTime;

    //Bullet data
    List<BulletManager> bullets;
    List<int> destroyBulletIndexes;

    // Screen
    float leftXFromCamera;
    float topYFromCamera;
    float rightXFromCamera;
    float bottomYFromCamera;
    Vector2 lastScreenSize;

    void Start() {
        playerCameraVectorPosition = playerCamera.transform.position;
        playerVectorPosition = playerPosition.position;
        destroyAsteroidIndexes = new List<int>();
        GenerateAsteroidMap();
        ScreenSizeChanged();
    }

    void ScreenSizeChanged() {
        Vector3 camPos = playerCamera.transform.position;
        {
            // Top Left angle
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(0, 0));
            leftXFromCamera = ray.GetPoint(0.0f).x - camPos.x;
            topYFromCamera = ray.GetPoint(0.0f).y - camPos.y;
        }

        {
            // Bottom Right angle
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width, Screen.height));
            rightXFromCamera = ray.GetPoint(0.0f).x - camPos.x;
            bottomYFromCamera = ray.GetPoint(0.0f).y - camPos.y;
        }
    }

    /// <summary>
    /// returns position that is outside of the player view frustum with Z at zero
    /// </summary>
    Vector3 GetInvisiblePosition()
    {
        // Free space near screen border
        int freeSpace = 5;
        Vector3 camPos = playerCameraVectorPosition;
        float leftX = leftXFromCamera + camPos.x;
        float topY = topYFromCamera + camPos.y;
        float rightX = rightXFromCamera + camPos.x;
        float bottomY = bottomYFromCamera + camPos.y;
        
        int maxPosOffset = (int)((sqrtNumberOfAsteroids / 2) - Mathf.Max(rightX - playerCameraVectorPosition.x, 
                                            topY - playerCameraVectorPosition.y));
        
        Vector3 val = new Vector3();

        // Set X
        switch (Random.Range(0, 2)) {
            case 0:
                val.x = Random.Range(leftX - freeSpace - maxPosOffset, leftX - freeSpace);
                break;
            case 1:
                val.x = Random.Range(rightX + freeSpace, rightX + freeSpace + maxPosOffset);
                break;
            default:
                break;
        }

        // Set Y
        switch (Random.Range(0, 2))
        {
            case 0:
                val.y = Random.Range(bottomY - freeSpace - maxPosOffset, bottomY - freeSpace);
                break;
            case 1:
                val.y = Random.Range(topY + freeSpace, topY + freeSpace + maxPosOffset);
                break;
            default:
                break;
        }

        return val;
    }

    Vector2 GetNewAsteroidVector() {
        return new Vector2(Random.Range(-1.0f * GetAsteroidSpeed(), 
                            1.0f * GetAsteroidSpeed()), 
                            Random.Range(-1.0f * GetAsteroidSpeed(), 
                            1.0f * GetAsteroidSpeed()));
    }

    float GetAsteroidSpeed() {
        return Random.Range(0.001f, 0.5f);
    }

    bool IsVisible(Vector3 asteroid) {
        float dist = 2.5f;
        Vector3 camPos = playerCamera.transform.position;
        float leftX = leftXFromCamera + camPos.x;
        float topY = topYFromCamera + camPos.y;
        float rightX = rightXFromCamera + camPos.x;
        float bottomY = bottomYFromCamera + camPos.y;

        if (asteroid.x > leftX - dist &&
            asteroid.x < rightX + dist &&
            asteroid.y > bottomY - dist &&
            asteroid.y < topY + dist)
            return true;

        return false;
    }

    public void Shoot(Vector3 pos, Quaternion rot) {
        bullets.Add(new BulletManager(Instantiate(bullet, pos, rot)));
        //bullets.Add(new BulletManager(bullet));

    }

    void Update() {

        playerCameraVectorPosition = playerCamera.transform.position;
        playerVectorPosition = playerPosition.position;

        if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y) {
            ScreenSizeChanged();
            lastScreenSize.x = Screen.width;
            lastScreenSize.y = Screen.height;
        }

        if (player.isPlaying)
        {
            // Section
            section = new int[sqrtNumberOfAsteroids][][];
            for (int i = 0; i < sqrtNumberOfAsteroids; i++)
            {
                section[i] = new int[sqrtNumberOfAsteroids][];
                for (int a = 0; a < sqrtNumberOfAsteroids; a++)
                {
                    section[i][a] = new int[0];
                }
            }

            // Moove bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].time += Time.deltaTime;
                if (bullets[i].time > 3)
                {
                    Destroy(bullets[i].bulletObject);
                    bullets.RemoveAt(i);
                }
                else
                {
                    bullets[i].bulletObject.transform.Translate(Vector3.up * Time.deltaTime * bulletSpeed, Space.Self);
                }
            }

            //TODO: Testing
            /*
            float delTime = Time.deltaTime;
            new Thread(delegate () {
                MooveAsteroids(0, 10, delTime);
            }).Start();
            */

            // Destroy asteroid by indexes
            for (int d = 0; d < destroyAsteroidIndexes.Count; d++) {
                Destroy(asteroids[destroyAsteroidIndexes[d]]);
            }

            /*
            for (int i = 0; i < initialNumberOfAsteroids; i++)
            {
                // Instantiate visible asteroids   
                if (IsVisible(asteroidPosition[i]))
                {
                    if (asteroids[i] == null)
                    {
                        
                        asteroids[i] = Instantiate(asteroid, new Vector3(asteroidPosition[i].x,
                                                                    asteroidPosition[i].y, 0),
                                                                    asteroid.transform.rotation);
                                                                    
                    }
                    else
                    {
                        asteroids[i].transform.position = asteroidPosition[i];
                    }
                }
                else
                {
                    // Destroy asteroid in scene
                    Destroy(asteroids[i]);
                    asteroids[i] = null;
                }
            }
            */

            // Moove asteroids
            
            for (int i = 0; i < initialNumberOfAsteroids; i++)
            {
                Vector3 asteroidPos = asteroidPosition[i];

                // pos Z = -1 if asteroid is disabled (for 1 second)
                if (asteroidPos.z != -1)
                {
                    float asteroidPosX = asteroidPosition[i].x;
                    float asteroidPosY = asteroidPosition[i].y;

                    asteroidPosition[i].x = asteroidPosX + Time.deltaTime * asteroidVector[i].x;
                    asteroidPosition[i].y = asteroidPosY + Time.deltaTime * asteroidVector[i].y;

                    // Position related to Camera
                    float relatedX = playerCamera.transform.position.x - asteroidPosX;
                    float relatedY = playerCamera.transform.position.y - asteroidPosY;

                    int offset = (sqrtNumberOfAsteroids / 2) - 1;
                    int thisX = (int)relatedX + offset;
                    int thisY = (int)relatedY + offset;

                    //If out of area
                    if (thisX >= sqrtNumberOfAsteroids || thisX < 0 || thisY >= sqrtNumberOfAsteroids || thisY < 0)
                    {
                        asteroidPosition[i] = GetInvisiblePosition();
                    }
                    else
                    {
                        int len = section[thisX][thisY].Length;
                        int[] inSection = section[thisX][thisY];
                        int[] newSection = new int[len + 1];
                        for (int n = 0; n < len; n++)
                        {
                            newSection[n] = inSection[n];
                        }
                        // Assign index of this asteroid in section
                        newSection[len] = i;
                        section[thisX][thisY] = newSection;

                        if (asteroids[i] != null)
                        {

                            // Check for colliding with bullets
                            for (int c = 0; c < bullets.Count; c++)
                            {
                                // Bulet with asteroid
                                if (Vector2.Distance(asteroids[i].transform.position, bullets[c].bulletObject.transform.position) < 0.25f)
                                {
                                    Destroy(asteroids[i]);

                                    // Increase player score by one
                                    player.IncreaseScore(1);

                                    Destroy(bullets[c].bulletObject);
                                    bullets.RemoveAt(c);

                                    // Recreate asteroid
                                    asteroidPosition[i].z = -1;
                                }
                            }

                            // Player with asteroid
                            if (Vector2.Distance(asteroids[i].transform.position, player.transform.position) < 0.25f)
                            {
                                player.GameOver();
                            }
                        }

                        // Check for colliding with other asteroids 
                        for (int c = 0; c < len; c++)
                        {
                            // Destroy asteroid
                            if (Vector2.Distance(asteroidPosition[i], asteroidPosition[newSection[c]]) < 0.25f)
                            {
                                Destroy(asteroids[i]);
                                Destroy(asteroids[newSection[c]]);

                                // Recreate asteroid
                                asteroidPosition[i].z = -1;
                                asteroidPosition[newSection[c]].z = -1;
                            }
                        }
                    }

                    // Instantiate visible asteroids   
                    if (IsVisible(asteroidPosition[i]))
                    {
                        if (asteroids[i] == null)
                        {
                            
                            asteroids[i] = Instantiate(asteroid, new Vector3(asteroidPosition[i].x,
                                                                        asteroidPosition[i].y, 0),
                                                                        asteroid.transform.rotation);
                                                                        
                                                                        
                        }
                        else
                        {
                            asteroids[i].transform.position = asteroidPosition[i];
                        }
                    }
                    else
                    {
                        // Destroy asteroid in scene
                        Destroy(asteroids[i]);
                        asteroids[i] = null;
                    }
                }
                else {
                    asteroidTime[i] += Time.deltaTime;
                    // After one second re-spawn
                    if (asteroidTime[i] > 1) {
                        asteroidPosition[i].z = 0;
                        asteroidPosition[i] = GetInvisiblePosition();
                        asteroidTime[i] = 0;
                    }
                }
            }
            
        }
    }

    public void GenerateAsteroidMap() {
        bullets = new List<BulletManager>();
        bulletSpeed = player.bulletSpeed;
        sqrtNumberOfAsteroids = (int)(Mathf.Sqrt(initialNumberOfAsteroids));
        lastScreenSize = new Vector2(Screen.width, Screen.height);
        asteroidPosition = new Vector3[initialNumberOfAsteroids];
        asteroidVector = new Vector2[initialNumberOfAsteroids];

        float from = -(sqrtNumberOfAsteroids / 2);
        float to = (sqrtNumberOfAsteroids / 2);

        int counter = 0;
        for (float x = from; x < to; x++)
        {
            for (float y = from; y < to; y++)
            {
                asteroidPosition[counter] = new Vector3(x, y);
                asteroidVector[counter] = GetNewAsteroidVector();
                counter++;
            }
        }

        // Array of asteroid game objects
        asteroids = new GameObject[initialNumberOfAsteroids];
        // Time for each asteroid
        asteroidTime = new float[initialNumberOfAsteroids];
        for (int i = 0; i < initialNumberOfAsteroids; i++)
        {
            asteroids[i] = null;
            asteroidTime[i] = 0;
        }
    }

    // Destroy all created objects in scene
    public void Clear()
    {
        for (int i = 0; i < asteroids.Length; i++)
            Destroy(asteroids[i]);
        for (int b = 0; b < bullets.Count; b++)
            Destroy(bullets[b].bulletObject);
    }
}

class BulletManager {
    public float time = 0;
    public GameObject bulletObject;

    public BulletManager(GameObject bulletObject)
    {
        this.bulletObject = bulletObject;
    }
}
