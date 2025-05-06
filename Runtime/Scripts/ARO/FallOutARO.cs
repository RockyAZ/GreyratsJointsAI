using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public class FallOutARO : ActionRewardObservation
	{
		public float fallDownRaycastDistance = 1000f;
		public string LayerToIgnore = "Agent";
		public float penalty = -100;
		public bool debug;
		public override void EpisodeReward()
		{
			RaycastHit hit;
			if (!Physics.Raycast(agent.AgentMainBone.position, Vector3.down, out hit, fallDownRaycastDistance, ~LayerMask.GetMask(LayerToIgnore)))
			{
				agent.ForceEndEpisode(-Mathf.Abs(penalty));
				if (debug)
					Debug.DrawRay(agent.AgentMainBone.position, Vector3.down * fallDownRaycastDistance, Color.red, 3f);
			}
		}

		public override void Dispose() { }
	}
}