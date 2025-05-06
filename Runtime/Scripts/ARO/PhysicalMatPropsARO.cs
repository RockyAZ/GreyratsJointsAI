using Greyrats.JointAI.JointAgent;
using System.Linq;
using Unity.MLAgents.Actuators;
using UnityEngine;

namespace Greyrats.JointAI.ARO
{
	public class PhysicalMatPropsARO : ActionRewardObservation
	{
		public PhysicsMaterial[] materials;
		public bool useDynamicFriction = true;
		public bool useStaticFriction = true;
		public bool useBouncines = true;

		public override void Initialize(AgentControllerBase a)
		{
			base.Initialize(a);
			//colliders = GetComponentsInChildren<Collider>();
			materials = a.AgentMainBone.GetComponentsInChildren<PhysicalMatPropsComponent>().Select(t => t.GetComponent<Collider>().material).ToArray();
		}

		public override void Dispose() { }

		public override int ActionsCount
		{
			get
			{
				int result = 0;
				if (useDynamicFriction)
					result += materials.Length;
				if (useDynamicFriction)
					result += materials.Length;
				if (useDynamicFriction)
					result += materials.Length;
				return result;
			}
		}

		public override void EpisodeAction(in ActionBuffers actions, ref int index)
		{
			var continuousActions = actions.ContinuousActions;
			for (int i = 0; i < materials.Length; i++)
			{
				if (useDynamicFriction)
				{
					materials[i].dynamicFriction = (1 + continuousActions[index++]) * 0.5f;
				}
				if (useStaticFriction)
				{
					materials[i].staticFriction = (1 + continuousActions[index++]) * 0.5f;
				}
				if (useBouncines)
				{
					materials[i].bounciness = (1 + continuousActions[index++]) * 0.5f;
				}
			}
		}
	}
}