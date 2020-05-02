using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PrefabManager : MonoBehaviour
{
    [FormerlySerializedAs("AgentPrefab")] public GameObject agentPrefab;
    [FormerlySerializedAs("SelectionPrefab")] public GameObject selectionPrefab;
}
