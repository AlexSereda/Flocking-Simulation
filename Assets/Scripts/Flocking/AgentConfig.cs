using UnityEngine;
using System.Collections;

public class AgentConfig : MonoBehaviour
{
    //all agents configurations


    public float radCoh;
    public float radSep;
    public float radAlign;
    public float RadAvoid;

    public float CoefCoh;
    public float CoefSep;
    public float CoefAling;
    public float CoefWander;
    public float CoefAvoid;

    public float MaxFieldOfViewAngle = 270;

    public float WanderJitter;
    public float WanderRadius;
    public float WanderDistance;

    public float maxAccel;
    public float maxVel;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
