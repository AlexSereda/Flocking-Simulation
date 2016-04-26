using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour {

    AudioSource GameAudio;
    public AudioClip audioS;
    public int numAgents; // initializing the number of agents
    public int numHunters;

    public Transform agentPrefab;// instantiating the prefabs of the agent
    public Transform hunterPrefab;
    
    public List<Agent> agents;
    public List<Hunter> hunters;

    public float bound;
    public float spawnRadius;

    // Use this for initialization
    void Start ()
    {
        agents = new List<Agent>();
        Spawn(agentPrefab, numAgents);

        agents.AddRange(FindObjectsOfType<Agent>());
        hunters.AddRange(FindObjectsOfType<Hunter>());
        GameAudio = GetComponent<AudioSource>();
        //======checking if sound is on onAwake and turns it ON
        if (!GameAudio.isPlaying)
        GetComponent<AudioSource>().Play();
        GameAudio.PlayOneShot(audioS, 0.65F);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    // spwan function create many instances of the object and place them rendomly in the scene
    void Spawn(Transform prefab, int num)
    {
        for (int i = 0; i < num; i++)
        {
            var onbject = Instantiate(prefab,
                new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(spawnRadius, spawnRadius), 0), Quaternion.identity);
        }
    }
    //implement getNeighbors
    public List<Agent>getNeighbor(Agent agent, float radius)
    {
        List<Agent> neighborList = new List<Agent>();
        foreach (var otherAgent in agents)
        {
            if (otherAgent == agent)// no one is a neighbor of itself
                continue;
            //compute the distance and check if it less or equal to the radius, and if it is, neighbor added
            if(Vector3.Distance(agent.pos, otherAgent.pos) <= radius)
            {
                neighborList.Add(otherAgent);
            }
        }

        //return other neighbor agents
        return neighborList;
    }


    //implementing Get Hunters
    public List<Hunter> getHunters(Agent agent, float radius)
    {
        List<Hunter> neighborList = new List<Hunter>();
        foreach (var hunter in hunters)
        {
            
            //compute the distance and check if it less or equal to the radius, and if it is, neighbor added
            if (Vector3.Distance(agent.pos, hunter.pos) <= radius)
            {
                neighborList.Add(hunter);
            }
        }

        //return other neighbor agents
        return neighborList;
    }
}
