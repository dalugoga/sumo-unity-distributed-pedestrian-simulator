using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PedestrianController : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 endPos;
    public List<Vector3> path;
    private static float speed = 0.005f;

    private static Vector3 sidewalkUp = new Vector3(100, 0, 54);
    private static Vector3 sidewalkDown = new Vector3(100, 0, 46);

    public float speedx = 0;
    public float speedz = 0;

    private ForcesVariables vars;

    private int stuck_counter = 0;
    private bool deadlock = false;
    private int dumb_move_timer = 0;


    private class Wall
    {
        public GameObject wall;
        public Vector3 point;
        public float distance;

        public Wall(GameObject wall, Vector3 point, float distance)
        {
            this.wall = wall;
            this.point = point;
            this.distance = distance;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        vars = transform.parent.gameObject.GetComponent<ForcesVariables>();

        startPos = transform.position;
        if (gameObject.name == "Person1")
            endPos = GameObject.Find("Person2").transform.position;
        if (gameObject.name == "Person2")
            endPos = GameObject.Find("Person1").transform.position;
        if (gameObject.name == "Person3")
            endPos = GameObject.Find("Person4").transform.position;
        if (gameObject.name == "Person4")
            endPos = GameObject.Find("Person3").transform.position;
        if (gameObject.name == "Person5")
            endPos = GameObject.Find("Person6").transform.position;
        if (gameObject.name == "Person6")
            endPos = GameObject.Find("Person5").transform.position;

        path = new List<Vector3>();

        if(Mathf.Abs(startPos.z - endPos.z) < 2)
            path.Add(endPos);
        else
        {
            if (Mathf.Abs(startPos.z - 54.2f) < 2)
            {
                path.Add(sidewalkUp);
                path.Add(sidewalkDown);
            }
            else
            {
                path.Add(sidewalkDown);
                path.Add(sidewalkUp);
            }

            path.Add(endPos);
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (path.Count == 0)
        {
            GameObject.Destroy(transform.gameObject);
            return;
        }

        if (Vector3.SqrMagnitude(transform.position - path[0]) > 0.1)
        {
            if (deadlock) 
            {
                DumbMove();
                dumb_move_timer--;
                if (dumb_move_timer == 0)
                    deadlock = false;
            }
            else
                ForcesMove();

        }
        else
            if (path.Count != 0)
            path.RemoveAt(0);
        
    }

    public void DumbMove()
    {
        Vector3 pos = transform.position;
        pos = Vector3.MoveTowards(pos, path[0], speed);
        transform.position = pos;
        transform.LookAt(path[0]);
    }

    private Wall WallRepulsionPoint()
    {
        Vector3 closest_wall = Vector3.zero;
        float closest_wall_distance = float.MaxValue;
        GameObject wall = null;
        foreach(GameObject w in vars.avoid)
        {
            Vector3 point = w.GetComponent<Collider>().ClosestPoint(transform.position);
            float distance = Vector3.Distance(point, transform.position);
            if(distance < closest_wall_distance)
            {
                closest_wall = point;
                closest_wall_distance = distance;
                wall = w;
            }
        }

        if (wall != null)
            Debug.Log("HAS AVOIDS");

        return new Wall(wall, closest_wall, closest_wall_distance);
    }

    //Based on the work of Peng Wang @ https://github.com/godisreal/SocialForceModel-1
    public void ForcesMove() {
        Vector3 pos = transform.position;
        float repx = 0;
        float repz = 0;
        float heading = Mathf.Atan2(path[0].z - pos.z, path[0].x - pos.x);// * Mathf.Rad2Deg;
        float h = Mathf.Atan2(speedx, speedz);// * Mathf.Rad2Deg;

        //foreach (Transform child in GameObject.Find("Persons").transform)
        foreach (Transform child in gameObject.transform.parent) // GameObject.Find("TestPeds").transform)
        {
            if(Vector3.SqrMagnitude(child.position - pos) < 4 * vars.D * vars.D && child.gameObject.name != gameObject.name)
            {
                float angle = Mathf.Atan2(child.position.z - pos.z, child.position.x - pos.x);
                float repxi = vars.A * Mathf.Exp((1 - Vector3.Distance(child.position, pos)) / vars.D) * Mathf.Sin(angle) * (1 - Mathf.Cos(angle - h));
                float repzi = vars.A * Mathf.Exp((1 - Vector3.Distance(child.position, pos)) / vars.D) * Mathf.Cos(angle) * (1 - Mathf.Cos(angle - h));
                repx += repxi;
                repz += repzi;
                //Debug.DrawLine(pos, new Vector3(pos.x + repxi, pos.y, pos.z + repzi), Color.blue);
            }
        }

        
        speedx = speedx + vars.dt * (repx + (vars.v0 * Mathf.Cos(heading) - speedx) / vars.tr);
        speedz = speedz + vars.dt * (repz + (vars.v0 * Mathf.Sin(heading) - speedz) / vars.tr);

        if (Mathf.Abs(speedx) + Mathf.Abs(speedz) < 0.1)
            stuck_counter++;

        Wall closest_wall = WallRepulsionPoint();
        if (closest_wall.distance < vars.min_distance_to_wall)
        {
            //Debug.DrawLine(pos, new Vector3(closest_wall.point.x, pos.y, closest_wall.point.z), Color.magenta);
            float angle = Mathf.Atan2(closest_wall.point.z - pos.z, closest_wall.point.x - pos.x);

            if (Vector3.Dot(new Vector3(closest_wall.point.x - pos.x, 0, closest_wall.point.z - pos.z), new Vector3(speedx, 0, speedz)) > 0)
            {
                //speedx =- speedx * 0.1f * Mathf.Sin(angle);
                speedz =- speedz * 0.1f * Mathf.Cos(angle);
            }
        }

        /*
        if(stuck_counter > 240)
        {
            stuck_counter = 0;
            deadlock = true;
            dumb_move_timer = 100;
            //speedx = vars.v0 / 2 * Mathf.Cos(heading);
            //speedz = vars.v0 / 2 * Mathf.Sin(heading);
        }*/

        transform.position = transform.position + new Vector3(speedx * vars.dt, 0, speedz * vars.dt);
        Debug.DrawLine(pos, new Vector3(pos.x + speedx, pos.y, pos.z), Color.red);
        Debug.DrawLine(pos, new Vector3(pos.x, pos.y, pos.z + speedz), Color.blue);
        Debug.DrawLine(pos, new Vector3(pos.x + speedx, pos.y, pos.z + speedz), Color.green);
        //Debug.DrawLine(pos, new Vector3(pos.x + vars.v0 * Mathf.Cos(heading), pos.y, pos.z + vars.v0 * Mathf.Sin(heading)), Color.red);


        transform.LookAt(path[0]);
        transform.forward = new Vector3(speedx, 0, speedz).normalized;
    }

}
