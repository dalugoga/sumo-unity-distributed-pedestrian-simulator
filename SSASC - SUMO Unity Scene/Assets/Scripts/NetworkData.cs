using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkData : MonoBehaviour
{
    [Tooltip("Edge Id")]
    public string id;

    [Tooltip("Is this edge for pedestrians?")]
    public bool pedWalk;
}
