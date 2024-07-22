using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class forceStrengthUi : MonoBehaviour
{
    public Slider slider_ui;

    public TMP_Text title;

    private AgentForce force;
    
    // Start is called before the first frame update
    void Start()
    {
        force = transform.GetComponent<AgentForce>();

        title.text = force.name;
        slider_ui.value = force.OVERALL_STRENGTH;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignForceStrength(float strength)
    {
        //force.OVERALL_STRENGTH = strength;
    }
}
