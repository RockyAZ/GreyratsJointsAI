using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public class GroundDistanceComponent : MonoBehaviour
	{
		public bool DebugDrawLineGround;
		public bool DebugLogErrorNoGround;
		public float GetDistance(string layerToIntersect)
		{
			if (Physics.Raycast(transform.position, Vector3.down, out var hit, 100000f, LayerMask.GetMask(layerToIntersect)))
			{
				if (DebugDrawLineGround)
				{
					Debug.DrawLine(transform.position, hit.point, Color.green, 5);
				}
				return hit.distance;
			}
			else
			{
#if UNITY_EDITOR
				if(DebugLogErrorNoGround)
					Debug.LogError($"no hit with ground in component[{gameObject.name}]|layerToIntersect:{layerToIntersect}");
				Debug.DrawLine(transform.position, transform.position + Vector3.down * 10000f, Color.red, 5);
#endif
				return 0.1f;
			}
		}
	}
}