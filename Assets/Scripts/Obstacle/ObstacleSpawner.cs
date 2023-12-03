
using UnityEngine;

public class ObstacleSpawner {

    float oWidth = 1f;
    float oHeight = 1f;
    Vector3 startPos, endPos;
    Vector3 escapePos1, escapePos2;

    void Update() {

        float minLaserLength = 2f, maxLaserLength = 4f;
        // Random khoảng cách giữa 2 ball
        float distanceToCameraRegion = Random.Range(minLaserLength, maxLaserLength);

        // Random tọa độ ball 1
        escapePos1 = new Vector3(LevelController.MAX_X + distanceToCameraRegion, Random.Range(LevelController.MIN_PLAY_Y + PlatformerRobot.ROBOT_HEIGHT, LevelController.MAX_PLAY_Y - PlatformerRobot.ROBOT_HEIGHT));
        
        // Frame này ko tìm thấy, để frame sau.
        if (!isInside(escapePos1)) {
            return;
        }
        
        Quaternion rotation;

        // TODO Tìm ball 2, sao cho vị trí ball 2 nằm trong Camera.
        rotation = Quaternion.AngleAxis(Random.Range(-180f, 180f), new Vector3(0, 0, 1));
        // Tọa độ ball 2 = ballPos1 + vector (1, 0, 0) quay quanh trục z góc theta
        escapePos2 = rotation * Vector3.right * (distanceToCameraRegion - 1.5f) + escapePos1;

        // Frame này ko tìm thấy, để frame sau.
        if (!isInside(escapePos2)) {
            return;
        }

        if (escapePos1.y > escapePos2.y) {
            Vector3 tmp = escapePos1;
            escapePos1 = escapePos2;
            escapePos2 = tmp;
        }

        int type = Random.Range(1, 4);
        int count = 0;
        if (type == 1) {
            count = Random.Range(1, 4);
        } else if (type == 2) {
            count = Random.Range(4, 7);
        } else {
            count = Random.Range(7, 10);
        }
    
        //startPos = new Vector3(Random.Range(LevelController.MAX_X - o));
    }

    
    bool isInside(Vector3 pos) {
        return !(LevelController.MIN_PLAY_Y + oHeight <= pos.y 
                && pos.y <= LevelController.MAX_PLAY_Y - oHeight);
    }
}