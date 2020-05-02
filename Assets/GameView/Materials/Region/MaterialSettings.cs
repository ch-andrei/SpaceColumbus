using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MaterialSettings : MonoBehaviour
{
    [FormerlySerializedAs("MainRegionMaterial")] public Material mainRegionMaterial;
    [FormerlySerializedAs("AgentMaterial")] public Material agentMaterial;
    [FormerlySerializedAs("SelectionMaterial")] public Material selectionMaterial;

    public void Start()
    {
        if (mainRegionMaterial == null) {
            Debug.Log("Warning: Not all materials were defined at runtime.");
        }
    }
}
