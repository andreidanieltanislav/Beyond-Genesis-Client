using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Experimental.Animations;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Animations;

// WORK IN PROGRESS
// NOT FUNCTIONAL



[BurstCompile]
public struct LimbIKJob : IWeightedAnimationJob
{
    public ReadWriteTransformHandle root;
    public ReadWriteTransformHandle mid;
    public ReadWriteTransformHandle tip;

    public ReadOnlyTransformHandle hint;
    public ReadOnlyTransformHandle target;

    public AffineTransform targetOffset;
    public Vector2 linkLengths;

    public FloatProperty targetPositionWeight;
    public FloatProperty targetRotationWeight;
    public FloatProperty hintWeight;

    public FloatProperty jobWeight { get; set; }

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        float w = jobWeight.Get(stream);
        if (w > 0f)
        {
            // [TODO] Needs work-around for physics
            /*RaycastHit hit;
            if (Physics.Raycast(target.GetPosition(stream), Vector3.down, out hit, 10f))
                targetOffset.translation = hit.point;*/

            AnimationRuntimeUtils.SolveTwoBoneIK(
                stream, root, mid, tip, target, hint,
                targetPositionWeight.Get(stream) * w,
                targetRotationWeight.Get(stream) * w,
                hintWeight.Get(stream) * w,
                linkLengths,
                targetOffset
                );
        }
        else
        {
            AnimationRuntimeUtils.PassThrough(stream, root);
            AnimationRuntimeUtils.PassThrough(stream, mid);
            AnimationRuntimeUtils.PassThrough(stream, tip);
        }
    }
}

/// LimbIKData contains all necessary data needed by the LimbIKJob
[Serializable]
public struct LimbIKData : IAnimationJobData
{

    [SyncSceneToStream] public Transform root;
    public Transform mid;
    public Transform tip;
    [SyncSceneToStream] public Transform target;
    [SyncSceneToStream] public Transform hint;

    public bool maintainTargetPositionOffset;
    public bool maintainTargetRotationOffset;

    [SyncSceneToStream] public float limbOffset;

    public string targetPositionWeightFloatProperty;
    public string targetRotationWeightFloatProperty;
    public string hintWeightFloatProperty;


    public bool IsValid()
    {
        if (root == null || mid == null || tip == null 
                                || root == mid || root == tip)
            return false;

        return true;
    }

    public void SetDefaultValues()
    {
        root = null;
        mid = null;
        tip = null;

        target = null;
        hint = null;

        limbOffset = 0;

        maintainTargetPositionOffset = false;
        maintainTargetRotationOffset = false;

        targetPositionWeightFloatProperty = "";
        targetRotationWeightFloatProperty = "";
        hintWeightFloatProperty = "";
    }
}

/// LimbIKBinder creates and destroys a LimbIKJob given specified LimbIKData
public class LimbIKBinder : AnimationJobBinder<LimbIKJob, LimbIKData>
{
    public override LimbIKJob Create(Animator animator, ref LimbIKData data, Component component)
    {
        var job = new LimbIKJob();

        job.root = ReadWriteTransformHandle.Bind(animator, data.root);
        job.mid = ReadWriteTransformHandle.Bind(animator, data.mid);
        job.tip = ReadWriteTransformHandle.Bind(animator, data.tip);

        data.target.Translate(data.target.forward * data.limbOffset);
        job.target = ReadOnlyTransformHandle.Bind(animator, data.target);

        if (data.hint != null)
            job.hint = ReadOnlyTransformHandle.Bind(animator, data.hint);

        job.targetOffset = AffineTransform.identity;
        if (data.maintainTargetPositionOffset)
            job.targetOffset.translation = data.tip.position - data.target.position;
        if (data.maintainTargetRotationOffset)
            job.targetOffset.rotation = Quaternion.Inverse(data.target.rotation) * data.tip.rotation;

        job.linkLengths[0] = Vector3.Distance(data.root.position, data.mid.position);
        job.linkLengths[1] = Vector3.Distance(data.mid.position, data.tip.position);

        job.targetPositionWeight = FloatProperty.Bind(animator, component, data.targetPositionWeightFloatProperty);
        job.targetRotationWeight = FloatProperty.Bind(animator, component, data.targetRotationWeightFloatProperty);
        job.hintWeight = FloatProperty.Bind(animator, component, data.hintWeightFloatProperty);

        return job;
    }

    public override void Destroy(LimbIKJob job)
    {
    }
}

/// LimbIK constraint component can be defined given it's job, data and binder
[DisallowMultipleComponent]
public class LimbIK : RigConstraint<LimbIKJob, LimbIKData, LimbIKBinder>
{ }