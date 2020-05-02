using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Navigation
{
    public class NavMeshGenerator : MonoBehaviour
    {
        private GameObject _navMeshRoot = null;

        private List<GameObject> _navMeshElements = new List<GameObject>(); // TODO: generate only using nav mesh elements

        private bool _navMeshInitialized = false;

        private void Awake()
        {
            if (_navMeshRoot == null)
            {
                _navMeshRoot = this.gameObject;
            }
        }

        public void BuildNavMesh()
        {
            Awake();

            // remove existing navMeshSurfaces
            foreach (NavMeshSurface navMeshSurface in _navMeshRoot.GetComponents<NavMeshSurface>())
                Destroy(navMeshSurface);

            int agentTypeCount = UnityEngine.AI.NavMesh.GetSettingsCount();
            if (agentTypeCount < 1) { return; }
            for (int i = 0; i < agentTypeCount; ++i)
            {
                NavMeshBuildSettings settings = UnityEngine.AI.NavMesh.GetSettingsByIndex(i);
                NavMeshSurface navMeshSurface = _navMeshRoot.AddComponent<NavMeshSurface>();
                navMeshSurface.AgentTypeId = settings.agentTypeID;

                NavMeshBuildSettings actualSettings = navMeshSurface.GetBuildSettings();

                navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes; // or you can use RenderMeshes
                //navMeshSurface.layerMask = true;

                // remove existing agents from the navmesh layermask
                navMeshSurface.layerMask -= LayerMask.GetMask("Agents");
                navMeshSurface.layerMask -= LayerMask.GetMask("Ignore Raycast");

                navMeshSurface.buildHeightMesh = true;

                navMeshSurface.BuildNavMesh();
            }

            this._navMeshInitialized = true;
        }
    }
}
