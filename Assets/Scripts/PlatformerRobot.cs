using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlatformerRobot : MonoBehaviour
{
    private float jumpForce = 6f;

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

        if (Input.GetKeyDown(KeyCode.Space)) {
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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
