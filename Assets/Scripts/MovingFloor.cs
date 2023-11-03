using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MovingFloor : MonoBehaviour
{
    public Transform floor;
    public GameObject floorPrefab; 

    public float moveSpeed;
    public State state;

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 2f;
        state = State.INITIAL;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject controller = GameObject.FindWithTag("GameController");
        if (controller.GetComponent<LevelController>().state == LevelController.State.STOPPED) {
            return;
        }

        float x = floor.position.x - moveSpeed * Time.deltaTime;
        floor.position = new Vector3(x, floor.position.y, floor.position.z);
        
        // Not x <= 0 for glitch
        if (this.state == State.INITIAL && x <= 0.5f) {
            Instantiate(floorPrefab, new Vector3(LevelController.FLOOR_WIDTH / 2, -4, 0), Quaternion.identity);
            this.state = State.PAST_ONE_HALF;
        } else if (this.state == State.PAST_ONE_HALF && x <= -LevelController.FLOOR_WIDTH / 2) {
            this.state = State.INVISIBLE;
            Destroy(gameObject);
        }
    }

    public enum State {
        INITIAL,
        PAST_ONE_HALF,
        INVISIBLE
    }
}
