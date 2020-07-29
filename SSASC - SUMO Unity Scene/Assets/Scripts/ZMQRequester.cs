using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Collections;

public class ZMQRequester : RunableThread {

    private Thing subject = new Thing("subject", 0, 0, 0, "", "", false);
    private GameObject person_prefab = Resources.Load("Person_v4") as GameObject;
    private GameObject car_prefab = Resources.Load("Car_v2") as GameObject;
    private GameObject persons = GameObject.Find("Persons");
    private GameObject cars = GameObject.Find("Cars");

    private List<Thing> personsList = new List<Thing>();
    private PersonsData personsData = new PersonsData();

    [Serializable]
    public  class Thing
    {
        public string name;
        public double x;
        public double y;
        public double angle;
        public string edge;
        public string lane;
        public bool pedWalk;
        public bool changedArea;

        public Thing(string name, double x, double y, double angle, string edge, string lane, bool pedWalk, bool changedArea = false)
        {
            this.name = name;
            this.x = x;
            this.y = y;
            this.angle = angle;
            this.edge = edge;
            this.lane = lane;
            this.pedWalk = pedWalk;
            this.changedArea = changedArea;
        }
    }

    [Serializable]
    public class Data
    {
        public Thing[] persons;
        public Thing[] vehicles;
    }

    [Serializable]
    public class PersonsData
    {
        public Thing[] persons;
    }

    public void UpdateSubject(double x, double y, double angle, string edge, string lane, bool pedWalk)
    {
        subject.x = x;
        subject.y = y;
        subject.angle = angle;
        subject.edge = edge;
        subject.lane = lane;
        subject.pedWalk = pedWalk;
    }

    public void UpdatePersonsList(List<Thing> things)
    {
        personsList = things;
        personsData.persons = things.ToArray();
    }

    private List<string> getNamesOfThings()
    {
        List<string> ret = new List<string>();

        //foreach (Transform child in persons.transform)
            //ret.Add(child.name);

        foreach (Transform child in cars.transform)
            ret.Add(child.name);

        return ret;
    }

    private IEnumerator HandleMessage(string message)
    {
        Data data = JsonUtility.FromJson<Data>(message);
        Debug.Log(data);

        List<string> things = getNamesOfThings();

        /*
        foreach (Thing p in data.persons)
        {
            GameObject go = GameObject.Find(p.name);
            SubjectController sc;

            if (go == null)
            {
                go = GameObject.Instantiate(person_prefab, new Vector3((float)p.x, 0.09f, (float)p.y), Quaternion.identity, persons.transform);
                go.name = p.name;

                if(p.name == "subject")
                {
                    //go.AddComponent(typeof(SubjectController));
                }
            }
            else if (p.name != "subject")
            {
                go.transform.position = new Vector3((float)p.x, 0.09f, (float)p.y);
                go.transform.rotation = Quaternion.Euler(0, (float)p.angle, 0);
            }

            things.Remove(p.name);
        }
        */
        

        foreach (Thing p in data.vehicles)
        {
            GameObject go = GameObject.Find(p.name);

            if (go == null)
            {
                go = GameObject.Instantiate(car_prefab, new Vector3(0, 0, 0), Quaternion.identity, cars.transform);
                go.name = p.name;
            }

            go.transform.position = new Vector3((float)p.x, 0f, (float)p.y);
            go.transform.rotation = Quaternion.Euler(0, (float)p.angle, 0);

            things.Remove(p.name);
        }

        foreach (string name in things)
        {
            GameObject.Destroy(GameObject.Find(name));
        }

        yield return null;
    }

    protected override void Run()
    {
        ForceDotNet.Force();
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");

            while (running)
            {
                //Debug.Log("Sending Hello");
                //client.SendFrame("subject: " + subject.x + "; " + subject.y);
                //Debug.Log("Sent " + JsonUtility.ToJson(personsData));
                client.SendFrame(JsonUtility.ToJson(personsData));
                

                string message = null;
                bool gotMessage = false;
                while (running)
                {
                    gotMessage = client.TryReceiveFrameString(out message);
                    if (gotMessage) break;
                }

                if (gotMessage)
                {
                    Debug.Log("Received " + message);
                    UnityMainThreadDispatcher.Instance().Enqueue(HandleMessage(message));
                }
            }

        }

        NetMQConfig.Cleanup();
    }

    protected /*override*/ void Run2()
    {
        ForceDotNet.Force(); 
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");

            while (running)
            {
                //Debug.Log("Sending Hello");
                //client.SendFrame("subject: " + subject.x + "; " + subject.y);
                client.SendFrame(JsonUtility.ToJson(subject));
                Debug.Log("Sent " + JsonUtility.ToJson(subject));

                string message = null;
                bool gotMessage = false;
                while (running)
                {
                    gotMessage = client.TryReceiveFrameString(out message);
                    if (gotMessage) break;
                }

                if (gotMessage)
                {
                    Debug.Log("Received " + message);
                    UnityMainThreadDispatcher.Instance().Enqueue(HandleMessage(message));
                }
            }

        }

        NetMQConfig.Cleanup();
    }
}
