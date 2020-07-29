using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{
    public GameObject[] spawnSurfaces;
    public int startPedestrians;
    public int pedestriansPerMinute;

    private float nextActionTime = 0.0f;
    private float period;
    private int pedestrianCount = 0;

    private GameObject person_prefab;
    private GameObject persons;

    private ForcesVariables vars;


    // Start is called before the first frame update
    void Start()
    {
        person_prefab = Resources.Load("Person_v4") as GameObject;
        persons = persons = GameObject.Find("Persons");

        vars = persons.gameObject.GetComponent<ForcesVariables>();

        period = 60 / pedestriansPerMinute;

        for (int i = 0; i < startPedestrians; i++)
            SpawnNewPedestrian();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            SpawnNewPedestrian();
        }
    }

    private void SpawnNewPedestrian()
    {

        Vector3 spawnPoint = getSpawnEndPoint();

        GameObject pedestrian = GameObject.Instantiate(person_prefab, spawnPoint, Quaternion.identity, persons.transform);
        pedestrian.name = "ped_" + pedestrianCount;
        PedestrianController pc = pedestrian.AddComponent<PedestrianController>();
        pc.startPos = spawnPoint;
        pc.endPos = getSpawnEndPoint();

        pedestrianCount++;
    }

    public Vector3 getSpawnEndPoint()
    {
        int surfaceIndex = Random.Range(0, spawnSurfaces.Length);
        GameObject chosenOne = spawnSurfaces[surfaceIndex];
        return SpawnPoint(chosenOne.GetComponent<Collider>().bounds);
    }

    public Vector3 SpawnPoint(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x + vars.min_distance_to_wall, bounds.max.x - vars.min_distance_to_wall),
            0.09f,
            Random.Range(bounds.min.z + vars.min_distance_to_wall, bounds.max.z - vars.min_distance_to_wall)
        );
    }
}
