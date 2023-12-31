using System;
using UnityEngine;
using UnityEngine.UI;

public class PlatformerRobot : MonoBehaviour
{
    public static float ROBOT_HEIGHT = 1.5f;
    /**
    * Lực nhảy > Trọng lượng Robot
    */
    public static float MIN_JUMP_FORCE = 50f;
    public static float MAX_JUMP_FORCE = 51.5f;
    private static float[] POWER_LEVEL_JUMP_FORCES = { 0.05f, 0.07f, 0.09f, 0.1f, 0.15f, 0.25f};

    // Thời gian bật jetpack
    private float jumpIncreaseTime = 0f;
    private float lastForceIncreasedTime = 0f;
    private float jumpDecreaseTime = 0f;
    private float lastForceDecreasedTime = 0f;
    private float jumpForce = MIN_JUMP_FORCE;
    [SerializeField]
    private GameObject robotChopChop;

    private Rigidbody2D body;
    
    public GameObject[] hearts;
    public int life;

    public GameObject gameOverUI;

    void Awake()
    {
        
        robotChopChop.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Không khởi tạo nữa
        if (GameObject.FindWithTag("Robot") != this.gameObject) {
            Destroy(this.gameObject);
            return;
        }

        life = hearts.Length;
        body = GetComponent<Rigidbody2D>();
        gameOverUI.SetActive(false);
        robotChopChop.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) {
            jumpIncreaseTime += Time.fixedDeltaTime;

            if (jumpForce < MAX_JUMP_FORCE && jumpIncreaseTime - lastForceIncreasedTime >= 0.1f) {
                float multiple = (jumpIncreaseTime - lastForceIncreasedTime) / 0.1f;
                
                jumpForce = Math.Min(MAX_JUMP_FORCE, jumpForce + multiple * (MAX_JUMP_FORCE - MIN_JUMP_FORCE) / 10);
                lastForceIncreasedTime = jumpIncreaseTime;
            }

           // Debug.Log("Time: " + (jumpIncreaseTime - lastForceDecreasedTime));
            //body.AddForce(new Vector3(0, 0, jumpForce), ForceMode2D.Impulse);
            if (body.velocity.y < 0) {
                // Vận tốc tăng 0.1 m / 0.1 s
                body.AddForce(new Vector2(0, MIN_JUMP_FORCE), ForceMode2D.Force);
            }
            body.AddForce(new Vector2(0, jumpForce), ForceMode2D.Force);
            jumpDecreaseTime = 0f;
            lastForceDecreasedTime = 0;

            //onGround = false;
        } else {
            jumpDecreaseTime += Time.fixedDeltaTime;

            if (jumpForce > MIN_JUMP_FORCE && jumpDecreaseTime - lastForceDecreasedTime >= 1.0f / 25) {
                float multiple = (jumpDecreaseTime - lastForceDecreasedTime) / (1.0f / 25);
                jumpForce = Math.Max(MIN_JUMP_FORCE, jumpForce - multiple * (MAX_JUMP_FORCE - MIN_JUMP_FORCE) / 25);
                lastForceDecreasedTime = jumpDecreaseTime;
            }

            jumpIncreaseTime = 0f;
            lastForceIncreasedTime = 0;

        }


        // Đức thêm phần lửa jumpfire

        // if (transform.position.y < -2.6f)
        // {
        //     isJumping = false; // Nếu tọa độ thấp hơn -2.6, đặt isJumping thành false
        // }

        // if (isJumping)
        // {
        //     // Kích hoạt Particle System khi Robot đang bay
        //     if (!jumpFire.isPlaying)
        //     {
        //         jumpFire.Play();
        //     }
        // }
        // else
        // {
        //     // Tắt Particle System khi Robot không bay
        //     jumpFire.Stop();
        // }
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.tag.Equals("Obstacle")) {
            Invoke("EnableBlink", 0f);
            Invoke("DisableBlink", 0.2f);

            if (life >= 1)
            {
                life -= 1;
            
                //Destroy(hearts[life].gameObject);
                hearts[life].GetComponent<Image>().enabled = false;
            }
            if (life < 1) 
            {
                if (other.gameObject.name.StartsWith("Rocket"))
                {
                    AudioManagerFactory audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManagerFactory>();
                    audioManager.PlaySFX(audioManager.dieSoundClip);
                    audioManager.musicAudioSource.Stop();
                }
                if (other.gameObject.name.StartsWith("Laser"))
                {
                    AudioManagerFactory audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManagerFactory>();
                    audioManager.PlaySFX(audioManager.electricShockClip);
                    audioManager.musicAudioSource.Stop();
                }
                if (other.gameObject.name.StartsWith("CayChup"))
                {
                    AudioManagerFactory audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManagerFactory>();
                    audioManager.PlaySFX(audioManager.brokenEngineClip);
                    audioManager.musicAudioSource.Stop();
                }
                this.gameObject.GetComponent<Animator>().enabled = false;
                GameObject score = GameObject.FindGameObjectWithTag("Timer");
                score.GetComponent<Score>().PauseTimer();

                GameObject controller = GameObject.FindWithTag("GameController");
                controller.GetComponent<LevelController>().Stop();
                
                gameOverUI.SetActive(true);
            }
            
        }
    }  

    private void EnableBlink() {
        robotChopChop.SetActive(true);
    }

    private void DisableBlink() {
        robotChopChop.SetActive(false);
    }
}
