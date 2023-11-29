using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformerRobot : MonoBehaviour
{
    public static float ROBOT_HEIGHT = 1.5f;
    public static float MIN_JUMP_FORCE = 0.1f;
    public static float MAX_JUMP_FORCE = 0.2f;
    private static float[] POWER_LEVEL_JUMP_FORCES = { 0.05f, 0.07f, 0.09f, 0.1f, 0.15f, 0.25f};

    // Thời gian bật jetpack
    private float jumpIncreaseTime = 0f;
    private float lastForceIncreasedTime = 0f;
    private float jumpDecreaseTime = 0f;
    private float lastForceDecreasedTime = 0f;
    private float jumpForce = 0.04f;

    private Rigidbody2D body;

    public GameObject[] hearts;
    public int life;

    public GameObject gameOverUI;
    
    private AudioManagerFactory audioManager;

    // //jumpfire
    // public ParticleSystem jumpFire;
    // //Trang thai bay
    // bool isJumping = false;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManagerFactory>();
    }

    // Start is called before the first frame update
    void Start()
    {
        life = hearts.Length;
        body = GetComponent<Rigidbody2D>();
        gameOverUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Tăng lực nhảy: 0.006 N / 0.1s
        // Giảm lực nhảy
        // Lực nhảy dựa trên jetpack đã bật hết công suất chưa?

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) {
            jumpIncreaseTime += Time.deltaTime;

            if (jumpForce < MAX_JUMP_FORCE && jumpIncreaseTime - lastForceIncreasedTime >= 0.1f) {
                float multiple = (jumpIncreaseTime - lastForceIncreasedTime) / 0.1f;
                jumpForce = Math.Min(MAX_JUMP_FORCE, jumpForce + multiple * (MAX_JUMP_FORCE - MIN_JUMP_FORCE) / 10);
                lastForceIncreasedTime = jumpIncreaseTime;
            }

            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpDecreaseTime = 0f;
            lastForceDecreasedTime = 0;

            
        } else {
            jumpDecreaseTime += Time.deltaTime;

            if (jumpForce > MIN_JUMP_FORCE && jumpDecreaseTime - lastForceDecreasedTime >= 1.0f / 25) {
                float multiple = (jumpDecreaseTime - lastForceDecreasedTime) / 0.1f;
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
            if (life >= 1)
            {
                life -= 1;
                Destroy(hearts[life].gameObject);
            }
            if (life < 1)
            {   
                gameOverUI.SetActive(true);
                this.gameObject.GetComponent<Animator>().enabled = false;

                GameObject controller = GameObject.FindWithTag("GameController");
                controller.GetComponent<LevelController>().Stop();

                audioManager.PlaySFX(audioManager.dieSoundClip);
                Destroy(gameObject);
                audioManager.musicAudioSource.Stop();
            }
            
        }
    }  

    // void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.tag.Equals("Ground"))
    //     {
    //         // Code khi Robot chạm đất

    //         isJumping = false; // Đặt lại trạng thái bay khi Robot chạm đất
    //     }
    // }

    // void FixedUpdate()
    // {
    //     // Code kiểm tra Robot đang nhảy

    //     if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow))
    //     {
    //         isJumping = true;
    //         jumpIncreaseTime += Time.deltaTime;

    //         if (jumpForce < MAX_JUMP_FORCE && jumpIncreaseTime - lastForceIncreasedTime >= 0.1f) {
    //             jumpForce = Math.Min(MAX_JUMP_FORCE, jumpForce + (MAX_JUMP_FORCE - MIN_JUMP_FORCE) / 10);
    //             lastForceIncreasedTime = jumpIncreaseTime;
    //         }

    //         body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    //         jumpDecreaseTime = 0f;
    //         lastForceDecreasedTime = 0;
    //     }
    // }
}
