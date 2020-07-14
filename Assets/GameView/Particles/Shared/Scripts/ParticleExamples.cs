using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class ParticleExamples {

	public string title;
	[TextArea]
	public string description;
	public bool isWeaponEffect;
	[FormerlySerializedAs("particleSystemGO")] public GameObject particleSystemGo;
	public Vector3 particlePosition, particleRotation;
}
