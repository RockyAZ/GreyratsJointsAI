using Greyrats.JointAI.ARO;
using Greyrats.JointAI.JointAgent;
using UnityEngine;

namespace Greyrats.JointAI
{
	public class MainAgentHolder : MonoBehaviour
	{
		[SerializeField] private AgentControllerBase agent;
		[SerializeField] private JointsControllerARO jointsController;
		[SerializeField] private Transform agentRelativeParentTransform;

		public AgentControllerBase Agent => agent;
		public JointsControllerARO JointsController => jointsController;
		public Transform AgentRelativeParentTransform => agentRelativeParentTransform;
	}
}