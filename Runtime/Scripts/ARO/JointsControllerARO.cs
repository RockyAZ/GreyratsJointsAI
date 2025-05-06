using Greyrats.JointAI.JointAgent;
using System.Collections;
using System.Linq;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System;
using Greyrats.JointAI.Analysis;

namespace Greyrats.JointAI.ARO
{
	public class JointsControllerARO : ActionRewardObservation
	{
		public Transform parentLocalTransformProvider;
		public bool ResetAgentOnBeginEpisode = true;
		[Header("Physics")]
		public bool Discrete = false;
		public int DiscreteSize = 3;
		[Space]
		protected Joint[] joints;
		protected Rigidbody[] rigidBodies;
		public bool AlwaysChangeValues = true;
		public float MaxForceLimit = 3.402823e+38f;
		public float MaxSpring = 10;
		public float Dampen = 1;
		public bool UseAcceleration;
		public float massScaler = 1;
		public float connectedMassScaler = 0;
		public float anguarLimitSpring = 0;
		public float angularLimitDampen = 0;
		[Range(0, 1)]
		public float bouncines = 0;
		public float contactDistance = 0;
		public bool ConfiguredInWorldSpace;
		public bool EnableCollision;
		public bool EnablePreprocessing;
		public JointProjectionMode JointProjectionMode;
		public float projectionDistance;
		public float projectionAngle;
		public float freeJointLimitValue = 180;

		[Space]
		public float maxAngularVelocity = 1000000000000f;
		public float maxlinearVelocity = 1000000000000f;

		[Space] public bool useForce;
		public bool observePosition = true;
		public bool observeRotation = true;
		public bool observeVelocity = false;
		[Space]
		public bool useSpringDamperAction;
		public float MinSpringValue = 0;
		public float MaxSpringValue = 0;
		public float MinDamperValue = 0;
		public float MaxDamperValue = 0;

		private bool _considerActions;
		private Transform _mainBone;
		private Vector3 _mainBoneStartPos;
		private Quaternion _mainBoneStartRot;
		private Collider _mainBoneCollider;

		private Coroutine smoothResetCoroutine;

		public override void Initialize(AgentControllerBase a)
		{
			base.Initialize(a);
			this.agent.ON_FORCE_END += OnForceEndEpisode;
			ConsiderActions = true;

			var mainAgentHolder = FindObjectOfType<MainAgentHolder>();
			if (mainAgentHolder == null)
			{
				Debug.LogWarning(
					"mainAgentHolder == null. Relative parent for resetting transformation is set to agent object, which is bad if scene is rotating");
				Initialize(agent.AgentMainBone, agent.transform);
			}
			else
			{
				Initialize(agent.AgentMainBone, mainAgentHolder.AgentRelativeParentTransform);
			}
		}

		public override void Dispose()
		{
			ResetJoints();
			this.agent.ON_FORCE_END -= OnForceEndEpisode;
		}

		void OnForceEndEpisode()
		{
			ResetJoints();
		}

		public void Initialize(Transform mainBone, Transform relativeParentTransform)
		{
			if (parentLocalTransformProvider == null)
				parentLocalTransformProvider = relativeParentTransform;
			_mainBone = mainBone;
			_mainBoneCollider = _mainBone.GetComponent<Collider>();
			if (_mainBoneCollider == null)
			{
				for (int i = 0; i < _mainBone.childCount; i++)
				{
					var child = _mainBone.GetChild(i);
					if (child.TryGetComponent<Collider>(out var collider))
					{
						_mainBoneCollider = collider;
						break;
					}
				}
				if (_mainBoneCollider == null)
				{
					throw new Exception("Main bone doesn't have collider");
				}
			}

			CalculateApplyLocalTransform();

			var j = mainBone.GetComponentsInChildren<ConfigurableJoint>();
			rigidBodies = mainBone.GetComponentsInChildren<Rigidbody>();
			joints = new Joint[j.Length];
			for (int i = 0; i < joints.Length; i++)
			{
				joints[i] = new Joint(j[i], j[i].GetComponent<ConfigurableJointConstraints>(), this, mainBone);
				joints[i].Initialize();
				joints[i].SetForce(useForce ? 0 : 1, 1, 1);
				joints[i].SetAdditionalData();
			}
		}

