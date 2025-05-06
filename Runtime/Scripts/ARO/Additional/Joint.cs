using System;
using UnityEngine;
using Greyrats.JointAI.Analysis;

namespace Greyrats.JointAI.ARO
{
	public class Joint
	{
		public Quaternion LocalRotation => Quaternion.Inverse(_mainBone.rotation) * _transform.rotation;
		public Vector3 LocalPosition => _mainBone.InverseTransformPoint(_transform.localPosition);
		public Vector3 LocalVelocityNorm => _mainBone.InverseTransformDirection(_rb.linearVelocity).normalized;
		public float VelocityMagnitude => Vector3.Magnitude(_rb.linearVelocity);
		public Vector3 LocalAngularVelocityNorm => _mainBone.InverseTransformDirection(_rb.angularVelocity).normalized;
		public float AngularVelocityMagnitude => Vector3.Magnitude(_rb.linearVelocity);
		public float CurrentForce => _configurableJoint.slerpDrive.maximumForce;

		public bool EnabledRotationX => _configurableJoint.angularXMotion == ConfigurableJointMotion.Free ||
										_configurableJoint.angularXMotion == ConfigurableJointMotion.Limited;
		public bool EnabledRotationY => _configurableJoint.angularYMotion == ConfigurableJointMotion.Free ||
										_configurableJoint.angularYMotion == ConfigurableJointMotion.Limited;
		public bool EnabledRotationZ => _configurableJoint.angularZMotion == ConfigurableJointMotion.Free ||
										_configurableJoint.angularZMotion == ConfigurableJointMotion.Limited;


		private ConfigurableJoint _configurableJoint;
		private ConfigurableJointConstraints _jointConstraints;
		private JointsControllerARO _controllerAro;
		private Transform _transform;
		private Transform _mainBone;
		private Rigidbody _rb;

		private Vector3 _startLocalPos;
		private Quaternion _startLocalRot;
		private JointLimitOffset _limitOffset;
		private Collider _collider;

		public Vector3 StartLocalPos => _startLocalPos;
		public Quaternion StartLocalRot => _startLocalRot;
		public Rigidbody RB => _rb;

		public Joint(ConfigurableJoint joint, ConfigurableJointConstraints jointConstraints, JointsControllerARO controllerAro, Transform mainBone)
		{
			_configurableJoint = joint;
			_jointConstraints = jointConstraints;
			_transform = joint.transform;
			_controllerAro = controllerAro;
			_rb = joint.GetComponent<Rigidbody>();
			_collider = joint.GetComponent<Collider>();
			if (_collider == null)
			{
				for (int i = 0; i < joint.transform.childCount; i++	)
				{
					var child = joint.transform.GetChild(i);
					if (child.TryGetComponent<Collider>(out var tmpCollider))
					{
						_collider = tmpCollider;
						break;
					}
				}
				if (_collider == null)
				{
					throw new Exception("No collider found for joint: " + joint.name);
				}

			}
			_mainBone = mainBone;

			if (joint.TryGetComponent<JointLimitOffset>(out var offset))
			{
				_limitOffset = offset;
			}
		}

		public void Initialize()
		{
			_startLocalPos = _transform.localPosition;
			_startLocalRot = _transform.localRotation;
		}

		public void Reset()
		{
			_transform.localPosition = _startLocalPos;
			_transform.localRotation = _startLocalRot;
			_rb.linearVelocity = Vector3.zero;
			_rb.angularVelocity = Vector3.zero;
		}

		public void SetLocalTransformation(Vector3 localPos, Quaternion localRot)
		{
			_transform.localPosition = localPos;
			_transform.localRotation = localRot;
		}

		public void StopRigidbody()
		{
			_rb.linearVelocity = Vector3.zero;
			_rb.angularVelocity = Vector3.zero;
		}

		public void SetColliderActive(bool active)
		{
			_collider.enabled = active;
		}

		public void SetForce(float force, float spring, float damper)
		{
			if (!_controllerAro.AlwaysChangeValues)
				return;
			var rawValForce = (force + 1f) * 0.5f * _controllerAro.MaxForceLimit;
			if (float.IsInfinity(rawValForce))
				rawValForce = 1;
			if (rawValForce > 3.402823e+30)
				rawValForce = 3.402823e+30f;

			var clampSpring = (spring + 1f) * 0.5f;
			var clampDamper = (damper + 1f) * 0.5f;
			float finalSpring;
			float finalDamper;
			if (_controllerAro.useSpringDamperAction)
			{
				finalSpring = Mathf.Lerp(_controllerAro.MinSpringValue, _controllerAro.MaxSpringValue, clampSpring);
				finalDamper = Mathf.Lerp(_controllerAro.MinDamperValue, _controllerAro.MaxDamperValue, clampDamper);
			}
			else
			{
				finalSpring = _controllerAro.MaxSpring;
				finalDamper = _controllerAro.Dampen;
			}

			var jd = new JointDrive
			{
				positionSpring = finalSpring,
				positionDamper = finalDamper,
				maximumForce = rawValForce,
				useAcceleration = _controllerAro.UseAcceleration
			};
			_configurableJoint.slerpDrive = jd;
			_configurableJoint.rotationDriveMode = RotationDriveMode.Slerp;
		}

