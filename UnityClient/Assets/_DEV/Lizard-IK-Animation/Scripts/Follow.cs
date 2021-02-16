using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private Transform target;

    void Start()
    {
        transform.rotation = Quaternion.Euler(Vector3.forward * -30) * Quaternion.Euler(Vector3.up * 90);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
    }
}
