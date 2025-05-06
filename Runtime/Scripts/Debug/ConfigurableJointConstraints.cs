using UnityEngine;

namespace Greyrats.JointAI.Analysis
{
    public class ConfigurableJointConstraints : MonoBehaviour
    {
        [System.Serializable]
        public class MinMax
        {
            [Range(0, 1)]
            public float min = 0;
            [Range(0, 1)]
            public float max = 1;
        }

        public MinMax rotationX;
        public MinMax rotationY;
        public MinMax rotationZ;
    }
}