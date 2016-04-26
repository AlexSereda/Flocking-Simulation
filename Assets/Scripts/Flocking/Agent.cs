using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour {

    public Vector3 pos;
    public Vector3 vel;
    public Vector3 accel;
    Vector3 wanderTarget;
    private Quaternion targetRotation;
    public float rotationSpeed = 2f;

    public World world;
    public AgentConfig conf;
    //Rigidbody desireVelocity;

    // Use this for initialization
    void Start() {
       // desireVelocity = GetComponent<Rigidbody>();
        world = FindObjectOfType<World>();
        conf = FindObjectOfType<AgentConfig>();
        pos = transform.position;
        vel = new Vector3(Random.Range(-3, 3), Random.Range(-3, 3), 0);
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.deltaTime;
        accel = Combine();
        accel = Vector3.ClampMagnitude(accel, conf.maxAccel);
        vel = vel + accel * t;
        vel = Vector3.ClampMagnitude(vel, conf.maxVel);
        pos = pos + vel * t;

        //to loop the position
        LoopTheEarth(ref pos, -world.bound, world.bound);

        transform.position = pos;

        //face agent toward the vector direction
        //if (vel.magnitude > 0)
        //    transform.LookAt(pos + vel);
    }
    // === cohesion behavior ===
    Vector3 Cohesion()
    {
        Vector3 a = new Vector3();
        int countAgents = 0;

        // get all agent's nerby neighbors inside radius of flock centroid
        var neighbor = world.getNeighbor(this, conf.radCoh);

        //ckecking if no neighbors nearby
        if (neighbor.Count == 0)
            return a;

        // find the clock centroid
        foreach (var agent in neighbor)
        {
            if (isInFieldOfView(agent.pos))
            {
                a += agent.pos;
                countAgents++;
            }
        }
        if (countAgents == 0)
            return a;
        a /= countAgents;

        //a vector from agent position  towards  the Center Oobjects Mass
        a = a - this.pos;

        a = Vector3.Normalize(a);

        //transform.up = radius;
        return a;
    }

    // Separation behavior
    Vector3 Separation()
    {
        //agents steer from each other in the opposite direction
        Vector3 r = new Vector3();

        //get all neighbors and pass the radius of separation
        var agents = world.getNeighbor(this, conf.radSep);

        //if no nearby agets located, no flee involved
        if (agents.Count == 0)
            return r;



        // add the contribution of each neighbor towards the agent
        //iterate for each of our neighbors
        foreach (var agent in agents)
        {
            if (isInFieldOfView(agent.pos))
            {
                //calculate the vector from neigbor agent to an agent
                //subtracting from agent position the position of the neighbor
                Vector3 towardsMe = this.pos - agent.pos;

                //force
                if (towardsMe.magnitude > 0)
                {
                    r += towardsMe.normalized / towardsMe.magnitude;                    
                }
            }
        }        
        return r.normalized;
    }
    
    Vector3 Alignment()
    {
        Vector3 r = new Vector3();

        //get all neighbors
        var agents = world.getNeighbor(this, conf.radAlign);

        // no neighbors, no alignment employed
        if (agents.Count == 0)

            return r;

        //match direction and speed == match velocity
        //do this for all neighbors
        foreach (var agent in agents)
            if (isInFieldOfView(agent.pos))
                r += agent.vel;
        
        return r.normalized;

    }

    virtual protected Vector3 Combine()
    {
        //combine all behaviors
        //weighted sum
        Vector3 r = conf.CoefCoh * Cohesion() + conf.CoefSep * Separation() + conf.CoefAling * Alignment()
            + conf.CoefWander * Wander() + conf.CoefAvoid * AvoidEnemies();

        transform.up = r;
        return r;
    }
    //Vectors in Unity are a structure, means it is pass as a value to the function, 
    //not as a reference, so in oreder to modify them and see the modifications, REF has to be used

    void LoopTheEarth(ref Vector3 v, float min, float max)
    {
        v.x = LoopTheEarthFloat(v.x, min, max);
        v.y = LoopTheEarthFloat(v.y, min, max);
        //    v.z = LoopTheEarthFloat(v.z, min, max);
    }
    //// when the value is greater than the maximum, asign the min
    float LoopTheEarthFloat(float value, float min, float max)
    {
            //     min...........value............max
           // when the value is greater than the maximum, asign the min
        if (value > max)
            value = min;
           // when the value is less than the maximum, asign the max
        else if (value < min)
            value = max;
        return value;

    }

    //boolean function that returns the the field of view value 
    bool isInFieldOfView(Vector3 faceThis)
    {
        return Vector3.Angle(this.vel, faceThis - this.pos) <= conf.MaxFieldOfViewAngle;
    }

    //face an agent towards the target
    public void Face(Vector3 faceThis)
    {
        Vector3 dir = faceThis - transform.position;
        float rot_z = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        targetRotation = Quaternion.Euler(0f, 0f, rot_z - 90);    
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

  
    protected Vector3 Wander()
    {
        //wander steer behavior
        float jitter = conf.WanderJitter * Time.deltaTime;

        // add a small random vector to the targets position
        // small random Vector will be generated by Binomial 
        wanderTarget += new Vector3(RandomBinomial() * jitter, RandomBinomial() * jitter, 0);

        //reproject the vector back to unit circle
        wanderTarget = wanderTarget.normalized;

        //increase length to be the same as the radius of the wander circle
        wanderTarget *= conf.WanderRadius;

        // position the target in front of the agent
        Vector3 targetInLocalSpace = wanderTarget + new Vector3(0, conf.WanderDistance,0 ); 

        //project the target from the local space to world space
        Vector3 targetInWorldSpace = transform.TransformPoint(targetInLocalSpace);

        //steer towards it        
        Face(targetInWorldSpace);

        return ((targetInWorldSpace-transform.position).normalized*0.5f);

    }

    //implementing the Binomial function with two uniform distributions beetween 0 and 1

    float RandomBinomial()
    {
        return Random.Range(-1f, 1f) - Random.Range(-1f, 1f);
    }

    //flee drom all enemies Behavior
    Vector3 AvoidEnemies()
    { 
        Vector3 e = new Vector3();
    
        var enemies = world.getHunters(this, conf.RadAvoid);// get all the enemies from the world

    //interested only who close to the agent
    if (enemies.Count == 0)
        return e;

    //iterate enemies
    foreach (var enemy in enemies)
        {
            e += Flee(enemy.pos);// sum flee behavior away from each of them
        }
        return e.normalized;
    }

    //implement Flee behavior. Run away in opposite direction from the target
    Vector3 Flee (Vector3 target)
    {
        //get direction subtraction target position from agent position
        Vector3 desiredVelocity = (pos - target).normalized * conf.maxVel;

        //steer agent's velocity
        return desiredVelocity - vel;

    }

    }// Agent class
