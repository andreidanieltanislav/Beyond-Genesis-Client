using System.Collections.Generic;
using UnityEngine;

public class SpiderGenerator : MonoBehaviour
{
    public Vector3 bodyScale = new Vector3(1, 1, 2);
    public float upperLegLength = 1.5f, middleLegLength = 1.5f, lowerLegLength = 1f;
    public float legsWidth = 0.5f;
    public float ikDelta = 0.01f;
    public int ikIterations = 100;
    public int legsPairCount = 4;
    public float maxLegDistance = 2f;
    public float minLegDistance = 1f; // Must be lower than the leg's total length
    public float legsRaiseHeight = 1f;
    public float timeToMoveLeg = 0.5f;
    public float spaceBetweenTargetsScale = 2f;

    private Transform _legTargets;

    private void Awake()
    {
        CreateSpider();
    }

    private void CreateSpider()
    {
        /*
         * Hierarchy (JSON-ish):
         * this GameObject: {
         *      Spine: {
         *          Cube (main body),
         *          Head: Sphere,
         *          Legs: [Leg: {
         *              UpperLeg: {
         *                  Cube,
         *                  MiddleLeg: {
         *                      Cube,
         *                      LowerLeg: Cube
         *                  }
         *              }
         *          }]
         *      },
         *      LegTargets: [ GameObjects ],
         * }
         */
        SpiderLegsController legsController = gameObject.AddComponent<SpiderLegsController>();
        List<LegIKTargetPair> legIKTargetPairs = new List<LegIKTargetPair>(legsPairCount * 2);

        // Spine
        GameObject spine = new GameObject("Spine");
        spine.transform.parent = transform;
        spine.transform.localPosition = Vector3.zero;

        GameObject spineBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        spineBody.transform.localScale = bodyScale;
        spineBody.transform.SetParent(spine.transform, false);

        // Head
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.parent = spine.transform;
        head.transform.localScale = new Vector3(bodyScale.x, bodyScale.x, bodyScale.x);
        head.transform.localPosition = new Vector3(0, 0, bodyScale.z / 2);

        // Legs
        GameObject legs = new GameObject("Legs");
        legs.transform.parent = spine.transform;

        GameObject legTargets = new GameObject("LegTargets");
        legTargets.transform.parent = transform;

        float bodyZStep = bodyScale.z / (legsPairCount + 1);
        float bodyZStart = bodyScale.z / 2;
        float targetsZStart = bodyScale.z * spaceBetweenTargetsScale / 2;
        float targetsZStep = bodyScale.z * spaceBetweenTargetsScale / (legsPairCount + 1);
        for (int i = 0; i < legsPairCount; i++)
        {
            float zPos = bodyZStart - bodyZStep * (i + 1); // TODO: Tweak
            float targetZPos = targetsZStart - targetsZStep * (i + 1);
            SpiderLegIK rightLeg = CreateLeg($" (Right {i})");
            rightLeg.transform.parent = legs.transform;
            rightLeg.transform.localPosition = new Vector3(bodyScale.x / 2, 0, zPos);

            SpiderLegIK leftLeg = CreateLeg($" (Left {i})");
            leftLeg.transform.parent = legs.transform;
            leftLeg.transform.localPosition = new Vector3(-bodyScale.x / 2, 0, zPos);

            GameObject rightLegTarget = new GameObject($"LegTarget (Right {i})");
            rightLegTarget.transform.parent = legTargets.transform;
            rightLegTarget.transform.localPosition = new Vector3(minLegDistance, 0, targetZPos);
            rightLegTarget.transform.localScale = Vector3.one * 0.5f;

            GameObject leftLegTarget = new GameObject($"LegTarget (Left {i})");
            leftLegTarget.transform.parent = legTargets.transform;
            leftLegTarget.transform.localPosition = new Vector3(-minLegDistance, 0, targetZPos);
            leftLegTarget.transform.localScale = Vector3.one * 0.5f;

            LegIKTargetPair pair = new LegIKTargetPair
            {
                legIK = rightLeg,
                legTarget = rightLegTarget.transform
            };
            legIKTargetPairs.Add(pair);
            pair = new LegIKTargetPair
            {
                legIK = leftLeg,
                legTarget = leftLegTarget.transform
            };
            legIKTargetPairs.Add(pair);
        }

        legsController.Init(legIKTargetPairs.ToArray(), maxLegDistance,
            minLegDistance, legsRaiseHeight, timeToMoveLeg);
        legs.transform.localPosition = Vector3.zero;
        _legTargets = legTargets.transform;
    }

    private SpiderLegIK CreateLeg(string suffix = "")
    {
        // The position is to be set from the calling method

        GameObject upperLeg = new GameObject($"UpperLeg {suffix}");
        SpiderLegIK legIK = upperLeg.AddComponent<SpiderLegIK>();

        GameObject upperLegMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        upperLegMesh.name = "UpperLegMesh";
        upperLegMesh.transform.parent = upperLeg.transform;
        upperLegMesh.transform.localScale = new Vector3(legsWidth, legsWidth, upperLegLength);
        upperLegMesh.transform.localPosition = new Vector3(0, 0, upperLegLength / 2);

        // Middle leg
        GameObject middleLeg = new GameObject($"MiddleLeg {suffix}");
        middleLeg.transform.parent = upperLeg.transform;
        middleLeg.transform.localPosition = new Vector3(0, 0, upperLegLength / 2);

        GameObject middleLegMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        middleLegMesh.name = "MiddleLegMesh";
        middleLegMesh.transform.parent = middleLeg.transform;
        middleLegMesh.transform.localScale = new Vector3(legsWidth, legsWidth, middleLegLength);
        middleLegMesh.transform.localPosition = new Vector3(0, 0, middleLegLength / 2);

        // Lower leg
        GameObject lowerLeg = new GameObject($"LowerLeg {suffix}");
        lowerLeg.transform.parent = middleLeg.transform;
        upperLeg.transform.localPosition = new Vector3(0, 0, middleLegLength / 2);

        GameObject lowerLegMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lowerLegMesh.name = "LowerLegMesh";
        lowerLegMesh.transform.parent = lowerLeg.transform;
        lowerLegMesh.transform.localScale = new Vector3(legsWidth, legsWidth, lowerLegLength);
        lowerLegMesh.transform.localPosition = new Vector3(0, 0, lowerLegLength / 2);

        legIK.Init(
            upperLeg.transform, middleLeg.transform, lowerLeg.transform,
            upperLegLength, middleLegLength, lowerLegLength,
            ikDelta, ikIterations
        );

        return legIK;
    }

    private void OnDrawGizmos()
    {
        if (_legTargets == null)
        {
            return;
        }
        
        Gizmos.color = new Color(0f, 0.7f, 0.7f, 1f);

        foreach (Transform legTarget in _legTargets)
        {
            Gizmos.DrawWireSphere(legTarget.position, 0.5f);
        }
    }
}