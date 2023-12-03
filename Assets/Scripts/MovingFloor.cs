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

    public bool canMove = true;
    /**
    * Di chuyển lùi
    */
    public float moveSpeed, prefabMoveSpeed;
    public State state;

    // Start is called before the first frame update
    void Start()
    {
        state = State.INITIAL;
        floor = this.gameObject.transform;

        if (canMove && moveSpeed == 0f) {
            moveSpeed = 2f;
        }
        if (prefabMoveSpeed == 0f) {
            prefabMoveSpeed = moveSpeed;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) {
            return;
        }

        GameObject controller = GameObject.FindWithTag("GameController");
        if (controller.GetComponent<LevelController>().state == LevelController.State.STOPPED) {
            return;
        }

        // Di chuyển lùi
        float x = floor.position.x - moveSpeed * Time.deltaTime * LevelController.SPEED_MULTIPLIER;
        floor.position = new Vector3(x, floor.position.y, floor.position.z);
        
        // Not x <= 0 for glitch
        // Dự trù 1 ô
        if (this.state == State.INITIAL && x <= 1f) {
            //Instantiate(floorPrefab, new Vector3(LevelController.WIDTH / 2, -LevelController.HEIGHT / 2 + LevelController.FLOOR_HEIGHT / 2, 0), Quaternion.identity);
            GameObject gameObject = Instantiate(floorPrefab, new Vector3(x + LevelController.WIDTH, floor.position.y, 0), Quaternion.identity);
            gameObject.GetComponent<MovingFloor>().moveSpeed = prefabMoveSpeed;

            this.state = State.PAST_ONE_HALF;
        } else if (this.state == State.PAST_ONE_HALF && x <= -LevelController.WIDTH) {
            // Destroy this GameObject to which Script is attached
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
