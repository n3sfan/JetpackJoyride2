using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Obstacle;
using Unity.VisualScripting;


public class LevelController : MonoBehaviour {
    /**
    * Chiều cao, rộng của vùng Camera.
    */
    public static float HEIGHT;
    public static float WIDTH;
    /**
    * Chiều cao sàn, trần. 
    */
    public static float FLOOR_HEIGHT;
    public static float CEILING_HEIGHT;
    /**
    * Tọa độ min, max của x, y nằm trong Camera.
    */
    public static float MIN_X, MAX_X, MIN_Y, MAX_Y;
    /**
    * Tọa độ min, max y nằm trong vùng chơi (trừ sàn + trần)
    */
    public static float MIN_PLAY_Y, MAX_PLAY_Y;

    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject prefabRocket;
    [SerializeField]
    private GameObject prefabCayChup;
    [SerializeField]
    private GameObject prefabLaserBall;
    [SerializeField]
    private GameObject prefabLaser;
    [SerializeField]
    private GameObject prefabLaserBeam;

    private float cayChupSeconds;
    private float rocketSeconds;
    private float laserSeconds;
    /**
    * Trạng thái Màn chơi. 
    */
    public State state;

    /**
    * List chướng ngại vật còn nằm trong Camera.
    */
    private List<GameObject> activeObstacles;

    public float initialSpeed = 5f; // Tốc độ ban đầu của chướng ngại vật
    public float acceleration = 0.5f; // Tốc độ gia tăng

    private float currentSpeed; // Tốc độ hiện tại của chướng ngại vật

    public GameObject Obstacles ; // Tham chiếu đến game object của chướng ngại vật


    /**
    * Khởi tạo 1 số giá trị toàn cục hay dùng.
    */
    void Awake() {
        // Độ cao của Camera (trong Project để mặc định của Unity là 5)
        HEIGHT = Camera.main.orthographicSize * 2f;
        WIDTH = HEIGHT * Camera.main.aspect;

        FLOOR_HEIGHT = 2;
        CEILING_HEIGHT = 0.6f;

        MIN_X = -WIDTH / 2;
        MAX_X = WIDTH / 2;
        MIN_Y = -HEIGHT / 2;
        MAX_Y = HEIGHT / 2;

        MIN_PLAY_Y = MIN_Y + FLOOR_HEIGHT;
        MAX_PLAY_Y = MAX_Y - CEILING_HEIGHT;
    }
   

    private void Start()
    {
        this.activeObstacles = new List<GameObject>();
        currentSpeed = initialSpeed;
    }

