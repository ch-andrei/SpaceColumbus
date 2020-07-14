using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.AI
{
    [ExecuteInEditMode]
    [AddComponentMenu("Navigation/NavMeshModifier", 32)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public class NavMeshModifier : MonoBehaviour
    {
        [FormerlySerializedAs("m_OverrideArea")] [SerializeField]
        bool mOverrideArea;
        public bool overrideArea { get { return mOverrideArea; } set { mOverrideArea = value; } }

        [FormerlySerializedAs("m_Area")] [SerializeField]
        int mArea;
        public int area { get { return mArea; } set { mArea = value; } }

        [FormerlySerializedAs("m_IgnoreFromBuild")] [SerializeField]
        bool mIgnoreFromBuild;
        public bool ignoreFromBuild { get { return mIgnoreFromBuild; } set { mIgnoreFromBuild = value; } }

        // List of agent types the modifier is applied for.
        // Special values: empty == None, m_AffectedAgents[0] =-1 == All.
        [FormerlySerializedAs("m_AffectedAgents")] [SerializeField]
        List<int> mAffectedAgents = new List<int>(new int[] { -1 });    // Default value is All

        static readonly List<NavMeshModifier> SNavMeshModifiers = new List<NavMeshModifier>();

        public static List<NavMeshModifier> activeModifiers
        {
            get { return SNavMeshModifiers; }
        }

        void OnEnable()
        {
            if (!SNavMeshModifiers.Contains(this))
                SNavMeshModifiers.Add(this);
        }

        void OnDisable()
        {
            SNavMeshModifiers.Remove(this);
        }

        public bool AffectsAgentType(int agentTypeId)
        {
            if (mAffectedAgents.Count == 0)
                return false;
            if (mAffectedAgents[0] == -1)
                return true;
            return mAffectedAgents.IndexOf(agentTypeId) != -1;
        }
    }
}
