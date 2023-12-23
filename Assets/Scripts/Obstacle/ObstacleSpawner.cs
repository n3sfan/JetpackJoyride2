
using System.Collections.Generic;
using UnityEngine;
using Obstacle;

/**
* Spawn chướng ngại vật theo thuật toán.
*/ 
public class ObstacleSpawner {

    float escapeHeight = 1f;
    Vector3 startPos = Vector3.zero, endPos = Vector3.zero;
    Vector3 escapePos1 = Vector3.zero, escapePos2 = Vector3.zero;
    List<Vector3> spawnPos;
    LevelController controller;

    float seconds = 0;

    public int maxObstacleCount = 5;
    public float interval = 1;

    public void Start() {
        controller = GameObject.FindWithTag("GameController").GetComponent<LevelController>();
        startPos = endPos = escapePos1 = escapePos2 = Vector3.zero;
        spawnPos = new List<Vector3>();
        spawnPos.Clear();
    }

    public void Update() {
        seconds += Time.deltaTime;

        // TODO FIX
        if (seconds < Random.Range(interval, interval + 2)) {
            return;
        }

        //Debug.Log(startPos.y + " " + endPos.y + " " + escapePos1.y + " " + escapePos2.y + " " + spawnPos.Count);

        if (spawnPos.Count > 0) {
            for (int i = spawnPos.Count - 1; i >= 0; --i) {
                int tp = Random.Range(0, 4);

                if (tp == 0) {
                    Rocket(spawnPos[i].y);
                } else if (tp == 1) {
                    SpawnLaserBeam(spawnPos[i].y);
                } else if (tp == 2) {
                    //SpawnCayChup(spawnPos[i].y);
                }

                spawnPos.RemoveAt(i);
            }
            
            return;
        }

        if (escapePos1 == Vector3.zero) {
            escapePos1 = new Vector3(1, Random.Range(-LevelController.MIN_PLAY_Y + escapeHeight, LevelController.MAX_PLAY_Y - escapeHeight));
        
            if (!isInside(escapePos1)) {
                escapePos1 = Vector3.zero;
                return;
            }    
        }

        if (escapePos2 == Vector3.zero) {
            if (Random.Range(0, 2) == 0) {
                escapePos2 = new Vector3(1, Random.Range(escapePos1.y + escapeHeight, LevelController.MAX_PLAY_Y - escapeHeight));
            } else {
                escapePos2 = new Vector3(1, Random.Range(-LevelController.MIN_PLAY_Y + escapeHeight, escapePos1.y - escapeHeight));
            }

             if (!isInside(escapePos2)) {
                escapePos2 = Vector3.zero;
                return;
            } 
        }

        if (escapePos1.y > escapePos2.y) {
            Vector3 tmp = escapePos1;
            escapePos1 = escapePos2;
            escapePos2 = tmp;
        }

        if (escapePos1 != Vector3.zero && startPos == Vector3.zero) {
            startPos = new Vector3(1, Random.Range(-LevelController.MIN_PLAY_Y + escapeHeight, escapePos1.y - escapeHeight));
            endPos = new Vector3(1, Random.Range(escapePos2.y + escapeHeight, LevelController.MAX_PLAY_Y - escapeHeight));
        
            if (startPos.y > escapePos1.y || endPos.y < escapePos2.y) {
                startPos = endPos = escapePos1 = escapePos1 = Vector3.zero;
                return;
            }
        }

        int count = Random.Range(1, maxObstacleCount);
        //Debug.Log(count);

        while (count > 0) {
            int i = Random.Range(0, 2);

            if (i == 0)
                spawnPos.Add(new Vector3(1, Random.Range(startPos.y + escapeHeight, escapePos1.y - escapeHeight)));
            else
                spawnPos.Add(new Vector3(1, Random.Range(escapePos2.y + escapeHeight, endPos.y - escapeHeight)));

            --count;
        }

        seconds = 0;
    }

