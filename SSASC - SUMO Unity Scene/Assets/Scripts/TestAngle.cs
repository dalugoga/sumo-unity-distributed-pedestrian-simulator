using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAngle : MonoBehaviour
{
    public GameObject sphere1;
    public GameObject sphere2;
    public float angle;
    public float angleDegrees;
    Vector3 s1;
    Vector3 s2;

    // Start is called before the first frame update
    void Start()
    {
        s1 = sphere1.transform.position;
        s2 = sphere2.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        s1 = sphere1.transform.position;
        s2 = sphere2.transform.position;

        angle = Mathf.Atan2(s2.z - s1.z, s2.x - s1.x) * 180 / Mathf.PI;
        angleDegrees = angle * Mathf.Rad2Deg;
    }
}
