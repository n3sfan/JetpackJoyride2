using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jumpfire : MonoBehaviour
{
    public GameObject jumpFire;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameObject robot = GameObject.FindGameObjectWithTag("Robot");
        jumpFire.transform.position = new Vector3(robot.transform.position.x, robot.transform.position.y - 0.7f);
    }
}
