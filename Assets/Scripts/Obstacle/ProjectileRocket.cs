using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Obstacle {
    public class ProjectileRocket : ObstacleBase {
        private float speed = 12f;
        public bool moveUpwards;
        public GameObject prefabAlertRocket;
        private GameObject alertRocket;
        private float seconds;
        private bool rotateUp;

        // Start is called before the first frame update
        void Start() {
            float alertWidth = 0.5f;
            float x = LevelController.FLOOR_WIDTH / 2 - alertWidth;
            alertRocket = Instantiate(prefabAlertRocket, new Vector3(x, this.gameObject.transform.position.y, 0), Quaternion.identity);
        }

        // Update is called once per frame
        void Update() {
            float y = 0;

            Transform transform = this.gameObject.transform;
            Vector3 movement = new Vector3(-speed, y, 0);
            transform.Translate(movement * Time.deltaTime);

            // Rotate rocket
            seconds += Time.deltaTime;

            // Sau 0.5s, ten lua bi quay len/xuong
            // TODO Smooth rotation
            if (seconds >= 0.1f) {
                if (rotateUp) {
                    transform.localEulerAngles = new Vector3(0, 0, -5f);
                }
                else {
                    transform.localEulerAngles = new Vector3(0, 0, 5f);
                }

                rotateUp = !rotateUp;
                seconds = 0f;
            }

            float rocketWidth = 1.5f;

            if (transform.position.x <= LevelController.FLOOR_WIDTH / 2 + rocketWidth) {
                Destroy(alertRocket);
            }
            // if (transform.position.x <= -LevelController.FLOOR_WIDTH / 2 - rocketWidth) {
            //     Destroy(gameObject);
            // }
        }
    }
}