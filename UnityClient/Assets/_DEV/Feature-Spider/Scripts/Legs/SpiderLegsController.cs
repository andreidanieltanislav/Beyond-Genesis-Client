using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderLegsController : MonoBehaviour
{
    public LegIKTargetPair[] legIKsTargets;
    public float maxLegDistance = 2f;
    public float legRaiseHeight = 1f;
    public float timeToMoveLeg = 0.5f;

    private List<Collider> _legsColliders;
    private Dictionary<SpiderLegIK, IEnumerator> _runningLegInterpolations;

    public bool manuallyInitialized;
    private bool _initialized;

    public void Init(LegIKTargetPair[] legIKsTargets, float maxLegDistance,
        float legRaiseHeight, float timeToMoveLeg)
    {
        this.legIKsTargets = legIKsTargets;
        this.maxLegDistance = maxLegDistance;
        this.legRaiseHeight = legRaiseHeight;
        this.timeToMoveLeg = timeToMoveLeg;
        
        _legsColliders = GetComponentsInChildren<Collider>().ToList();
        _runningLegInterpolations = new Dictionary<SpiderLegIK, IEnumerator>(legIKsTargets.Length);
        
        _initialized = true;
    }

    private void Awake()
    {
        if (manuallyInitialized)
        {
            _legsColliders = GetComponentsInChildren<Collider>().ToList();
            _runningLegInterpolations = new Dictionary<SpiderLegIK, IEnumerator>(legIKsTargets.Length);
        }
    }

    private void LateUpdate()
    {
        if (!_initialized && !manuallyInitialized)
        {
            return;
        }
        
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
            SpiderLegIK legIK = pair.legIK;
            float sqrDistanceBetweenLegAndTarget = (legIK.CurrentIKTarget.Position - targetPosition).sqrMagnitude;

            if (sqrDistanceBetweenLegAndTarget > maxLegDistance * maxLegDistance)
            {
                // Leg is too far away, update its target (position)
                Debug.Log("Updating leg position");
                SpiderLegIKTarget nextTarget = new SpiderLegIKTarget
                {
                    Normal = targetForward,
                    Position = targetPosition
                };
                if (_runningLegInterpolations.ContainsKey(legIK))
                {
                    return;
                    // StopCoroutine(_runningLegInterpolations[legIK]);
                }

                _runningLegInterpolations[legIK] = InterpolateLegPosition(legIK, legIK.CurrentIKTarget, nextTarget);
                StartCoroutine(_runningLegInterpolations[legIK]);
            }
        }
    }

    private IEnumerator InterpolateLegPosition(SpiderLegIK leg, SpiderLegIKTarget previousTarget,
        SpiderLegIKTarget nextTarget)
    {
        Debug.Log("Started coroutine");
        float elapsedTime = 0f;
        Vector3 prevPos = previousTarget.Position;
        Vector3 prevNormal = previousTarget.Normal;
        Vector3 nextPos = nextTarget.Position;
        Vector3 nextNormal = nextTarget.Normal;
        Vector3 middlePos = Vector3.Slerp(prevPos, nextPos, 0.5f);
        Vector3 middleNormal = Vector3.Slerp(prevNormal, nextNormal, 0.5f);
        /*
         * raisedMiddlePos Might not look like raising at all
         * when walking uneven terrain.
         * Possible idea for enhancement: get the highest point of all 3
         * (prev, middle, next).
         */
        Vector3 raisedMiddlePos = middlePos + middleNormal * legRaiseHeight;

        Vector3 lastRaisedPos = prevPos;

        while (elapsedTime < timeToMoveLeg)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / timeToMoveLeg);

            Vector3 newPos, newNormal;
            if (t < timeToMoveLeg / 2)
            {
                newPos = Vector3.Slerp(prevPos, raisedMiddlePos, t * 2);
                newNormal = Vector3.Slerp(prevNormal, middleNormal, t * 2);
                lastRaisedPos = newPos;
            }
            else
            {
                newPos = Vector3.Slerp(lastRaisedPos, nextPos, (t - timeToMoveLeg / 2) * 2);
                newNormal = Vector3.Slerp(middleNormal, nextNormal, (t - timeToMoveLeg / 2) * 2);
            }

            // newPos = Vector3.Lerp(prevPos, nextPos, t);
            // newNormal = Vector3.Lerp(prevNormal, nextNormal, t);

            leg.CurrentIKTarget = new SpiderLegIKTarget
            {
                Normal = newNormal,
                Position = newPos
            };
            yield return null;
        }

        _runningLegInterpolations.Remove(leg);
    }
}

[Serializable]
public struct LegIKTargetPair
{
    public SpiderLegIK legIK;
    public Transform legTarget;
}