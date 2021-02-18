using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpiderLegsController : MonoBehaviour
{
    public LegIKTargetPair[] legIKsTargets;
    public float maxLegDistance = 2f;
    public float minLegDistance = 1f;
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
        // UpdateLegs();
        UpdateCrossLegs();
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

    private void UpdateCrossLegs()
    {
        if (_runningLegInterpolations.Count > 0)
        {
            return;
        }
        // Check first legs. TODO: Replace with first zig-zag line

        LegIKTargetPair firstPair = legIKsTargets[0];
        float distToTarget = (firstPair.legIK.CurrentIKTarget.Position - firstPair.legTarget.position).magnitude;
        if (distToTarget < minLegDistance)
        {
            return;
        }
        
        int legsCount = legIKsTargets.Length;
        _runningLegInterpolations[firstPair.legIK] = PreparePairForInterpolation(firstPair);
        StartCoroutine(_runningLegInterpolations[firstPair.legIK]);
        for (int i = 3; i < legsCount; i+= 4)
        {
            LegIKTargetPair pair = legIKsTargets[i];
            _runningLegInterpolations[pair.legIK] = PreparePairForInterpolation(pair);
            StartCoroutine(_runningLegInterpolations[pair.legIK]);
            if (i + 1 < legsCount)
            {
                pair = legIKsTargets[i + 1];
                _runningLegInterpolations[pair.legIK] = PreparePairForInterpolation(pair);
                StartCoroutine(_runningLegInterpolations[pair.legIK]);
            }
        }

        // Drag the other legs, no matter the distance
        StartCoroutine(UpdateOtherLegs());

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

    // Not a coroutine, just a shorthand
    private IEnumerator PreparePairForInterpolation(LegIKTargetPair pair)
    {
        SpiderLegIK leg = pair.legIK;
        SpiderLegIKTarget nextTarget = new SpiderLegIKTarget
        {
            Normal = pair.legTarget.forward,
            Position = pair.legTarget.position
        };
        return InterpolateLegPosition(leg, leg.CurrentIKTarget, nextTarget);
    }

    private IEnumerator UpdateOtherLegs2()
    {
        yield return new WaitWhile(() => _runningLegInterpolations.Count > 0);
        LegIKTargetPair pair1 = legIKsTargets[2];
        LegIKTargetPair pair2 = legIKsTargets[3];
        foreach (LegIKTargetPair pair in new[] {pair1, pair2})
        {
            SpiderLegIKTarget nextTarget = new SpiderLegIKTarget
            {
                Normal = pair.legTarget.forward,
                Position = pair.legTarget.position
            };
            _runningLegInterpolations[pair.legIK] = InterpolateLegPosition(
                pair.legIK, pair.legIK.CurrentIKTarget, nextTarget);
            StartCoroutine(_runningLegInterpolations[pair.legIK]);
        }
    }
    
    private IEnumerator UpdateOtherLegs()
    {
        yield return new WaitWhile(() => _runningLegInterpolations.Count > 0);

        int legsCount = legIKsTargets.Length;
        for (int i = 1; i < legsCount; i += 4)
        {
            LegIKTargetPair pair = legIKsTargets[i];
            _runningLegInterpolations[pair.legIK] = PreparePairForInterpolation(pair);
            StartCoroutine(_runningLegInterpolations[pair.legIK]);

            if (i + 1 < legsCount)
            {
                pair = legIKsTargets[i+1];
                _runningLegInterpolations[pair.legIK] = PreparePairForInterpolation(pair);
                StartCoroutine(_runningLegInterpolations[pair.legIK]);
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