		public override void EpisodeBegin()
		{
			if (!ResetAgentOnBeginEpisode)
				return;

			ResetJoints();
		}
		void ResetJoints()
		{
			_mainBone.position = GetStartPosition();
			_mainBone.rotation = GetStartRotation();

			for (int i = 0; i < joints.Length; i++)
			{
				joints[i].Reset();
			}
		}

		public void SmoothResetTransform(float duration)
		{
			if (smoothResetCoroutine != null)
				StopCoroutine(smoothResetCoroutine);
			smoothResetCoroutine = StartCoroutine(SmoothResetCoroutine(duration));
		}

		private IEnumerator SmoothResetCoroutine(float duration)
		{
			float tmpDuration = 0;
			Vector3[] initialLocalPositions = joints.Select(t => t.RB.transform.localPosition).ToArray();
			Quaternion[] initialLocalRotations = joints.Select(t => t.RB.transform.localRotation).ToArray();

			Vector3 mainBoneStartPos;
			Quaternion mainBoneStartRot;
			Vector3 mainBoneTargetPos;
			Quaternion mainBoneTargetRot;
			mainBoneStartPos = _mainBone.position;
			mainBoneStartRot = _mainBone.rotation;

			for (int i = 0; i < joints.Length; i++)
			{
				joints[i].StopRigidbody();
				joints[i].SetColliderActive(false);
			}

			_mainBoneCollider.enabled = false;

			while (tmpDuration < duration)
			{
				float lerp = tmpDuration / duration;

				for (int i = 0; i < joints.Length; i++)
				{
					joints[i].SetLocalTransformation(
						Vector3.Lerp(initialLocalPositions[i], joints[i].StartLocalPos, lerp),
						Quaternion.Lerp(initialLocalRotations[i], joints[i].StartLocalRot, lerp)
						);
				}

				mainBoneTargetPos = GetStartPosition();
				mainBoneTargetRot = GetStartRotation();

				_mainBone.position = Vector3.Lerp(mainBoneStartPos, mainBoneTargetPos, lerp);
				_mainBone.rotation = Quaternion.Lerp(mainBoneStartRot, mainBoneTargetRot, lerp);

				tmpDuration += Time.deltaTime;
				yield return 0;
			}

			for (int i = 0; i < joints.Length; i++)
			{
				joints[i].SetColliderActive(true);
				joints[i].StopRigidbody();
			}
			_mainBoneCollider.enabled = true;

			smoothResetCoroutine = null;
		}

		public override void EpisodeObservation(VectorSensor sensor)
		{
			if (observePosition)
				ObserveRotations(sensor);
			if (observeRotation)
				ObservePositions(sensor);
			if (useForce)
				ObserveForce(sensor);
			if (observeVelocity)
				ObserveVelocity(sensor);
		}

		public override void EpisodeAction(in ActionBuffers actions, ref int index)
		{
			if (!ConsiderActions)
				return;
			if (!Discrete)
				ContinuosAction(actions, ref index);
			else
				DiscreteAction(actions, ref index);
		}

		public void ContinuosAction(in ActionBuffers actions, ref int index)
		{
			var continuousActions = actions.ContinuousActions;

			for (int i = 0; i < joints.Length; i++)
			{
				Joint j = joints[i];
				MakeJointActions(j,
					j.EnabledRotationX ? continuousActions[index++] : 0,
					j.EnabledRotationY ? continuousActions[index++] : 0,
					j.EnabledRotationZ ? continuousActions[index++] : 0,
					useForce ? continuousActions[index++] : 1,
					useSpringDamperAction ? continuousActions[index++] : 1,
					useSpringDamperAction ? continuousActions[index++] : 1
				);
			}
		}

