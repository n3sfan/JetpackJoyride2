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
    public class ProjectileRocket : ObstacleBase {
        // Di chuyển lùi
        public static float SPEED = 6f * LevelController.SPEED_MULTIPLIER;
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
        private float rotateSeconds;

        // Start is called before the first frame update
        void Start() {
            // Cảnh báo tên lửa sắp đến
            float alertWidth = 0.5f;
            float x = LevelController.WIDTH / 2 - alertWidth;
            // Khởi tạo Cảnh báo
            alertRocket = Instantiate(prefabAlertRocket, new Vector3(x, this.gameObject.transform.position.y, 0), Quaternion.identity);
            this.speed = SPEED;
        }

        // Update is called once per frame
        void Update() {
            float y = 0;

            // Di chuyen lui
            Transform transform = this.gameObject.transform;
            Vector3 movement = new Vector3(-speed, y, 0);
            transform.Translate(movement * Time.deltaTime);

            // Rotate rocket
            seconds += Time.deltaTime;
            rotateSeconds += Time.deltaTime;

            // Thời gian tên lửa quay lên hoặc xuống
            float rocketRotationSeconds = 1f / LevelController.SPEED_MULTIPLIER;

            // Sau 0.5s, quay tên lửa lên hoặc xuống (tùy theo lần trước)
            // TODO Smooth rotation
            if (seconds >= 0.1f) {
                float angle = 10f;

                if (rotateUp) {
                    // Neu ten lua da quay len, bay gio quay xuong
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, -angle)), 1 / rocketRotationSeconds * rotateSeconds);
                } else {
                    // Nguoc lai
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), 1 / rocketRotationSeconds * rotateSeconds);
                }

                if (rotateSeconds >= rocketRotationSeconds) {
                    rotateSeconds = 0f;
                }

                // Lần tiếp tên lửa sẽ quay ngược hướng với lúc này
                if (!rotateUp && Mathf.DeltaAngle(transform.localEulerAngles.z, angle) >= 0f) {
                    rotateUp = true;
                } else if (rotateUp && Mathf.DeltaAngle(transform.localEulerAngles.z, -angle) <= 0f) {
                    rotateUp = false;
                }

                seconds = 0f;
            }


            if (transform.position.x <= LevelController.WIDTH / 2 + rocketWidth) {
                Destroy(alertRocket);
            }
            // if (transform.position.x <= -LevelController.FLOOR_WIDTH / 2 - rocketWidth) {
            //     Destroy(gameObject);
            // }
        }
    }
}