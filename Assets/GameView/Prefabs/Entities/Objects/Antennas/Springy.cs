using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[SelectionBase]//Diverts the selection to this object
public class Springy : MonoBehaviour
{
    public Transform springTarget;
    public Transform springObj;

    [Space(12)]
    public float drag = 4f;//drag
    public float springForce = 100.0f;//Spring

    [Space(12)]
    public Transform geoParent;

    Rigidbody _springRb;

    void Start()
    {
        _springRb = springObj.GetComponent<Rigidbody>();//Find the RigidBody component
        springObj.transform.parent = GameObject.FindGameObjectWithTag("springs").transform; //Take the spring out of the hierarchy
    }

    void FixedUpdate()
    {
        //Sync the rotation
        _springRb.transform.rotation = this.transform.rotation;

        //Calculate the distance between the two points
        var localDistance = springTarget.InverseTransformDirection(springTarget.position - springObj.position);
        _springRb.AddRelativeForce(localDistance * springForce);//Apply Spring

        //Calculate the local velocity of the springObj point
        var localVelocity = springObj.InverseTransformDirection(_springRb.velocity);
        _springRb.AddRelativeForce(-localVelocity * drag);//Apply drag

        //Aim the visible geo at the spring target
        geoParent.transform.LookAt(springObj.position, new Vector3(0, 0, 1));
    }
}