    private void Update()
    {
        if (this.state == State.PLAYING) {
            SpawnProjectiles();
            SpawnLaserBeam();
            SpawnCayChup();
            
        }
            // Tăng tốc độ của chướng ngại vật dần dần
            currentSpeed += acceleration * Time.deltaTime;

            // Di chuyển chướng ngại vật theo tốc độ hiện tại
            transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);


    
        // Xóa Chướng ngại vật sau khi ra khỏi Camera.
        for (int i = this.activeObstacles.Count - 1; i >= 0; --i) {
            GameObject obstacle = this.activeObstacles[i];
            float width;

            if (obstacle.name.StartsWith("LaserBeam")) {
                width = obstacle.transform.GetChild(2).GetComponent<SpriteRenderer>().bounds.size.x + LaserBeam.BALL_RADIUS * 2;
            } else {
                width = obstacle.GetComponent<SpriteRenderer>().bounds.size.x;
            }

            width /= 2; 

            if (obstacle.name.StartsWith("CayChup")) {
                if (obstacle.transform.position.x <= -WIDTH / 2 - 30) {
                    Destroy(obstacle);
                    this.activeObstacles.RemoveAt(i);
                }
            } 
            else if (obstacle.transform.position.x <= -WIDTH / 2 - width) {
                Destroy(obstacle);
                this.activeObstacles.RemoveAt(i);
            } 
        }
    }

    public void Stop() {
        this.state = State.STOPPED;
    }

    /* Spawn Chướng ngại vật */
    private void SpawnProjectiles() {
        rocketSeconds += Time.deltaTime;

        // Sau 3 giây mới spawn 1 tên lửa
        if (rocketSeconds < 3) {
            return;
        }

        // Khởi tạo Tên lửa
        GameObject rocket = Instantiate(prefabRocket);
        float x = WIDTH / 2 + 25;
        // Set vị trí
        rocket.transform.position = new Vector3(x, Random.Range(-2f, 4.5f), 0);
        // Phần thừa, đừng để ý
        ProjectileRocket script = rocket.GetComponent<ProjectileRocket>();
        script.moveUpwards = Random.Range(0, 2) == 0;

        this.activeObstacles.Add(rocket);

        rocketSeconds = 0;
    }

    private void SpawnCayChup() {
        cayChupSeconds += Time.deltaTime;

        // Sau 3 giây mới spawn 1 tên lửa
        if (cayChupSeconds < 3) {
            return;
        }

        GameObject caychup = Instantiate(prefabCayChup);
        float x = -WIDTH / 2 - 25;
        // Set vị trí
        caychup.transform.position = new Vector3(x, Random.Range(-2f, 4.5f), 0);
        // Phần thừa, đừng để ý
        // CayChup script = caychup.GetComponent<CayChup>();
        // script.moveUpwards = Random.Range(0, 2) == 0;

        this.activeObstacles.Add(caychup);

        cayChupSeconds = 0;
    }

    /**
    * Laser Beam = Ball 1, 2 + laser
    */
    private void SpawnLaserBeam() {
        laserSeconds += Time.deltaTime;

        // Sau Random(5, 8) giây mới spawn laser beam
        if (laserSeconds < Random.Range(5, 8)) {
            return;
        }

        float minLaserLength = 2f, maxLaserLength = 5f;
        // Random khoảng cách giữa 2 ball
        float distanceToCameraRegion = Random.Range(minLaserLength, maxLaserLength);

        // Random tọa độ ball 1
        Vector3 ballPos1 = new Vector3(MAX_X + distanceToCameraRegion, Random.Range(MIN_PLAY_Y + LaserBeam.BALL_RADIUS, MAX_PLAY_Y - LaserBeam.BALL_RADIUS));
        Vector3 ballPos2;
        Quaternion rotation;

        // TODO Tìm ball 2, sao cho vị trí ball 2 nằm trong Camera.
        rotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), new Vector3(0, 0, 1));
        // Tọa độ ball 2 = ballPos1 + vector (1, 0, 0) quay quanh trục z góc theta
        ballPos2 = rotation * Vector3.right * (distanceToCameraRegion - LaserBeam.BALL_RADIUS) + ballPos1;

        // Frame này ko tìm thấy, để frame sau.
        if (!(MIN_PLAY_Y + LaserBeam.BALL_RADIUS + PlatformerRobot.ROBOT_HEIGHT <= ballPos2.y && ballPos2.y <= MAX_PLAY_Y - PlatformerRobot.ROBOT_HEIGHT)) {
            return;
        }

        Vector3 laserPos = (ballPos1 + ballPos2) / 2;

        // Khởi tạo ball 1, 2 và tia laser
        GameObject ball1 = Instantiate(prefabLaserBall, ballPos1, Quaternion.identity);
        GameObject ball2 = Instantiate(prefabLaserBall, ballPos2, Quaternion.identity);
        GameObject laser = Instantiate(prefabLaser, laserPos, rotation);
        laser.transform.localScale = new Vector3(Vector3.Distance(ballPos1, ballPos2) * (32f / 128f), laser.transform.localScale.y);

        // Vị trí của 3 GameObject con (ball 1 + 2, laser) sẽ trở nên tương đối với GameObject parent (laserBeam).
        // Tức là laserBeam làm gốc tọa độ của 3 GameObject con
        GameObject laserBeam = Instantiate(prefabLaserBeam, laserPos, Quaternion.identity);
        ball1.transform.parent = laserBeam.transform;
        ball2.transform.parent = laserBeam.transform;
        laser.transform.parent = laserBeam.transform;

        this.activeObstacles.Add(laserBeam);

        laserSeconds = 0;
    }

    /* Trạng thái của Màn chơi */
    public enum State {
        PLAYING,
        STOPPED // Khi Robot va chạm với Obstacle 
    }
}

