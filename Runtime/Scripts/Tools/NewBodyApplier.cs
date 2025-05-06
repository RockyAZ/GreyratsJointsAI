using Greyrats.JointAI.JointAgent;
using UnityEngine;

namespace Greyrats.JointAI.Tools
{
	public class NewBodyApplier : MonoBehaviour
	{
		public AgentControllerBase agentController;

		public void SetNewBody(GameObject newBody)
		{
			var oldToDestroy = agentController.AgentMainBone.gameObject;
			var saveParent = agentController.AgentMainBone.parent;
			newBody.transform.parent = saveParent;
			agentController.ChangeAgentBody(newBody.transform);
			Destroy(oldToDestroy);
		}
	}
}