    private void Rocket(float y) {
        // Frame này ko tìm thấy, để frame sau.
        if (!(LevelController.MIN_PLAY_Y + ProjectileRocket.rocketHeight + PlatformerRobot.ROBOT_HEIGHT <= y && y <= LevelController.MAX_PLAY_Y -  ProjectileRocket.rocketHeight)) {
            return;
        }

        GameObject rocket = GameObject.Instantiate(controller.prefabRocket);
        float x = LevelController.WIDTH / 2 + 25;
        GameObject robot = GameObject.FindGameObjectWithTag("Robot");
        // float y = robot.transform.position.y;
        // float y1 = y - 1f;
        // if (y1 < -2f) y1 = -2f;
        // float y2 = y + 1f;
        // if (y2 > 4.5f) y2 = 4.5f;
        // Set vị trí
        rocket.transform.position = new Vector3(x, y, 0);
        // Phần thừa, đừng để ý
        ProjectileRocket script = rocket.GetComponent<ProjectileRocket>();
        script.moveUpwards = Random.Range(0, 2) == 0;

        controller.activeObstacles.Add(rocket);
    }

     private void SpawnLaserBeam(float y1) {
        float minLaserLength = 2f, maxLaserLength = 5f;
        // Random khoảng cách giữa 2 ball
        float distanceToCameraRegion = Random.Range(minLaserLength, maxLaserLength);

        // Random tọa độ ball 1
        Vector3 ballPos1 = new Vector3(LevelController.MAX_X + distanceToCameraRegion, y1);
        Vector3 ballPos2;
        Quaternion rotation;

        // TODO Tìm ball 2, sao cho vị trí ball 2 nằm trong Camera.
        rotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), new Vector3(0, 0, 1));
        // Tọa độ ball 2 = ballPos1 + vector (1, 0, 0) quay quanh trục z góc theta
        ballPos2 = rotation * Vector3.right * (distanceToCameraRegion - LaserBeam.BALL_RADIUS) + ballPos1;

        // Frame này ko tìm thấy, để frame sau.
        if (!(LevelController.MIN_PLAY_Y + LaserBeam.BALL_RADIUS <= Mathf.Min(ballPos1.y, ballPos2.y) && Mathf.Max(ballPos1.y, ballPos2.y) <= LevelController.MAX_PLAY_Y - LaserBeam.BALL_RADIUS)) {
            return;
        }

        Vector3 laserPos = (ballPos1 + ballPos2) / 2;

        // Khởi tạo ball 1, 2 và tia laser
        GameObject ball1 = GameObject.Instantiate(controller.prefabLaserBall, ballPos1, Quaternion.identity);
        GameObject ball2 = GameObject.Instantiate(controller.prefabLaserBall, ballPos2, Quaternion.identity);
        GameObject laser = GameObject.Instantiate(controller.prefabLaser, laserPos, rotation);
        laser.transform.localScale = new Vector3(Vector3.Distance(ballPos1, ballPos2) * (32f / 128f), laser.transform.localScale.y);

        // Vị trí của 3 GameObject con (ball 1 + 2, laser) sẽ trở nên tương đối với GameObject parent (laserBeam).
        // Tức là laserBeam làm gốc tọa độ của 3 GameObject con
        GameObject laserBeam = GameObject.Instantiate(controller.prefabLaserBeam, laserPos, Quaternion.identity);
        ball1.transform.parent = laserBeam.transform;
        ball2.transform.parent = laserBeam.transform;
        laser.transform.parent = laserBeam.transform;

        //Debug.Log("ok");

        controller.activeObstacles.Add(laserBeam);
    }
    
    bool isInside(Vector3 pos) {
        return !(LevelController.MIN_PLAY_Y + escapeHeight <= pos.y 
                && pos.y <= LevelController.MAX_PLAY_Y - escapeHeight);
    }
}