using EasyButtons;
using Greyrats.JointAI.ARO;
using UnityEngine;

namespace Greyrats.JointAI.Analysis
{
	public class DebugChildJoints : MonoBehaviour
	{
		public JointsControllerARO jc;
		public Transform mainBone;
		[Space]
		[Range(-1, 1)] public float X;
		[Range(-1, 1)] public float Y;
		[Range(-1, 1)] public float Z;
		[Range(-1, 1)] public float force;

		void Start()
		{
			jc.Initialize(mainBone, transform);
		}

		[Button]
		public void Reinitiaize()
		{
			Start();
		}

		private void OnDisable()
		{
			jc.ConsiderActions = true;
		}

		void LateUpdate()
		{
			jc.ConsiderActions = false;
			jc.DebugMakeAllJointsAction(X, Y, Z, force);
		}
	}
}