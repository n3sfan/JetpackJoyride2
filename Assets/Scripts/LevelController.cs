using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Obstacle;
using Unity.VisualScripting;


public class LevelController : MonoBehaviour {
    public static float HEIGHT;
    public static float FLOOR_WIDTH;
    public static float FLOOR_HEIGHT;
    public static float MIN_X, MAX_X, MIN_Y, MAX_Y;

    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject prefabRocket;
    [SerializeField]
    private GameObject prefabLaserBall;
    [SerializeField]
    private GameObject prefabLaser;
    [SerializeField]
    private GameObject prefabLaserBeam;

    private float rocketSeconds;
    private float laserSeconds;
    public State state;
    private List<GameObject> activeObstacles;


    void Awake() {
        HEIGHT = Camera.main.orthographicSize * 2f;
        FLOOR_WIDTH = HEIGHT * Camera.main.aspect;
        FLOOR_HEIGHT = 2;
        MIN_X = -FLOOR_WIDTH / 2;
        MAX_X = FLOOR_WIDTH / 2;
        MIN_Y = -HEIGHT / 2;
        MAX_Y = HEIGHT / 2;
    }

    void Start() {
        this.activeObstacles = new List<GameObject>();
    }

    // Update is called once per frame
    void Update() {
        if (this.state == State.STOPPED) {
            return;
        }

        SpawnProjectiles();
        SpawnLaserBeam();

        // Xoa chuong ngai vat sau khi ra khoi Camera
        for (int i = this.activeObstacles.Count - 1; i >= 0; --i) {
            GameObject obstacle = this.activeObstacles[i];
            float width;

            if (obstacle.name.StartsWith("LaserBeam")) {
                width = obstacle.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite.bounds.size.x / 2 + LaserBeam.BALL_RADIUS * 2;
            }
            else {
                width = obstacle.GetComponent<SpriteRenderer>().sprite.bounds.size.x / 2;
            }

            if (obstacle.transform.position.x <= -FLOOR_WIDTH / 2 - width) {
                Destroy(obstacle);
                this.activeObstacles.RemoveAt(i);
            }
        }
    }

    public void Stop() {
        this.state = State.STOPPED;
    }

    private void SpawnProjectiles() {
        rocketSeconds += Time.deltaTime;

        if (rocketSeconds < 3) {
            return;
        }

        GameObject rocket = Instantiate(prefabRocket);
        float x = FLOOR_WIDTH / 2 + 25;
        rocket.transform.position = new Vector3(x, Random.Range(-2f, 4.5f), 0);

        ProjectileRocket script = rocket.GetComponent<ProjectileRocket>();
        script.moveUpwards = Random.Range(0, 2) == 0;

        this.activeObstacles.Add(rocket);

        rocketSeconds = 0;
    }

    private void SpawnLaserBeam() {
        laserSeconds += Time.deltaTime;

        if (laserSeconds < Random.Range(5, 8)) {
            return;
        }

        float minLaserLength = 2f, maxLaserLength = 5f;
        float distanceToCameraRegion = Random.Range(minLaserLength, maxLaserLength);

        // Tinh toa do ball 1 = Random
        Vector3 ballPos1 = new Vector3(MAX_X + distanceToCameraRegion, Random.Range(MIN_Y + FLOOR_HEIGHT + LaserBeam.BALL_RADIUS, MAX_Y - LaserBeam.BALL_RADIUS));
        Vector3 ballPos2;
        Quaternion rotation;

        // TODO Tim ballPos2, sao cho ballPos2 luc sau nam trong Camera
        rotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), new Vector3(0, 0, 1));
        // Toa do ball 2 = ballPos1 + vector (1, 0, 0) quay quanh truc z goc theta
        ballPos2 = rotation * Vector3.right * (distanceToCameraRegion - LaserBeam.BALL_RADIUS) + ballPos1;

        //Debug.Log(MIN_Y + FLOOR_HEIGHT + LaserBeam.BALL_RADIUS <= ballPos2.y && ballPos2.y <= MAX_Y - LaserBeam.BALL_RADIUS);
        // Frame nay ko tim duoc ballPos2, de Frame sau.
        if (!(MIN_Y + FLOOR_HEIGHT + LaserBeam.BALL_RADIUS <= ballPos2.y && ballPos2.y <= MAX_Y - LaserBeam.BALL_RADIUS)) {
            return;
        }

        Vector3 laserPos = (ballPos1 + ballPos2) / 2;

        GameObject ball1 = Instantiate(prefabLaserBall, ballPos1, Quaternion.identity);
        GameObject ball2 = Instantiate(prefabLaserBall, ballPos2, Quaternion.identity);
        GameObject laser = Instantiate(prefabLaser, laserPos, rotation);
        laser.transform.localScale = new Vector3(Vector3.Distance(ballPos1, ballPos2), laser.transform.localScale.y);

        // Toa do cac children (ball 1 + 2, laser) se bien thanh relative voi laserBeam
        GameObject laserBeam = Instantiate(prefabLaserBeam, laserPos, Quaternion.identity);
        ball1.transform.parent = laserBeam.transform;
        ball2.transform.parent = laserBeam.transform;
        laser.transform.parent = laserBeam.transform;

        this.activeObstacles.Add(laserBeam);

        laserSeconds = 0;
    }

    public enum State {
        PLAYING,
        STOPPED
    }
}
