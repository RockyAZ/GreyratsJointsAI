using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Greyrats.JointAI
{
	public class TransformFollower : MonoBehaviour
	{
		public Transform TransformToFollow;
		public Vector3 FollowOffset;
		public Vector3 RotationOffset;
		public bool followPosition = true;
		public bool followRotation = true;

		void OnValidate()
		{
			FixedUpdate();
		}

		protected virtual void FixedUpdate()
		{
			if (followPosition)
				transform.position = TransformToFollow.position + FollowOffset;
			if (followRotation)
				transform.rotation = TransformToFollow.rotation * Quaternion.Euler(RotationOffset);
		}
	}
}