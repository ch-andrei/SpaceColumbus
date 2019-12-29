﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSettings : MonoBehaviour
{
    public Material MainRegionMaterial;

    public void Start()
    {
        if (MainRegionMaterial == null) {
            Debug.Log("Warning: Not all materials were defined at runtime.");
        }
    }
}
