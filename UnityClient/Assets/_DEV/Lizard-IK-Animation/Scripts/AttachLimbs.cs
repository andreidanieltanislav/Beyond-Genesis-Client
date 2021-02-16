using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachLimbs : MonoBehaviour
{
    [SerializeField] private GameObject rightArm;
    [SerializeField] private GameObject leftArm;
    [SerializeField] private GameObject rightLeg;
    [SerializeField] private GameObject leftLeg;
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject tail;

    private Transform _rightArmOrigin;
    private Transform _leftArmOrigin;
    private Transform _rightLegOrigin;
    private Transform _leftLegOrigin;
    private Transform _headOrigin;
    private Transform _tailOrigin;

    private void Start()
    {
        _rightArmOrigin = transform.Find("RightArmOrigin");
        _leftArmOrigin = transform.Find("LeftArmOrigin");

        _rightLegOrigin = transform.Find("RightLegOrigin");
        _leftLegOrigin = transform.Find("LeftLegOrigin");

        _headOrigin = transform.Find("HeadOrigin");
        _tailOrigin = transform.Find("TailOrigin");
    }

    void LateUpdate()
    {
        rightArm.transform.position = _rightArmOrigin.position;
        leftArm.transform.position = _leftArmOrigin.position;

        rightLeg.transform.position = _rightLegOrigin.position;
        leftLeg.transform.position = _leftLegOrigin.position;

        head.transform.position = _headOrigin.position;
        tail.transform.position = _tailOrigin.position;
    }
}
