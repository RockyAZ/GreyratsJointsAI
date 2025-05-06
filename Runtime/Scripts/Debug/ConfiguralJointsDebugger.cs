using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;

namespace Greyrats.JointAI.Analysis
{
    [ExecuteInEditMode]
    public class ConfiguralJointsDebugger : MonoBehaviour
    {
        private ConfigurableJoint joint;
        private ConfigurableJointConstraints jointConstraints;
        private Vector3 savedPosition;
        private Quaternion savedRotation;
        private bool initialized = false;

        // Rotation visualization sliders
        [Header("Rotation Visualization")]
        [Range(0, 1)] public float xAxisRotation = 0.5f;
        [Range(0, 1)] public float yAxisRotation = 0.5f;
        [Range(0, 1)] public float zAxisRotation = 0.5f;
        [Range(0, 1)] public float xyzCombinedRotation = 0.5f;

        // Joint limits information display (read-only)
        [Header("Joint Limits Info (Read-Only)")]
        [SerializeField] private float lowXLimit;
        [SerializeField] private float highXLimit;
        [SerializeField] private float yLimit;
        [SerializeField] private float zLimit;

        private void OnEnable()
        {
            // Skip if in play mode
            if (Application.isPlaying)
                return;

            // Initialize on component being added
            if (!initialized)
            {
                SaveTransform();
                initialized = true;
            }
        }

        private void Awake()
        {
            // Skip if in play mode
            if (Application.isPlaying)
                return;

            // Get the ConfigurableJoint component
            joint = GetComponent<ConfigurableJoint>();
            jointConstraints = GetComponent<ConfigurableJointConstraints>();

            if (joint != null)
            {
                // Update the display of joint limits
                UpdateJointLimitsInfo();
            }
        }

        private void UpdateJointLimitsInfo()
        {
            // Skip if in play mode
            if (Application.isPlaying)
                return;

            if (joint != null)
            {
                // Display the current joint limits
                lowXLimit = joint.lowAngularXLimit.limit;
                highXLimit = joint.highAngularXLimit.limit;
                yLimit = joint.angularYLimit.limit;
                zLimit = joint.angularZLimit.limit;
            }
        }

        // Update is called once per frame in play mode or when something changes in editor
        void Update()
        {
            // Skip everything if in play mode
            if (Application.isPlaying)
                return;
            if (jointConstraints == null)
            {
                jointConstraints = GetComponent<ConfigurableJointConstraints>();
            }


            // Update the joint limits display
            if (joint != null)
            {
                UpdateJointLimitsInfo();
            }

            // Calculate rotation based on sliders
            Vector3 rotation = Vector3.zero;

            // If combined rotation is active, use it to influence all axes
            if (xyzCombinedRotation != 0.5f)
            {
                if (joint != null)
                {
                    // X rotation - map 0-1 from low to high limit
                    if (joint.angularXMotion == ConfigurableJointMotion.Limited)
                    {
                        if (jointConstraints != null)
                        {
                            rotation.x = Mathf.Lerp(lowXLimit, highXLimit, Mathf.Clamp(xyzCombinedRotation, jointConstraints.rotationX.min, jointConstraints.rotationX.max));
                        }
                        else
                        {
                            rotation.x = Mathf.Lerp(lowXLimit, highXLimit, xyzCombinedRotation);
                        }
                    }

                    // Y rotation - map 0-1 from -yLimit to +yLimit
                    if (joint.angularYMotion == ConfigurableJointMotion.Limited)
                    {
                        if (jointConstraints != null)
                        {
                            rotation.y = Mathf.Lerp(-yLimit, yLimit, Mathf.Clamp(xyzCombinedRotation, jointConstraints.rotationY.min, jointConstraints.rotationY.max));
                        }
                        else
                        {
                            rotation.y = Mathf.Lerp(-yLimit, yLimit, xyzCombinedRotation);
                        }
                    }

                    // Z rotation - map 0-1 from -zLimit to +zLimit
                    if (joint.angularZMotion == ConfigurableJointMotion.Limited)
                    {
                        if (jointConstraints != null)
                        {
                            rotation.z = Mathf.Lerp(-zLimit, zLimit, Mathf.Clamp(xyzCombinedRotation, jointConstraints.rotationZ.min, jointConstraints.rotationZ.max));
                        }
                        else
                        {
                            rotation.z = Mathf.Lerp(-zLimit, zLimit, xyzCombinedRotation);
                        }
                    }
                }
            }
            else
            {
                // Individual axis rotations
                if (joint != null)
                {
                    // X rotation - map 0-1 from low to high limit
                    if (joint.angularXMotion == ConfigurableJointMotion.Limited && xAxisRotation != 0.5f)
                    {
                        if (jointConstraints != null)
                        {
                            rotation.x = Mathf.Lerp(lowXLimit, highXLimit, Mathf.Clamp(xAxisRotation, jointConstraints.rotationX.min, jointConstraints.rotationX.max));
                        }
                        else
                        {
                            rotation.x = Mathf.Lerp(lowXLimit, highXLimit, xAxisRotation);
                        }
                    }
                    // Y rotation - map 0-1 from -yLimit to +yLimit
                    if (joint.angularYMotion == ConfigurableJointMotion.Limited && yAxisRotation != 0.5f)
                    {
                        if (jointConstraints != null)
                        {
                            rotation.y = Mathf.Lerp(-yLimit, yLimit, Mathf.Clamp(yAxisRotation, jointConstraints.rotationY.min, jointConstraints.rotationY.max));
                        }
                        else
                        {
                            rotation.y = Mathf.Lerp(-yLimit, yLimit, yAxisRotation);
                        }
                    }
                    // Z rotation - map 0-1 from -zLimit to +zLimit
                    if (joint.angularZMotion == ConfigurableJointMotion.Limited && zAxisRotation != 0.5f)
                    {
                        if (jointConstraints != null)
                        {
                            rotation.z = Mathf.Lerp(-zLimit, zLimit, Mathf.Clamp(zAxisRotation, jointConstraints.rotationZ.min, jointConstraints.rotationZ.max));
                        }
                        else
                        {
                            rotation.z = Mathf.Lerp(-zLimit, zLimit, zAxisRotation);
                        }
                    }
                }
            }

            // Apply the rotation
            transform.localRotation = savedRotation * Quaternion.Euler(rotation);
        }

        [Button]
        public void SaveTransform()
        {
            // Skip if in play mode
            if (Application.isPlaying)
                return;

            savedPosition = transform.localPosition;
            savedRotation = transform.localRotation;
            Debug.Log("Transform saved!");
        }

        [Button]
        public void ReturnToSaved()
        {
            // Skip if in play mode
            if (Application.isPlaying)
                return;

            transform.localPosition = savedPosition;
            transform.localRotation = savedRotation;

            // Reset all sliders to middle position
            xAxisRotation = 0.5f;
            yAxisRotation = 0.5f;
            zAxisRotation = 0.5f;
            xyzCombinedRotation = 0.5f;

            Debug.Log("Returned to saved transform!");
        }
    }
}