		public void DiscreteAction(in ActionBuffers actions, ref int index)
		{
			var discreteActions = actions.DiscreteActions;

			for (int i = 0; i < joints.Length; i++)
			{
				Joint j = joints[i];
				MakeJointActions(j,
					j.EnabledRotationX ? ConvertToRange(discreteActions[index++]) : 0,
					j.EnabledRotationY ? ConvertToRange(discreteActions[index++]) : 0,
					j.EnabledRotationZ ? ConvertToRange(discreteActions[index++]) : 0,
					useForce ? ConvertToRange(discreteActions[index++]) : 1,
					useSpringDamperAction ? ConvertToRange(discreteActions[index++]) : 1,
					useSpringDamperAction ? ConvertToRange(discreteActions[index++]) : 1
				);
			}
		}
		private float ConvertToRange(int value)
		{
			float result = 2f * (value) / (DiscreteSize - 1) - 1f;
			return result;
		}
		private void MakeJointActions(Joint joint, float rotX, float rotY, float rotZ, float force, float spring, float damper)
		{
			// force = DELETE_FORCE; TODO: add penalty for force
			//TODO: add offset instead of clamping force
			joint.SetTargetRotation(rotX, rotY, rotZ);
			joint.SetForce(force, spring, damper);
			joint.SetAdditionalData();
		}

		public void DebugMakeAllJointsAction(float rotX, float rotY, float rotZ, float force)
		{
			for (int i = 0; i < joints.Length; i++)
			{
				MakeJointActions(joints[i], rotX, rotY, rotZ, force, 1, 1);
			}
		}

		public override int ActionsCount
		{
			get
			{
				return TotalActionsAmount();
			}
		}

		public override int ObservationsCount
		{
			get
			{
				return TotalObservationAmount();
			}
		}

		public bool ConsiderActions
		{
			get => _considerActions;
			set => _considerActions = value;
		}

		int TotalActionsAmount()
		{
			int index = 0;
			for (int i = 0; i < joints.Length; i++)
			{
				Joint j = joints[i];
				if (j.EnabledRotationX)
					index++;
				if (j.EnabledRotationY)
					index++;
				if (j.EnabledRotationZ)
					index++;

				if (useForce)
				{
					index++;
				}

				if (useSpringDamperAction)
				{
					index += 2;
				}
			}

			print("Joint Controller actions:" + index);

			return index;
		}

		int TotalObservationAmount()
		{
			int result = (observePosition ? ObservationAmountRotations() : 0) +
						  (observeRotation ? ObservationAmountPositions() : 0) +
						  (observeVelocity ? ObservationAmountVelocity() : 0) +
						  (useForce ? ObservationAmountForce() : 0);
			print("Joint Controller observations:" + result);
			return result;
		}

		#region Observations
		//Since parent bone is not configurable joints and just rigidbody, we collect observation of rotation from all rbs
		void ObserveRotations(VectorSensor sensor)
		{
			for (int i = 0; i < rigidBodies.Length; i++)
			{
				sensor.AddObservation(rigidBodies[i].LocalRotation(_mainBone));
			}
		}
		int ObservationAmountRotations() => rigidBodies.Length * 4;

		void ObservePositions(VectorSensor sensor)
		{
			for (int i = 0; i < joints.Length; i++)
			{
				sensor.AddObservation(joints[i].LocalPosition);
			}
		}
		int ObservationAmountPositions() => joints.Length * 3;

		void ObserveForce(VectorSensor sensor)
		{
			for (int i = 0; i < joints.Length; i++)
			{
				sensor.AddObservation(joints[i].CurrentForce);
			}
		}
		int ObservationAmountForce() => joints.Length;

		void ObserveVelocity(VectorSensor sensor)
		{
			for (int i = 0; i < joints.Length; i++)
			{
				sensor.AddObservation(joints[i].LocalVelocityNorm);
				sensor.AddObservation(joints[i].VelocityMagnitude);
				sensor.AddObservation(joints[i].LocalAngularVelocityNorm);
				sensor.AddObservation(joints[i].AngularVelocityMagnitude);
			}
		}
		int ObservationAmountVelocity() => joints.Length * (3 + 1 + 3 + 1);

		#endregion

		private Vector3 GetStartPosition()
		{
			return parentLocalTransformProvider.TransformPoint(_mainBoneStartPos);
		}

		private Quaternion GetStartRotation()
		{
			return parentLocalTransformProvider.rotation * _mainBoneStartRot;
		}

		private void CalculateApplyLocalTransform()
		{
			_mainBoneStartPos = parentLocalTransformProvider.InverseTransformPoint(_mainBone.position);
			_mainBoneStartRot = Quaternion.Inverse(parentLocalTransformProvider.rotation) * _mainBone.rotation;
		}
	}
}