		public void SetAdditionalData()
		{
			if (!_controllerAro.AlwaysChangeValues)
				return;
			_configurableJoint.massScale = _controllerAro.massScaler;
			_configurableJoint.connectedMassScale = _controllerAro.connectedMassScaler;
			_configurableJoint.angularXLimitSpring = new SoftJointLimitSpring() { spring = _controllerAro.anguarLimitSpring, damper = _controllerAro.angularLimitDampen };
			_configurableJoint.angularYZLimitSpring = new SoftJointLimitSpring() { spring = _controllerAro.anguarLimitSpring, damper = _controllerAro.angularLimitDampen };
			_configurableJoint.lowAngularXLimit = new SoftJointLimit()
			{
				limit = _configurableJoint.lowAngularXLimit.limit,
				bounciness = _controllerAro.bouncines,
				contactDistance = _controllerAro.contactDistance
			};
			_configurableJoint.highAngularXLimit = new SoftJointLimit()
			{
				limit = _configurableJoint.highAngularXLimit.limit,
				bounciness = _controllerAro.bouncines,
				contactDistance = _controllerAro.contactDistance
			};
			_configurableJoint.angularYLimit = new SoftJointLimit()
			{
				limit = _configurableJoint.angularYLimit.limit,
				bounciness = _controllerAro.bouncines,
				contactDistance = _controllerAro.contactDistance
			};
			_configurableJoint.angularZLimit = new SoftJointLimit()
			{
				limit = _configurableJoint.angularZLimit.limit,
				bounciness = _controllerAro.bouncines,
				contactDistance = _controllerAro.contactDistance
			};
			_configurableJoint.configuredInWorldSpace = _controllerAro.ConfiguredInWorldSpace;
			_configurableJoint.swapBodies = false;
			_configurableJoint.enableCollision = _controllerAro.EnableCollision;
			_configurableJoint.enablePreprocessing = _controllerAro.EnablePreprocessing;
			_configurableJoint.projectionMode = _controllerAro.JointProjectionMode;
			_rb.maxAngularVelocity = _controllerAro.maxAngularVelocity;
			_rb.maxLinearVelocity = _controllerAro.maxlinearVelocity;
			_configurableJoint.projectionDistance = _controllerAro.projectionDistance;
			_configurableJoint.projectionAngle = _controllerAro.projectionAngle;
		}

		/// <summary>
		/// 0-1 Range
		/// </summary>
		/// <param name="x">0-1</param>
		/// <param name="y">0-1</param>
		/// <param name="z">0-1</param>
		public void SetTargetRotation(float x, float y, float z)
		{
			x = (x + 1f) * 0.5f;
			y = (y + 1f) * 0.5f;
			z = (z + 1f) * 0.5f;
			if (_limitOffset != null)
			{
				x += _limitOffset.offset.x;
				y += _limitOffset.offset.y;
				z += _limitOffset.offset.z;
			}

			x = Mathf.Clamp01(x);
			y = Mathf.Clamp01(y);
			z = Mathf.Clamp01(z);

			float xRot;
			float yRot;
			float zRot;
			if (_configurableJoint.angularXMotion == ConfigurableJointMotion.Free)
				xRot = Mathf.Lerp(-_controllerAro.freeJointLimitValue, _controllerAro.freeJointLimitValue, x);
			else
			{
				if(_jointConstraints != null)
				{
					xRot = Mathf.Lerp(_configurableJoint.lowAngularXLimit.limit, _configurableJoint.highAngularXLimit.limit,
					 Mathf.Clamp(x, _jointConstraints.rotationX.min, _jointConstraints.rotationX.max));
				}
				else
				{
					xRot = Mathf.Lerp(_configurableJoint.lowAngularXLimit.limit, _configurableJoint.highAngularXLimit.limit, x);
				}
			}

			if (_configurableJoint.angularYMotion == ConfigurableJointMotion.Free)
				yRot = Mathf.Lerp(-_controllerAro.freeJointLimitValue, _controllerAro.freeJointLimitValue, y);
			else
			{
				if(_jointConstraints != null)
				{
					yRot = Mathf.Lerp(-_configurableJoint.angularYLimit.limit, _configurableJoint.angularYLimit.limit,
					 Mathf.Clamp(y, _jointConstraints.rotationY.min, _jointConstraints.rotationY.max));
				}
				else
				{
					yRot = Mathf.Lerp(-_configurableJoint.angularYLimit.limit, _configurableJoint.angularYLimit.limit, y);
				}
			}

			if (_configurableJoint.angularZMotion == ConfigurableJointMotion.Free)
				zRot = Mathf.Lerp(-_controllerAro.freeJointLimitValue, _controllerAro.freeJointLimitValue, z);
			else
			{
				if(_jointConstraints != null)
				{
					zRot = Mathf.Lerp(-_configurableJoint.angularZLimit.limit, _configurableJoint.angularZLimit.limit,
					 Mathf.Clamp(z, _jointConstraints.rotationZ.min, _jointConstraints.rotationZ.max));
				}
				else
				{
					zRot = Mathf.Lerp(-_configurableJoint.angularZLimit.limit, _configurableJoint.angularZLimit.limit, z);
				}
			}
			_configurableJoint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
		}
	}
}