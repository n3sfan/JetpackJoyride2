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
        private float speed = 2f;
        // Những biến chưa được sử dụng
        private GameObject ball1, ball2, laser;
        private float seconds;

        void Start() {
            this.ball1 = this.gameObject.transform.GetChild(0).gameObject;
            this.ball2 = this.gameObject.transform.GetChild(1).gameObject;
            this.laser = this.gameObject.transform.GetChild(2).gameObject;
        }

        void Update() {
            Transform transform = this.gameObject.transform;

            Vector3 movement = new Vector3(-speed, 0, 0);
            transform.Translate(movement * Time.deltaTime, Space.World);
        }
    }
}