using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/**
* Sàn di chuyển lùi. Ra khỏi màn hình sẽ bị xóa. => Sàn mới được spawn tiếp
*/
public class MovingFloor : MonoBehaviour
{
    public Transform floor;
    public GameObject floorPrefab; 

    /**
    * Di chuyển lùi
    */
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

        // Di chuyển lùi
        float x = floor.position.x - moveSpeed * Time.deltaTime;
        floor.position = new Vector3(x, floor.position.y, floor.position.z);
        
        // Not x <= 0 for glitch
        if (this.state == State.INITIAL && x < 0f) {
            //Instantiate(floorPrefab, new Vector3(LevelController.WIDTH / 2, -LevelController.HEIGHT / 2 + LevelController.FLOOR_HEIGHT / 2, 0), Quaternion.identity);
            Instantiate(floorPrefab, new Vector3(LevelController.WIDTH, floor.position.y, 0), Quaternion.identity);
            this.state = State.PAST_ONE_HALF;
        } else if (this.state == State.PAST_ONE_HALF && x <= -LevelController.WIDTH) {
            this.state = State.INVISIBLE;
            Destroy(gameObject);
        }
    }

    /**
    * Trạng thái của Sàn
    */
    public enum State {
        INITIAL,
        PAST_ONE_HALF,
        INVISIBLE
    }
}
