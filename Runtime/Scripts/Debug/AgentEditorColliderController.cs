using UnityEngine;
using EasyButtons;
using System.Linq;
namespace Greyrats.JointAI.Analysis
{
    public class AgentEditorColliderController : MonoBehaviour
    {
        public GameObject[] colliders;
        public Rigidbody[] rigidbodies;

        public float[] rigidbodiesMass;

        [Button]
        public void HideColliderRenderer()
        {
            foreach (var collider in colliders)
            {
                collider.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        [Button]
        public void ShowColliderRenderer()
        {
            foreach (var collider in colliders)
            {
                collider.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        [Button]
        public void FindAllChildRigidbodies()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
        }

        [Button]
        public void FindAllChildColliders()
        {
            colliders = GetComponentsInChildren<Collider>().Select(c => c.gameObject).ToArray();
        }

        [Button]
        public void SetRigidbodiesMass(float mass)
        {
            rigidbodiesMass = new float[rigidbodies.Length];
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodiesMass[i] = rigidbodies[i].mass;
            }

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodies[i].mass = mass;
            }
        }
        public void RevertRigidbodiesMass()
        {
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodies[i].mass = rigidbodiesMass[i];
            }
        }
    }
}