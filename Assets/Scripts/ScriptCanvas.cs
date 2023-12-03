using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCanvas : MonoBehaviour
{
    public String name;

    // Start is called before the first frame update
    void Start()
    {
        // Không khởi tạo nữa
        if (GameObject.Find(name) != this.gameObject) {
            Destroy(this.gameObject);
            return;
        }
    }
}
