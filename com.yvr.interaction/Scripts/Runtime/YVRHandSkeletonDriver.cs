using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace YVR.Interaction.Runtime
{
// 1. 重写 FindJointsFromRoot 方法适配 YVR 中手部骨骼的命名规则，
// 2. 以及修改 OnJointsUpdated 方法适配 YVR 中手部骨骼的更新方式(YVR 中手部模型与 Unity XRHand 模型不一致，差异在 Z 轴上的旋转角度不同，需要重新计算手部骨骼的本地坐标系)
    public class YVRHandSkeletonDriver : XRHandSkeletonDriver
    {
        [SerializeField] private SkinnedMeshRenderer m_MeshRenderer;

        public bool forceUpdateSkeleton { get; set; }
        private bool willUpdateSkeleton => (m_MeshRenderer != null && m_MeshRenderer.enabled) || forceUpdateSkeleton;

        public override void FindJointsFromRoot(List<string> missingJointNames)
        {
            void SetupJointTransformReference(
                XRHandJointID jointId,
                Transform jointTransform)
            {
                var reference = new JointToTransformReference
                {
                    jointTransform = jointTransform,
                    xrHandJointID = jointId
                };

                this.jointTransformReferences.Add(reference);
            }

            if (missingJointNames != null)
                missingJointNames.Clear();

            jointTransformReferences.Clear();

            Transform wristRootTransform = null;

            // First check if the parent itself is the wrist
            var rootTransformName = rootTransform.name;
            if (StartsOrEndsWith(rootTransformName, XRHandJointID.Wrist.ToString().ToUpper()))
            {
                wristRootTransform = rootTransform;
            }
            else // Otherwise check under parent for wrist joint as a child
            {
                for (var childIndex = 0; childIndex < rootTransform.childCount; ++childIndex)
                {
                    var child = rootTransform.GetChild(childIndex);
                    if (child.gameObject.name.EndsWith(XRHandJointID.Wrist.ToString().ToUpper()))
                        wristRootTransform = child;
                }
            }

            if (wristRootTransform == null)
            {
                if (missingJointNames != null)
                    missingJointNames.Add(XRHandJointID.Wrist.ToString().ToUpper());
            }
            else
            {
                SetupJointTransformReference(XRHandJointID.Wrist, wristRootTransform);
                Transform palmTransform = null;

                // Find all the joints under the wrist
                for (var childIndex = 0; childIndex < wristRootTransform.childCount; ++childIndex)
                {
                    var child = wristRootTransform.GetChild(childIndex);

                    // Find the palm joint
                    if (child.name.EndsWith(XRHandJointID.Palm.ToString().ToUpper()))
                    {
                        palmTransform = child;
                        continue;
                    }

                    // Find the finger joints
                    for (var fingerIndex = (int) XRHandFingerID.Thumb;
                         fingerIndex <= (int) XRHandFingerID.Little;
                         ++fingerIndex)
                    {
                        var fingerId = (XRHandFingerID) fingerIndex;
                        var jointIdFront = fingerId.GetFrontJointID();
                        if (!StartsOrEndsWith(child.name, jointIdFront.ToString()))
                            continue;

                        SetupJointTransformReference(jointIdFront, child);
                        var lastChild = child;
                        var jointIndexBack = fingerId.GetBackJointID().ToIndex();

                        // Find the rest of the joints for the finger
                        for (var jointIndex = jointIdFront.ToIndex() + 1;
                             jointIndex <= jointIndexBack;
                             ++jointIndex)
                        {
                            // Find the next child that ends with the joint name
                            var jointName = XRHandJointIDUtility.FromIndex(jointIndex).ToString();
                            for (var nextChildIndex = 0; nextChildIndex < lastChild.childCount; ++nextChildIndex)
                            {
                                var nextChild = lastChild.GetChild(nextChildIndex);
                                if (StartsOrEndsWith(nextChild.name, jointName))
                                {
                                    lastChild = nextChild;
                                    break;
                                }
                            }

                            if (StartsOrEndsWith(lastChild.name, jointName))
                            {
                                var jointId = XRHandJointIDUtility.FromIndex(jointIndex);
                                SetupJointTransformReference(jointId, lastChild);
                            }
                            else if (missingJointNames != null)
                                missingJointNames.Add(jointName);
                        }
                    }
                }

                for (var fingerIndex = (int) XRHandFingerID.Thumb;
                     fingerIndex <= (int) XRHandFingerID.Little;
                     ++fingerIndex)
                {
                    var fingerId = (XRHandFingerID) fingerIndex;
                    var jointIdFront = fingerId.GetFrontJointID();

                    // Check if front joint id is present in the list of joint references
                    if (jointTransformReferences.Any(jointReference =>
                                                         jointReference.xrHandJointID == jointIdFront))
                        continue;

                    if (missingJointNames != null)
                        missingJointNames.Add(jointIdFront.ToString());
                }

                if (palmTransform != null)
                {
                    SetupJointTransformReference(XRHandJointID.Palm, palmTransform);
                }
                else if (missingJointNames != null)
                {
                    missingJointNames.Add(XRHandJointID.Palm.ToString());
                }
            }
        }

        private bool StartsOrEndsWith(string value, string searchTerm)
        {
            value = value.ToUpper().Replace("_", "");
            searchTerm = searchTerm.ToUpper();
            return value.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
                   value.EndsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase);
        }

        protected override void OnRootPoseUpdated(Pose rootPose) { }

        protected override void OnJointsUpdated(XRHandJointsUpdatedEventArgs args)
        {
            if (!willUpdateSkeleton) return;

            UpdateJointLocalPoses(args);
            ApplyUpdatedTransformPoses();
        }

        protected override void ApplyUpdatedTransformPoses()
        {
            // Apply the local poses to the joint transforms
            for (var i = 0; i < m_JointTransforms.Length; i++)
            {
                if (m_HasJointTransformMask[i])
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (m_JointTransforms[i] == null)
                    {
                        Debug.LogError(
                                       "XR Hand Skeleton has detected that a joint transform has been destroyed after it was initialized." +
                                       " After removing or modifying transform joint references at runtime it is required to call InitializeFromSerializedReferences to update the joint transform references.",
                                       this);

                        continue;
                    }
#endif
                    int index = (i == 0) ? 1 : (i == 1) ? 0 : i;
                    if (index == 0) // 更新手位置
                    {
                        m_JointTransforms[index].SetLocalPositionAndRotation(m_JointLocalPoses[index].position,
                                                                             m_JointLocalPoses[index].rotation);
                    }
                    else
                    {
                        Quaternion worldRotation = this.transform.rotation *
                                                   m_JointLocalPoses[i].rotation;
                        var localRotation = Quaternion.Inverse(m_JointTransforms[index].parent.rotation) *
                                            worldRotation;
                        m_JointTransforms[index].localRotation = localRotation;
                    }
                }
            }
        }

        private new void UpdateJointLocalPoses(XRHandJointsUpdatedEventArgs args)
        {
            // Calculate the local poses for all the joints, accessing the internal joints array to enable burst compilation when available
            ChangeJointsOrientation(ref args, ref m_JointLocalPoses);
        }

        private void ChangeJointsOrientation(ref XRHandJointsUpdatedEventArgs args,
                                             ref NativeArray<Pose> jointLocalPoses)
        {
            for (int i = 0; i < jointLocalPoses.Length; i++)
            {
                args.hand.GetJoint((XRHandJointID) (i + 1)).TryGetPose(out var jointpose);
                Quaternion rot = jointpose.rotation;
                Pose covertpose;
                covertpose.position = jointpose.position;
                covertpose.rotation = rot * Quaternion.AngleAxis(180f, Vector3.up);
                jointLocalPoses[i] = covertpose;
            }
        }
    }
}