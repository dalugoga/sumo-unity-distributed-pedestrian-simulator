using UnityEngine;
using System.Collections.Generic;

public class ZMQClient : MonoBehaviour {

    private ZMQRequester zmqRequester;
    GameObject subject;
    private readonly int rayCastLayerMask = 1 << 9;
    List<ZMQRequester.Thing> previous_things;

    void Start () {
        zmqRequester = new ZMQRequester();
        zmqRequester.Start();
	}
	
    void OnDestroy()
    {
        zmqRequester.Stop();
    }

    private void Update()
    {
        Transform persons = GameObject.Find("Persons").transform;

        List<ZMQRequester.Thing> things = new List<ZMQRequester.Thing>();
        foreach(Transform t in persons)
        {
            RaycastHit hit;
            string hit_id = "";
            string hit_lane = "";
            bool hit_pedWalk = false;
            bool changedArea = false;

            if (Physics.Raycast(t.transform.position, t.transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, rayCastLayerMask))
            {
                GameObject hit_go = hit.collider.gameObject;
                hit_id = hit_go.GetComponent<NetworkData>().id;
                hit_lane = hit_go.name;
                hit_pedWalk = hit_go.GetComponent<NetworkData>().pedWalk;
            }

            if (previous_things != null)
                foreach (ZMQRequester.Thing old in previous_things)
                    if (old.name == t.gameObject.name)
                        if (old.edge != hit_id)
                            changedArea = true;

            ZMQRequester.Thing th = new ZMQRequester.Thing(t.gameObject.name, t.position.x, t.position.z, t.rotation.eulerAngles.y, hit_id, hit_lane, hit_pedWalk, changedArea);
            things.Add(th);
        }

        zmqRequester.UpdatePersonsList(things);
    }

    private void Update2()
    {
        subject = GameObject.Find("subject");

        if (subject == null)
            return;

        RaycastHit hit;
        string hit_id = "";
        string hit_lane = "";
        bool hit_pedWalk = false;

        if (Physics.Raycast(subject.transform.position, subject.transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, rayCastLayerMask))
        {
            GameObject hit_go = hit.collider.gameObject;
            hit_id = hit_go.GetComponent<NetworkData>().id;
            hit_lane = hit_go.name;
            hit_pedWalk = hit_go.GetComponent<NetworkData>().pedWalk;
        }
        
        zmqRequester.UpdateSubject(subject.transform.position.x, subject.transform.position.z, subject.transform.rotation.eulerAngles.y, hit_id, hit_lane, hit_pedWalk);
    }
}
