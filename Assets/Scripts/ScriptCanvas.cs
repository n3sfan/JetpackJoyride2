using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Không khởi tạo nữa
        if (GameObject.FindWithTag("Menu") != this.gameObject) {
            Destroy(this.gameObject);
            return;
        }
    }
}
