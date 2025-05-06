using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public static class RigidbodyExtensions
	{
		public static Quaternion LocalRotation(this Rigidbody rb, Transform mainBone)
		{
			if (mainBone != null)
			{
				return Quaternion.Inverse(mainBone.rotation) * rb.transform.rotation;
			}
			else
			{
				Debug.LogWarning("Main bone is not assigned.");
				return Quaternion.identity;
			}
		}
	}
}