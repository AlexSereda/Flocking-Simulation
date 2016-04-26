using UnityEngine;
using System.Collections;

public class Hunter : Agent
{
    //Class hunter gets the return value of wander from the combine function in Agent class
    override protected Vector3 Combine()
    {
        return conf.CoefWander * Wander();
    }
}
