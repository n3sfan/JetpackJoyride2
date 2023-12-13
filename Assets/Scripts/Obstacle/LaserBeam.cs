using UnityEngine;
using UnityEngine.Video;

namespace Obstacle {
    /**
    * GameObject LaserBeam gan voi script nay.
    * Prefab LaserBeam chua 2 ball, 1 laser.
    */
    public class LaserBeam : ObstacleBase { 
        public static float BALL_RADIUS = 0.5f;
        public static float LASER_WIDTH = 2.56f, LASER_HEIGHT = 0.32f;

        // Di chuyển lùi
        public static float SPEED = 2f * LevelController.SPEED_MULTIPLIER;
        // Những biến chưa được sử dụng
        private GameObject ball1, ball2, laser;
        private float seconds;
        private float created;

        void Start() {
            this.ball1 = this.gameObject.transform.GetChild(0).gameObject;
            this.ball2 = this.gameObject.transform.GetChild(1).gameObject;
            this.laser = this.gameObject.transform.GetChild(2).gameObject;
            this.speed = SPEED;
            created = Time.time * 1000;
        }

        void Update() {
            Transform transform = this.gameObject.transform;

            Vector3 movement = new Vector3(-speed, 0, 0);
            transform.Translate(movement * Time.deltaTime, Space.World);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag.Equals("Obstacle")) {
                if (other.gameObject.name.StartsWith("LaserBeam") && other.gameObject.GetComponent<LaserBeam>().created < this.gameObject.GetComponent<LaserBeam>().created) {
                    Destroy(this.gameObject);
                }
            }
         }   
    }
}