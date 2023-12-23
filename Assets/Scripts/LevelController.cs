using System.Collections.Generic;
using UnityEngine;
using Obstacle;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour {
    /**
    * Chiều cao, rộng của vùng Camera.
    */
    public static float HEIGHT;
    public static float CAMERA_WIDTH;
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
    public static float SPEED_MULTIPLIER = 2f;

    /**
    * Các prefab ([SerializeField] để Unity hiểu đây là tham số cần truyền trong Editor)
    */
    [SerializeField]
    private GameObject floor;
    [SerializeField]
    public GameObject prefabRocket;
    [SerializeField]
    public GameObject prefabCayChup;
    [SerializeField]
    public GameObject prefabLaserBall;
    [SerializeField]
    public GameObject prefabLaser;
    [SerializeField]
    public GameObject prefabLaserBeam;
    [SerializeField]
    private GameObject prefabFactoryWall;
    [SerializeField]
    private GameObject prefabBackground;

    /**
    * Thời gian giữa các lần spawn chướng ngại vật dưới.
    */
    private float cayChupSeconds;
    private float rocketSeconds;
    private float laserSeconds;

    /**
    * Trạng thái Màn chơi. 
    */
    public State state;
    private int levelIndex = 1;

    /**
    * List chướng ngại vật còn nằm trong Camera.
    */
    public List<GameObject> activeObstacles;

    public float initialSpeed = 5f; // Tốc độ ban đầu của chướng ngại vật
    public float acceleration = 0.5f; // Tốc độ gia tăng

    /**
    * UpdateBackground
    */
    private float scrollSeconds;
    private string nextBackgroundPrefabName = null;
    private GameObject firstBackground;

    public float arcPlaySeconds;
    public GameObject Obstacles; // Tham chiếu đến game object của chướng ngại vật

    /**
    * Spawn chướng ngại vật theo thuật toán.
    */
    ObstacleSpawner spawner;

    /**
    * Khởi tạo 1 số giá trị toàn cục hay dùng.
    */
    void Awake() {
        // Không khởi tạo nữa
        if (GameObject.FindWithTag("GameController") != this.gameObject) {
            Destroy(this.gameObject);
            return;
        }


        // Độ cao của Camera (trong Project để mặc định của Unity là 5)
        WIDTH = 17.7f;

        // Chỉnh Camera
        float ratio = (float)Screen.width / Screen.height;

        Camera.main.orthographicSize = Mathf.Max(5f, 1 / ratio * 18.1f * 0.5f - 0.01f);

        HEIGHT = Camera.main.orthographicSize * 2f;
        CAMERA_WIDTH = HEIGHT * Camera.main.aspect;

        FLOOR_HEIGHT = 2;
        CEILING_HEIGHT = 0.6f;

        MIN_Y = -HEIGHT / 2;
        MAX_Y = HEIGHT / 2;

        MIN_PLAY_Y = MIN_Y + FLOOR_HEIGHT;
        MAX_PLAY_Y = MAX_Y - CEILING_HEIGHT;

        SceneManager.sceneLoaded += PostSceneLoad;
    }

    private void Start() {
        // Chỉnh Camera
        float margin = (CAMERA_WIDTH - 18.1f) / (CAMERA_WIDTH);
        Camera.main.rect = new Rect(margin, 0f, 1f - 2 * margin, 1f);

        WIDTH = 17.7f;

        MIN_X = -WIDTH / 2;
        MAX_X = WIDTH / 2;

        this.activeObstacles = new List<GameObject>();

        // Các GameObject ko được destroy khi chuyển scene.
        DontDestroyOnLoad(GameObject.FindWithTag("Menu"));
        DontDestroyOnLoad(GameObject.FindWithTag("Robot"));
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(GameObject.Find("EventSystem"));
        DontDestroyOnLoad(GameObject.Find("Jumpfire"));

        spawner = new ObstacleSpawner();
        SetupScene();
    }

    private void Update() {
        if (this.state == State.PAUSE) {
            return;
        }

        if (this.state == State.PLAYING) {
            // Gọi hàm pawn tương ứng cho mỗi arc khi trò chơi bắt đầu
            if (SceneManager.GetActiveScene().name == "LevelFactory") {
                SpawnArc1();
            } else if (SceneManager.GetActiveScene().name == "LevelSea") {
                SpawnArc2();
            } else if (SceneManager.GetActiveScene().name == "LevelLava") {
                SpawnArc3();
            }

            spawner.Update();
            // SpawnProjectiles();
            // SpawnLaserBeam();
            // SpawnCayChup();
        }

        if (this.state == State.PLAYING || this.state == State.CHANGING_SCENE) {
            UpdateBackground();
            ChangeArc();
        }

        // Không trong state PLAYING cũng có thể có obstacle.
        // Xóa Chướng ngại vật sau khi ra khỏi Camera.
        for (int i = this.activeObstacles.Count - 1; i >= 0; --i) {
            GameObject obstacle = this.activeObstacles[i];

            if (obstacle == null) {
                this.activeObstacles.RemoveAt(i);
                continue;
            }

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
            } else if (obstacle.transform.position.x <= -WIDTH / 2 - width) {
                Destroy(obstacle);
                this.activeObstacles.RemoveAt(i);
            }
        }
    }


    public void Stop() {
        this.state = State.STOPPED;
    }

    /* Spawn Arc theo vị trí Player */
    private void SpawnArc1() {
        // Chỉ spawn tên lửa ở arc 1
        SpawnProjectiles();
    }

    private void SpawnArc2() {
        // Spawn cả tên lửa và laser ở arc 2
        SpawnProjectiles();
        SpawnLaserBeam();
        SpawnCayChup();
    }

    private void SpawnArc3() {
        // Spawn tên lửa, laser và cây chụp ở arc 3
        SpawnProjectiles();
        SpawnLaserBeam();
        SpawnCayChup();
    }

    /* Scene Load */
    GameObject[] objects;
    int index = -1;
    bool force = false;

    public void ChangeArc(int index = -1) {
        ChangeArc(false, -1);
    }

    public void ChangeArc(bool force, int index = -1) {
        if (!force && levelIndex == 3) {
            return;
        }

        arcPlaySeconds += Time.deltaTime;

        // TODO FIX
        int arcTotalSeconds = 40;

        if (force || arcPlaySeconds >= arcTotalSeconds) {
            this.index = index;
            this.force = force;

            // Mảng này lúc này toàn FactoryWall
            objects = GameObject.FindGameObjectsWithTag("Background");

            if (objects != null) {
                // Background ở vị trí phải nhất
                GameObject background = objects[0];

                this.state = State.CHANGING_SCENE;

                bool allFactoryWall = true;

                foreach (GameObject obj in objects) {
                    if (!obj.name.StartsWith("FactoryWall")) {
                        allFactoryWall = false;
                        break;
                    }
                }

                // Đợi tới Frame có Factory Bg
                if (!force && !allFactoryWall) {
                    return;
                }

                // Còn chướng ngại vật, ko chuyển.
                if (!force && this.activeObstacles.Count > 0) {
                    return;
                }

                if (force) {
                    foreach (GameObject obj in activeObstacles) {
                        Destroy(obj);
                    }
                }

                PreSceneLoad(index != -1 ? index : (levelIndex < 3 ? levelIndex + 1 : 1));
                Invoke("LoadScene", 1.5f);
                // Để không bị gọi lần 2
                arcPlaySeconds = 0;
            }
        }
    }

    void LoadScene() {
        objects = GameObject.FindGameObjectsWithTag("Background");
        // Giữ cho Scene sau
        foreach (GameObject obj in objects) {
            if (obj != null)
                DontDestroyOnLoad(obj);
        }

        // Màn chuyển là LevelFactory, đã có sẵn tất cả các GameObject
        if (index == 1) {
            //Debug.Log("dd");

            //Destroy(GameObject.FindWithTag("Menu"));
            //Destroy(GameObject.FindWithTag("Robot"));
            //Destroy(this.gameObject);

            foreach (GameObject obj in objects) {
                Destroy(obj);
            }
        }

        // Chuyển scene
        if (index == -1) {
            if (levelIndex == 3) {
                // TODO Làm gì khi tới màn cuối.
                //levelIndex = 0;
            } else {
                SceneManager.LoadScene(++levelIndex, LoadSceneMode.Single);
            }
        } else {
            this.levelIndex = index;
            SceneManager.LoadScene(index, LoadSceneMode.Single);
        }

        arcPlaySeconds = 0;
    }

    void SetupScene() {
        this.nextBackgroundPrefabName = prefabBackground.name;
        spawner.Start();
    }

    void PreSceneLoad(int nextLevelIndex) {
        GameObject.Find("TransitionFade").GetComponent<Animator>().SetBool("changing_scene", true);

        // TODO Thêm chỉ số vào sau prefab để dễ thêm
        switch (nextLevelIndex) {
            case 1:
                prefabBackground = (GameObject)Resources.Load("Prefabs/Background/Factory");

                prefabLaser = (GameObject)Resources.Load("Prefabs/Laser");
                prefabLaserBall = (GameObject)Resources.Load("Prefabs/LaserBall");
                prefabRocket = (GameObject)Resources.Load("Prefabs/Rocket");
                prefabCayChup = (GameObject)Resources.Load("Prefabs/CayChup");
                break;
            case 2:
                prefabBackground = (GameObject)Resources.Load("Prefabs/Background/Ocean");

                prefabLaser = (GameObject)Resources.Load("Prefabs/Laser2");
                prefabLaserBall = (GameObject)Resources.Load("Prefabs/LaserBall2");
                prefabRocket = (GameObject)Resources.Load("Prefabs/Rocket2");
                prefabCayChup = (GameObject)Resources.Load("Prefabs/CayChup2");
                break;
            case 3:
                prefabBackground = (GameObject)Resources.Load("Prefabs/Background/End");

                prefabLaser = (GameObject)Resources.Load("Prefabs/Laser");
                prefabLaserBall = (GameObject)Resources.Load("Prefabs/LaserBall");
                prefabRocket = (GameObject)Resources.Load("Prefabs/Rocket");
                prefabCayChup = (GameObject)Resources.Load("Prefabs/CayChup");
                break;
            default:
                break;
        }
    }

    // TODO FIX
    float changeBackgroundInterval = 15f;

    /**
    * Các thiết lập khi chuyển Scene
    */
    void PostSceneLoad(Scene scene, LoadSceneMode mode) {
        if (this.state != State.CHANGING_SCENE) {
            return;
        }

        if (this.activeObstacles != null) {
            this.activeObstacles.Clear();
        }

        // Setup các GameObject trong scene mới
        GameObject.FindWithTag("Menu").GetComponent<Canvas>().worldCamera = Camera.main;

        float margin = (CAMERA_WIDTH - 18.1f) / (CAMERA_WIDTH);
        Camera.main.rect = new Rect(margin, 0f, 1f - 2 * margin, 1f);

        // Từng Arc có các setup riêng
        switch (levelIndex) {
            case 1:
                SPEED_MULTIPLIER = 2f;
                spawner.maxObstacleCount = 5;
                spawner.interval = 1.2f;
                break;
            case 2:
                SPEED_MULTIPLIER = 2.5f;
                spawner.maxObstacleCount = 10;
                spawner.interval = 2f;
                break;
            case 3:
                SPEED_MULTIPLIER = 3f;
                spawner.maxObstacleCount = 14;
                spawner.interval = 1.9f;
                break;
            default:
                SPEED_MULTIPLIER = 2f;
                break;
        }

        ProjectileRocket.SPEED = 6f * SPEED_MULTIPLIER;
        LaserBeam.SPEED = 2f * SPEED_MULTIPLIER;
        CayChup.SPEED = 4f * SPEED_MULTIPLIER;

        // Background
        if (levelIndex != 1 && GameObject.FindWithTag("Background") == null) {
            Instantiate(prefabBackground, new Vector3(-0.03f, 1.33f), Quaternion.identity);
        }

        if ("FactoryWall" == nextBackgroundPrefabName) {
            scrollSeconds = changeBackgroundInterval;
        }

        Invoke("RemoveTransitionChangeScene", levelIndex == 1 ? 1.5f : 0.2f);

        SetupScene();

        this.nextBackgroundPrefabName = prefabBackground.name;
        this.state = State.PLAYING;
    }

    private void RemoveTransitionChangeScene() {
        // Trước đó GameOver, reset điểm về 0
        if (force) {
            GameObject.FindWithTag("Timer").GetComponent<Score>().timeValue = 0;
            GameObject.FindWithTag("Timer").GetComponent<Score>().score = 0;
        }

        GameObject.Find("TransitionFade").GetComponent<Animator>().SetBool("changing_scene", false);

        // Xóa Controller mới của Level 1
        GameObject[] objects = GameObject.FindGameObjectsWithTag("GameController");

        foreach (GameObject obj in objects) {
            if (obj != this.gameObject) {
                Destroy(obj);
                break;
            }
        }

        // Chỉnh Camera
        float ratio = (float)Screen.width / Screen.height;
        Camera.main.orthographicSize = Mathf.Max(5f, 1 / ratio * 18.1f * 0.5f - 0.01f);
        float margin = (CAMERA_WIDTH - 18.1f) / (CAMERA_WIDTH);
        Camera.main.rect = new Rect(margin, 0f, 1f - 2 * margin, 1f);
    }

    /* Scrolling Background */
    private void UpdateBackground() {
        GameObject[] tmpObjects = GameObject.FindGameObjectsWithTag("Background");

        // Điều kiện để scrollSeconds được tăng
        if (tmpObjects != null && tmpObjects.Length > 0 && scrollSeconds < changeBackgroundInterval) {
            firstBackground = tmpObjects[0];

            if (firstBackground != null) {
                if (nextBackgroundPrefabName != null && !firstBackground.name.StartsWith(nextBackgroundPrefabName)) {
                    scrollSeconds = 0;
                } else {
                    scrollSeconds += Time.deltaTime;
                }
            }

            //Debug.Log(scrollSeconds + " " + nextBackgroundPrefabName + " fr " + firstBackground.name);
        }


        // Trình tự: Outside -> Factory Bg, lặp lại.
        if (scrollSeconds >= changeBackgroundInterval) {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Background");

            if (objects != null && objects.Length > 0) {
                // Background ở vị trí phải nhất
                GameObject background = objects[objects.Length - 1];
                MovingFloor scriptMovingFloor = background.GetComponent<MovingFloor>();

                if (background.name.StartsWith("FactoryWall")) {
                    // Đang chuyển scene thì ko đổi bg.
                    if (this.state == State.PLAYING) {
                        scriptMovingFloor.floorPrefab = prefabBackground;
                        // Cho tốc độ scroll của Outside là 2 để đuổi kịp Bg Glass. 
                        scriptMovingFloor.prefabMoveSpeed = 1.5f;

                        nextBackgroundPrefabName = prefabBackground.name;
                    } else if (this.state == State.CHANGING_SCENE) {
                        nextBackgroundPrefabName = prefabFactoryWall.name;
                    }
                } else {
                    scriptMovingFloor.floorPrefab = prefabFactoryWall;
                    // Cho tốc độ scroll của Factory Bg là 2. 
                    scriptMovingFloor.prefabMoveSpeed = 1.5f;

                    nextBackgroundPrefabName = prefabFactoryWall.name;
                }
            }
        }
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
        GameObject robot = GameObject.FindGameObjectWithTag("Robot");
        float y = robot.transform.position.y;
        float y1 = y - 1f;
        if (y1 < -2f) y1 = -2f;
        float y2 = y + 1f;
        if (y2 > 4.5f) y2 = 4.5f;
        // Set vị trí
        rocket.transform.position = new Vector3(x, Random.Range(y1, y2), 0);
        // Phần thừa, đừng để ý
        ProjectileRocket script = rocket.GetComponent<ProjectileRocket>();
        script.moveUpwards = Random.Range(0, 2) == 0;

        this.activeObstacles.Add(rocket);

        rocketSeconds = 0;
    }

    private void SpawnCayChup() {
        cayChupSeconds += Time.deltaTime;

        // Sau 5 giây mới spawn 1 tên lửa
        if (cayChupSeconds < 5) {
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

    private void SpawnCayChupMedium() {
        cayChupSeconds += Time.deltaTime;

        // Sau 4 giây mới spawn 1 tên lửa
        if (cayChupSeconds < 4) {
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
        if (laserSeconds < Random.Range(10, 12)) {
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
        /** 
        * Từ khi arcPlaySeconds >= arcTotalSeconds đến trước khi Scene mới đã được load
        * Không có chướng ngại vật nào trong trạng thái này.
        */
        CHANGING_SCENE,
        PAUSE,
        STOPPED // Khi Robot va chạm với Obstacle 
    }
}

