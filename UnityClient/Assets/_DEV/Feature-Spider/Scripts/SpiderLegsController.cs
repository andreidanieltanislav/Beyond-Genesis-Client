using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderLegsController : MonoBehaviour
{
    public LegIKTargetPair[] legIKsTargets;
    public float maxLegDistance = 2f;

    private List<Collider> _legsColliders;

    private void Awake()
    {
        _legsColliders = GetComponentsInChildren<Collider>().ToList();
    }

    private void LateUpdate()
    {
        UpdateTargets();
        UpdateLegs();
    }

    private void UpdateTargets()
    {
        // Temporarily put legs on ignore layer, assuming all legs are on same layer initially
        int previousLayer = legIKsTargets[0].legIK.gameObject.layer;
        _legsColliders.ForEach(legCollider => legCollider.gameObject.layer = 2);
        
        foreach (LegIKTargetPair pair in legIKsTargets)
        {
            Transform legTarget = pair.legTarget;
            Vector3 raycastOrigin = legTarget.position + Vector3.up * 0.5f;
            if (Physics.Raycast(raycastOrigin, Vector3.down, out RaycastHit hit))
            {
                pair.legTarget.position = hit.point;
                pair.legTarget.forward = hit.normal;
            }
        }
        
        // Restore layer
        _legsColliders.ForEach(legCollider => legCollider.gameObject.layer = previousLayer);
    }

    private void UpdateLegs()
    {
        foreach (LegIKTargetPair pair in legIKsTargets)
        {
            Vector3 targetPosition = pair.legTarget.position;
            Vector3 targetForward = pair.legTarget.forward;
            float sqrDistanceBetweenLegAndTarget = (pair.legIK.LegIKTarget.Position - targetPosition).sqrMagnitude;
            
            if (sqrDistanceBetweenLegAndTarget > maxLegDistance * maxLegDistance)
            {
                // Leg is too far away, update its target (position)
                Debug.Log("Updating leg position");
                pair.legIK.LegIKTarget = new SpiderLegIKTarget {Normal = targetForward, Position = targetPosition};
            }
        }
    }
}

[Serializable]
public struct LegIKTargetPair
{
    public SpiderLegIK legIK;
    public Transform legTarget;
}