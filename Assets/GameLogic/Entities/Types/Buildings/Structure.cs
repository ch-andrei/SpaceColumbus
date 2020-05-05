using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

using Entities.Bodies.Health;
using Entities.Bodies.Damages;

namespace Entities
{
    [RequireComponent(typeof(NavMeshObstacle), typeof(BoxCollider))]
    public class Structure : Entity
    {
        public override string Name { get { return "Structure"; } }
        
        // TODO
        public override bool IsDamageable { get { return false; } }
        // TODO
        public override bool IsDamaged { get { return false; } }

        private void Awake()
        {
            this.entityType = EntityType.Structure;
        }

        public override void Start()
        {
            base.Start();

            //gameObject.GetComponent<NavMeshObstacle>().size = gameObject.GetComponent<BoxCollider>().size;
        }

        void Update()
        {

        }

        public override EDamageState GetDamageState()
        {
            return EDamageState.None;
        }

        public override void TakeDamage(Damage damage)
        {
            return;
        }
    }
}
