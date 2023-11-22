using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Obstacle {
    /**
    * 
    */
    public class CayChup : ObstacleBase {
        // Di chuyển lùi
        private float speed = 12f;
        // TODO: Xem lai bien nay nen su dung cho ten lua nhu the nao?
        public bool moveUpwards;
        public GameObject prefabAlertRocket;
        private GameObject alertRocket;
        // Giây
        private float seconds;
        
        // Lần trước quay lên hay xuống?
        private bool rotateUp;

        // Chiều rộng tên lửa (theo trục x)
        float rocketWidth = 1.5f;

        // Start is called before the first frame update
        void Start() {
            // Cảnh báo tên lửa sắp đến
            float alertWidth = 0.5f;
            float x = -LevelController.WIDTH / 2 + alertWidth;
            // Khởi tạo Cảnh báo
            alertRocket = Instantiate(prefabAlertRocket, new Vector3(x, this.gameObject.transform.position.y, 0), Quaternion.identity);
            //alertRocket = Instantiate(prefabAlertRocket, new Vector3(0, 0, 0), Quaternion.identity);
        }

        // Update is called once per frame
        void Update() {
            float y = 0;

            GameObject robot = GameObject.FindGameObjectWithTag("Robot");

            // Di chuyen toi
            Transform transform = this.gameObject.transform;
            if (transform.position.x > robot.transform.position.x) speed *= -1;

            Vector3 movement = new Vector3(speed, y, 0);
            transform.Translate(movement * Time.deltaTime);

            // Rotate rocket
            seconds += Time.deltaTime;
/*
            // Sau 0.5s, quay tên lửa lên hoặc xuống (tùy theo lần trước)
            // TODO Smooth rotation
            if (seconds >= 0.1f) {
                if (rotateUp) {
                    // Neu ten lua da quay len, bay gio quay xuong
                    transform.localEulerAngles = new Vector3(0, 0, -5f);
                } else {
                    // Nguoc lai
                    transform.localEulerAngles = new Vector3(0, 0, 5f);
                }

                // Lần tiếp tên lửa sẽ quay ngược hướng với lúc này
                rotateUp = !rotateUp;
                seconds = 0f;
            }
*/

            if (transform.position.x >= -LevelController.WIDTH / 2 - rocketWidth) {
                Destroy(alertRocket);
            }
            // if (transform.position.x <= -LevelController.FLOOR_WIDTH / 2 - rocketWidth) {
            //     Destroy(gameObject);
            // }
        }
    }
}