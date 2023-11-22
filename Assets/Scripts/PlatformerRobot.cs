using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformerRobot : MonoBehaviour
{
    public static float ROBOT_HEIGHT = 1.5f;
    public static float MIN_JUMP_FORCE = 0.05f;
    public static float MAX_JUMP_FORCE = 0.1f;
    private static float[] POWER_LEVEL_JUMP_FORCES = { 0.05f, 0.07f, 0.09f, 0.1f, 0.15f, 0.25f};

    // Thời gian bật jetpack
    private float jumpIncreaseTime = 0f;
    private float lastForceIncreasedTime = 0f;
    private float jumpDecreaseTime = 0f;
    private float lastForceDecreasedTime = 0f;
    private float jumpForce = 0.05f;
    private int powerLevel;
    private bool jumping = false;

    private Rigidbody2D body;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // float deltaX = 0.8f * speed;
        // Vector2 movement = new Vector2(deltaX, body.velocity.y);
        // body.velocity = movement;
        
        // Tăng lực nhảy: 0.006 N / 0.1s
        // Giảm lực nhảy
        // Lực nhảy dựa trên jetpack đã bật hết công suất chưa?

        Debug.Log(jumpForce);

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) {
            // if (jumpForce < MAX_JUMP_FORCE) {
            //     jumpIncreaseTime += Time.deltaTime;

            //     if (jumpIncreaseTime - lastForceIncreasedTime >= 0.1f) {
            //         jumpForce = Math.Min(MAX_JUMP_FORCE, jumpForce + 0.006f);
            //         lastForceIncreasedTime = jumpIncreaseTime;
            //     }
            // }
            jumpIncreaseTime += Time.deltaTime;

            if (jumpForce < MAX_JUMP_FORCE && jumpIncreaseTime - lastForceIncreasedTime >= 0.1f) {
                jumpForce = Math.Min(MAX_JUMP_FORCE, jumpForce + (MAX_JUMP_FORCE - MIN_JUMP_FORCE) / 10);
                lastForceIncreasedTime = jumpIncreaseTime;
            }

            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpDecreaseTime = 0f;
            lastForceDecreasedTime = 0;
        } else {
            jumpDecreaseTime += Time.deltaTime;

            if (jumpForce > MIN_JUMP_FORCE && jumpDecreaseTime - lastForceDecreasedTime >= 0.1f) {
                jumpForce = Math.Max(MIN_JUMP_FORCE, jumpForce - (MAX_JUMP_FORCE - MIN_JUMP_FORCE) / 10);
                lastForceDecreasedTime = jumpDecreaseTime;
            }

            jumpIncreaseTime = 0f;
            lastForceIncreasedTime = 0;
   
            // if (jumpForce > MIN_JUMP_FORCE) {
            //     jumpDecreaseTime += Time.deltaTime;

            //     if (jumpDecreaseTime - lastForceDecreasedTime >= 0.1f) {
            //         float delta;
                    
            //         if (jumpForce < MIN_JUMP_FORCE / 3) {
            //             jumpForce
            //         } else if (jumpForce < MIN_JUMP_FORCE / 3 * 2) {

            //         } else {

            //         }

            //         jumpForce = Math.Max(MIN_JUMP_FORCE, jumpForce - 0.008f);
            //         lastForceDecreasedTime = jumpDecreaseTime;
            //     }
            // }            

            // jumpIncreaseTime = 0f;
        }
    }


    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.tag.Equals("Obstacle")) {
            this.gameObject.GetComponent<Animator>().enabled = false;

            GameObject controller = GameObject.FindWithTag("GameController");
            controller.GetComponent<LevelController>().Stop();
        }
    }
}
