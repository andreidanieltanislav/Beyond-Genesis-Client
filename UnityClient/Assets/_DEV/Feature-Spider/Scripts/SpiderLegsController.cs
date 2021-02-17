using System;
using UnityEngine;

public class SpiderLegsController : MonoBehaviour
{
    public LegIKTargetPair[] legIKsTargets;
    public float maxLegDistance = 2f;

    private void LateUpdate()
    {
        UpdateLegsTarget();
    }

    private void UpdateLegsTarget()
    {
        foreach (LegIKTargetPair pair in legIKsTargets)
        {
            Vector3 targetPosition = pair.legTarget.position;
            float sqrDistanceBetweenLegAndTarget = (pair.legIK.LegIKTarget.Position - targetPosition).sqrMagnitude;
            
            if (sqrDistanceBetweenLegAndTarget > maxLegDistance * maxLegDistance)
            {
                Debug.Log("Updating leg position");
                // Leg is too far away, update its target (position)
                pair.legIK.LegIKTarget = new SpiderLegIKTarget {Normal = Vector3.up, Position = targetPosition};
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