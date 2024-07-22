using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotator : MonoBehaviour
{

    public float rotation_speed;

    private float y_rotation;
    
    
    // Start is called before the first frame update
    void Start()
    {
        y_rotation = 0;
    }

    // Update is called once per frame
    void Update()
    {
        y_rotation = y_rotation + Time.deltaTime * rotation_speed;
        Vector3 object_rotation = new Vector3(0, y_rotation, 0);
        transform.localEulerAngles = object_rotation;
        
    }
}
