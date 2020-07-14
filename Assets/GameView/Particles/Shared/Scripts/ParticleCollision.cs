using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script demonstrate how to use the particle system collision callback.
/// The sample using it is the "Extinguish" prefab. It use a second, non displayed
/// particle system to lighten the load of collision detection.
/// </summary>
public class ParticleCollision : MonoBehaviour
{
    private List<ParticleCollisionEvent> _mCollisionEvents = new List<ParticleCollisionEvent>();
    private ParticleSystem _mParticleSystem;


    private void Start()
    {
        _mParticleSystem = GetComponent<ParticleSystem>();
    }


    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = _mParticleSystem.GetCollisionEvents(other, _mCollisionEvents);
        for (int i = 0; i < numCollisionEvents; ++i)
        {
            var col = _mCollisionEvents[i].colliderComponent;

            var fire = col.GetComponent<ExtinguishableFire>();
            if (fire != null)
                fire.Extinguish();
        }
    }
}
