using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Obstacle;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using System.Linq;


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
    public static float SPEED_MULTIPLIER = 2f;

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
    [SerializeField]
    private GameObject prefabFactoryWall;
    [SerializeField]
    private GameObject prefabBackground;

    private float cayChupSeconds;
    private float rocketSeconds;
    private float laserSeconds;
    private float accelerationArc3 = 5.0f;

    /**
    * Trạng thái Màn chơi. 
    */
    public State state;
    private int levelIndex = 1;
    private string[] levelNames = { "LevelFactory", "LevelSea", "LevelLava", "LevelForest", "LevelDesert", "LevelSnow", "LevelGalaxy"};

    /**
    * List chướng ngại vật còn nằm trong Camera.
    */
    private List<GameObject> activeObstacles;

    public float initialSpeed = 5f; // Tốc độ ban đầu của chướng ngại vật
    public float acceleration = 0.5f; // Tốc độ gia tăng
    /**
    * Tốc độ Scroll
    */
    private float scrollSpeed = 2f;

    /**
    * UpdateBackground
    */
    private float scrollSeconds;
    private string nextBackgroundPrefabName = null;
    private GameObject firstBackground;

    public float arcPlaySeconds;

    private float currentSpeed; // Tốc độ hiện tại của chướng ngại vật

    public GameObject Obstacles; // Tham chiếu đến game object của chướng ngại vật
    private GameObject middlegroundGlass;
    private GameObject backgroundFactoryWall;

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

        SceneManager.sceneLoaded += PostSceneLoad;
    }
   
    private void Start()
    {
        this.activeObstacles = new List<GameObject>();
        currentSpeed = initialSpeed;       

        // Các GameObject ko được destroy khi chuyển scene.
        DontDestroyOnLoad(GameObject.FindWithTag("Menu"));
        DontDestroyOnLoad(GameObject.FindWithTag("Robot"));
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(GameObject.Find("EventSystem"));
        DontDestroyOnLoad(GameObject.Find("Jumpfire"));
    }

    private void Update()
    {
        if (this.state == State.PAUSE) {
            return;
        }

        if (this.state == State.PLAYING ) {
            // Gọi hàm pawn tương ứng cho mỗi arc khi trò chơi bắt đầu
            if (SceneManager.GetActiveScene().name == "LevelFactory")
            {
                SpawnArc1();
                //currentSpeed += accelerationArc3 * Time.deltaTime;
            }
            else if (SceneManager.GetActiveScene().name == "LevelSea")
            {
                SpawnArc2();
            }
            else if (SceneManager.GetActiveScene().name == "LevelLava")
            {
                SpawnArc3();
            }
            else if (SceneManager.GetActiveScene().name == "LevelForest")
            {
                PawnArc4();
            }
            else if (SceneManager.GetActiveScene().name == "LevelDesert")
            {
                PawnArc5();
            }
            else if (SceneManager.GetActiveScene().name == "LevelSnow")
            {
                PawnArc6();
            }
            else if (SceneManager.GetActiveScene().name == "LevelGalaxy")
            {
                PawnArc7();
            }
            // SpawnProjectiles();
            // SpawnLaserBeam();
            // SpawnCayChup();
        }

        if (this.state == State.PLAYING || this.state == State.CHANGING_SCENE) {
            UpdateBackground();
            ChangeArc();
        }

        // Tăng tốc độ của chướng ngại vật dần dần
        //currentSpeed += acceleration * Time.deltaTime;

        // Di chuyển chướng ngại vật theo tốc độ hiện tại
        //transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

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

    /* Spawn Arc */
    private void SpawnArc1()
    {
        // Chỉ spawn tên lửa ở arc 1
        SpawnProjectiles();
    }

    private void SpawnArc2()
    {
        // Spawn cả tên lửa và laser ở arc 2
        SpawnProjectiles();
        SpawnLaserBeam();
        SpawnCayChup();
    }

    private void SpawnArc3()
    {
        // Spawn tên lửa, laser và cây chụp ở arc 3
        SpawnProjectiles();
        SpawnLaserBeam();
        SpawnCayChup();
    }

    private void PawnArc4()
    {
        // Spawn tên lửa mức medium
        SpawnProjectilesHard();
        SpawnLaserBeam();
        SpawnCayChup();
    }
    private void PawnArc5()
    {
        // Spawn tên lửa, cây chụp mức medium
        SpawnProjectilesHard();
        SpawnLaserBeam();
        SpawnCayChupMedium();
    }
    private void PawnArc6()
    {
        // Spawn cả 3 tên lửa, laser và cây chụp mức Medium
        SpawnProjectilesHard();
        SpawnLaserBeamMedium();
        SpawnCayChupMedium();
    }
    private void PawnArc7()
    {
        // Spawn tên lửa, laser và cây chụp siêu khó
        SpawnProjectilesHard();
        SpawnLaserBeamHard();
        SpawnCayChupHard();
    }


    /* Scene Load */
    GameObject[] objects;
    int index = -1;
    bool force = false;

    public void ChangeArc(int index = -1) {
        ChangeArc(false, -1);
    }

    public void ChangeArc(bool force, int index = -1) {
        arcPlaySeconds += Time.deltaTime;

        // TODO FIX
        int arcTotalSeconds = 10;

        if (force || arcPlaySeconds >= arcTotalSeconds) {
            this.index = index;
            this.force = force;

            // Mảng này lúc này toàn FactoryWall
            objects = GameObject.FindGameObjectsWithTag("Background");
            // Background ở vị trí phải nhất
            GameObject background = objects[0];
            
            // Đợi tới Frame có Factory Bg
            if (!force && !background.name.StartsWith("FactoryWall")) {
                return;
            }
            
            this.state = State.CHANGING_SCENE;
            
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

    void LoadScene() {
        objects = GameObject.FindGameObjectsWithTag("Background");
        // Giữ cho Scene sau
        foreach (GameObject obj in objects) {
            if (obj != null)
                DontDestroyOnLoad(obj);
        }

        // Màn chuyển là LevelFactory
        if (levelIndex == 3 || index == 1) {
            Destroy(GameObject.FindWithTag("Menu"));
            Destroy(GameObject.FindWithTag("Robot"));
            //Destroy(this.gameObject);
            Destroy(GameObject.Find("EventSystem"));
            Destroy(GameObject.Find("Jumpfire"));

            foreach (GameObject obj in objects) {
                Destroy(obj);
            }
        }

        // Chuyển scene
        if (index == -1) {
            if (levelIndex == 3) {
                // TODO Làm gì khi tới màn cuối.
                levelIndex = 0;
            } 
            
            SceneManager.LoadScene(++levelIndex, LoadSceneMode.Single);
        } else {
            this.levelIndex = index;
            SceneManager.LoadScene(index, LoadSceneMode.Single);
        }

        arcPlaySeconds = 0;
    }

    /**
    * Trả về levelIndex của scene tiếp theo nếu load thành công,
    * Hoặc levelIndex cũ nếu chưa load được.
    **/
    // public static int ChangeScene(int levelIndex) {
    //     GameObject[] objects = GameObject.FindGameObjectsWithTag("Background");
    //     // Background ở vị trí phải nhất
    //     GameObject background = objects[0];
        
    //     // Đợi tới Frame có Factory Bg
    //     if (!background.name.StartsWith("FactoryWall")) {
    //         return levelIndex;
    //     }
       
    //     // Giữ cho Scene sau
    //     foreach (GameObject obj in objects) {
    //         DontDestroyOnLoad(obj);
    //     }

    //     GameObject.Find("TransitionFade").GetComponent<Animator>().SetBool("changing_scene", true);

    //     // Chuyển scene
    //     if (levelIndex == 3) {
    //         // TODO Làm gì khi tới màn cuối.
    //     } else {
    //         SceneManager.LoadScene(++levelIndex, LoadSceneMode.Single);
    //     }

    //     return levelIndex;
    // }

    void PreSceneLoad(int nextLevelIndex) {
        GameObject.Find("TransitionFade").GetComponent<Animator>().SetBool("changing_scene", true);

        // TODO Thêm chỉ số vào sau prefab để dễ thêm
        switch (nextLevelIndex) {
            case 1:
                prefabBackground = (GameObject) Resources.Load("Prefabs/Background/Factory");

                prefabLaser = (GameObject) Resources.Load("Prefabs/Laser");
                prefabLaserBall = (GameObject) Resources.Load("Prefabs/LaserBall");
                prefabRocket = (GameObject) Resources.Load("Prefabs/Rocket");
                prefabCayChup = (GameObject) Resources.Load("Prefabs/CayChup");
                break;
            case 2:
                prefabBackground = (GameObject) Resources.Load("Prefabs/Background/Ocean");
                
                prefabLaser = (GameObject) Resources.Load("Prefabs/Laser2");
                prefabLaserBall = (GameObject) Resources.Load("Prefabs/LaserBall2");
                prefabRocket = (GameObject) Resources.Load("Prefabs/Rocket2");
                prefabCayChup = (GameObject) Resources.Load("Prefabs/CayChup2");
                break;
            case 3:
                prefabBackground = (GameObject) Resources.Load("Prefabs/Background/End");
                
                prefabLaser = (GameObject) Resources.Load("Prefabs/Laser");
                prefabLaserBall = (GameObject) Resources.Load("Prefabs/LaserBall");
                prefabRocket = (GameObject) Resources.Load("Prefabs/Rocket");
                prefabCayChup = (GameObject) Resources.Load("Prefabs/CayChup");
                break;
            default:
                break;
        }
    }

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

        // Từng Arc có các setup riêng
        switch (levelIndex) {
            case 1:
                SPEED_MULTIPLIER = 2f;
                break;
            case 2:
                SPEED_MULTIPLIER = 2.5f;
                break;
            case 3:
                SPEED_MULTIPLIER = 3f;
                break;
            default:
                SPEED_MULTIPLIER = 2f;
                break;
        }

        ProjectileRocket.SPEED = 6f * SPEED_MULTIPLIER;
        LaserBeam.SPEED = 2f * SPEED_MULTIPLIER;
        CayChup.SPEED = 4f * SPEED_MULTIPLIER;

        // Set background của Scene mới 
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Background");
        // Background ở vị trí phải nhất
        GameObject background = objects[objects.Length - 1];
        MovingFloor scriptMovingFloor = background.GetComponent<MovingFloor>();
        scriptMovingFloor.floorPrefab = prefabBackground;

        Invoke("RemoveTransitionChangeScene", levelIndex == 1 ? 0.5f : 0f);

        // Reset background scrolling
        scrollSeconds = 0;
        nextBackgroundPrefabName = null;
        
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
    }

    /* Scrolling Background */
    private void UpdateBackground() {
        GameObject[] tmpObjects = GameObject.FindGameObjectsWithTag("Background");

        if (tmpObjects != null && tmpObjects.Length > 0) {
            firstBackground = tmpObjects[0];

            if (firstBackground != null) {
                // Cho tốc độ scroll của Outside là 1 khi Outside ở vị trí gốc tọa độ (chiếm toàn Camera).
                // Trước đó có thể > 1 nên chỉnh lại.
                // if (firstBackground.name.StartsWith("Background")) {
                //     MovingFloor scriptMovingFloor = firstBackground.GetComponent<MovingFloor>();

                //     if (scriptMovingFloor.moveSpeed != 1f && firstBackground.transform.position.x >= 0f) {
                //         scriptMovingFloor.moveSpeed = 1f;
                //     }
                // }

                if (nextBackgroundPrefabName != null && !firstBackground.name.StartsWith(nextBackgroundPrefabName)) {
                    scrollSeconds = 0;
                } else {
                    nextBackgroundPrefabName = null;
                    scrollSeconds += Time.deltaTime;
                }
            }

            Debug.Log(scrollSeconds + " " + nextBackgroundPrefabName + " fr " + firstBackground.name);
        }

        float changeBackgroundInterval = 10;

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
    
    private void SpawnProjectilesHard() {
        rocketSeconds += Time.deltaTime;

        // Sau 2 giây mới spawn 1 tên lửa
        if (rocketSeconds < 2) {
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

    private void SpawnCayChupHard() {
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

    private void SpawnLaserBeamMedium() {
        laserSeconds += Time.deltaTime;

        // Sau Random(4, 7) giây mới spawn laser beam
        if (laserSeconds < Random.Range(4, 7)) {
            return;
        }

        float minLaserLength = 2f, maxLaserLength = 5f;
        // Random khoảng cách giữa 2 ball
        float distanceToCameraRegion = Random.Range(minLaserLength, maxLaserLength);

        // Random tọa độ ball 1
        Vector3 ballPos1 = new Vector3(MAX_X + distanceToCameraRegion, Random.Range(MIN_PLAY_Y + LaserBeam3.BALL_RADIUS, MAX_PLAY_Y - LaserBeam3.BALL_RADIUS));
        Vector3 ballPos2;
        Quaternion rotation;

        // TODO Tìm ball 2, sao cho vị trí ball 2 nằm trong Camera.
        rotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), new Vector3(0, 0, 1));
        // Tọa độ ball 2 = ballPos1 + vector (1, 0, 0) quay quanh trục z góc theta
        ballPos2 = rotation * Vector3.right * (distanceToCameraRegion - LaserBeam3.BALL_RADIUS) + ballPos1;

        // Frame này ko tìm thấy, để frame sau.
        if (!(MIN_PLAY_Y + LaserBeam3.BALL_RADIUS + PlatformerRobot.ROBOT_HEIGHT <= ballPos2.y && ballPos2.y <= MAX_PLAY_Y - PlatformerRobot.ROBOT_HEIGHT)) {
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
        GameObject laserBeam3 = Instantiate(prefabLaserBeam, laserPos, Quaternion.identity);
        ball1.transform.parent = laserBeam3.transform;
        ball2.transform.parent = laserBeam3.transform;
        laser.transform.parent = laserBeam3.transform;

        this.activeObstacles.Add(laserBeam3);

        laserSeconds = 0;
    }
    private void SpawnLaserBeamHard() {
        laserSeconds += Time.deltaTime;

        // Sau Random(3, 6) giây mới spawn laser beam
        if (laserSeconds < Random.Range(3, 6)) {
            return;
        }

        float minLaserLength = 2f, maxLaserLength = 5f;
        // Random khoảng cách giữa 2 ball
        float distanceToCameraRegion = Random.Range(minLaserLength, maxLaserLength);

        // Random tọa độ ball 1
        Vector3 ballPos1 = new Vector3(MAX_X + distanceToCameraRegion, Random.Range(MIN_PLAY_Y + LaserBeam3.BALL_RADIUS, MAX_PLAY_Y - LaserBeam3.BALL_RADIUS));
        Vector3 ballPos2;
        Quaternion rotation;

        // TODO Tìm ball 2, sao cho vị trí ball 2 nằm trong Camera.
        rotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), new Vector3(0, 0, 1));
        // Tọa độ ball 2 = ballPos1 + vector (1, 0, 0) quay quanh trục z góc theta
        ballPos2 = rotation * Vector3.right * (distanceToCameraRegion - LaserBeam3.BALL_RADIUS) + ballPos1;

        // Frame này ko tìm thấy, để frame sau.
        if (!(MIN_PLAY_Y + LaserBeam3.BALL_RADIUS + PlatformerRobot.ROBOT_HEIGHT <= ballPos2.y && ballPos2.y <= MAX_PLAY_Y - PlatformerRobot.ROBOT_HEIGHT)) {
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
        GameObject laserBeam3 = Instantiate(prefabLaserBeam, laserPos, Quaternion.identity);
        ball1.transform.parent = laserBeam3.transform;
        ball2.transform.parent = laserBeam3.transform;
        laser.transform.parent = laserBeam3.transform;

        this.activeObstacles.Add(laserBeam3);

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

