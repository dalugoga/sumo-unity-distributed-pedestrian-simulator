using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class SubjectController : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.Find("lpMaleG").gameObject.layer = 8;
        GameObject.Find("RightHand").SetActive(false);
        GameObject.Find("LeftHand").SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject actual_headset = VRTK_SDKManager.GetLoadedSDKSetup().actualHeadset;
        

        gameObject.transform.position = new Vector3(actual_headset.transform.position.x, gameObject.transform.position.y, actual_headset.transform.position.z);
        gameObject.transform.rotation = Quaternion.Euler(0, actual_headset.transform.rotation.eulerAngles.y, 0);

        
    }    
}
