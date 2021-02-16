using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private LimbController rArm;
    [SerializeField] private LimbController lArm;
    [SerializeField] private LimbController rLeg;
    [SerializeField] private LimbController lLeg;
    [SerializeField] private TailController tail;

    [Header("Body Properties")]
    [SerializeField] private float height = 1.5f;
    [SerializeField, Range(0f, 1f)] private float ratio = 1 / 3f;
    [SerializeField] private float tailStiffness = 5f;
    private float _heightPercent = 0f;
    private float _tailPercent = 0f;

    [Header("Movement Properties")]
    [SerializeField] public float limbOffset = 0.5f;
    [SerializeField] public float stepHeight = 0.2f;
    [SerializeField] public float distanceThreshold = 0.7f;
    [SerializeField] public float walkingSpeed = 10f;
    [SerializeField] public float stepOffset = 0.5f;


    void Start()
    {
        rArm.Init(this, true, -1);
        lArm.Init(this, false, 1);
        rLeg.Init(this, false, -1);
        lLeg.Init(this, true, 1);

        StartCoroutine(nameof(MoveCoroutine));
    }

    void Update()
    {
        ApplyAveragePosition();
        ApplyAverageRotation();
    }

    private void ApplyAverageRotation()
    {
        float pitch = FrontToBackRotation();
        float roll = LeftToRightRotation();

        Quaternion desiredRotation = Quaternion.AngleAxis(pitch, transform.forward) 
                                            * Quaternion.AngleAxis(roll, transform.right);
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * 5f);

        // Rotate tail
        tail.SetForce(pitch * Vector3.up + roll * Vector3.right);
        _tailPercent += Time.deltaTime * 3f;
        tail.AddForce(Mathf.Sin(_tailPercent) * tailStiffness * Vector3.right);
    }

    private float FrontToBackRotation()
    {
        float heightFront = (rArm.GetHeight() + lArm.GetHeight()) / 2f;
        float heightBack = (rLeg.GetHeight() + lLeg.GetHeight()) / 2f;
        float heightDifference = (heightFront - heightBack);

        return Mathf.Atan2(heightDifference, 1) * Mathf.Rad2Deg;
    }

    private float LeftToRightRotation()
    {
        float heightLeft = (lArm.GetHeight() + lLeg.GetHeight()) / 2f;
        float heightRight = (rArm.GetHeight() + rLeg.GetHeight()) / 2f;
        float heightDifference = (heightLeft - heightRight);

        return -Mathf.Atan2(heightDifference, 1) * Mathf.Rad2Deg;
    }

    private void ApplyAveragePosition()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDirection = v * transform.right - h * transform.forward;

        Vector3 avgPosition = lArm.GetPosition() + lLeg.GetPosition() +
                                        rArm.GetPosition() + rLeg.GetPosition();
        avgPosition /= 4;
        if (moveDirection.magnitude == 0f)
            _heightPercent += Time.deltaTime * 5f;
        else
            _heightPercent = 0f;

        avgPosition += Vector3.up * GetHeight();

        transform.position = Vector3.Lerp(transform.position, avgPosition, Time.deltaTime * 5f);
        transform.position += moveDirection * Time.deltaTime * 4f;
    }

    float GetHeight()
    {
        return (((3f + Mathf.Sin(_heightPercent)) / 4f - 1) * ratio + 1) * height;
    }

    IEnumerator MoveCoroutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => lArm.ShouldMove() && rLeg.ShouldMove());
            yield return new WaitUntil(() => rArm.Grounded && lLeg.Grounded);
            lArm.UpdatePosition();
            rLeg.UpdatePosition();
            yield return new WaitForSeconds(Time.fixedDeltaTime * 2f);

            yield return new WaitUntil(() => rArm.ShouldMove() && lLeg.ShouldMove());
            yield return new WaitUntil(() => lArm.Grounded && rLeg.Grounded);
            rArm.UpdatePosition();
            lLeg.UpdatePosition();
            yield return new WaitForSeconds(Time.fixedDeltaTime * 2f);
        }
    }
}
