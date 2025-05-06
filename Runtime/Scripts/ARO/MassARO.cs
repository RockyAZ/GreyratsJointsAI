using Greyrats.JointAI.JointAgent;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public class MassARO : ActionRewardObservation
	{
		public float MinMass = 0.5f;
		public float MaxMass = 5f;
		public bool Observe;
		private Rigidbody[] rbs;
		public override int ActionsCount { get => rbs.Length; }
		public override int ObservationsCount { get => Observe ? rbs.Length : 0; }

		public override void Initialize(AgentControllerBase a)
		{
			base.Initialize(a);
			rbs = a.AgentMainBone.GetComponentsInChildren<Rigidbody>();
		}

		public override void Dispose()
		{

		}

		public override void EpisodeAction(in ActionBuffers actions, ref int index)
		{
			var continuousActions = actions.ContinuousActions;
			for (int i = 0; i < rbs.Length; i++)
			{
				float action = continuousActions[index++];
				action = (action + 1f) * 0.5f;
				rbs[i].mass = Mathf.Lerp(MinMass, MaxMass, action);
			}
		}

		public override void EpisodeObservation(VectorSensor sensor)
		{
			for (int i = 0; i < rbs.Length; i++)
			{
				sensor.AddObservation(rbs[i].mass);
			}
		}
	}
}