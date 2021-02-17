using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailController : MonoBehaviour
{
    [SerializeField] private JiggleChain jiggleChain;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public void AddForce(Vector3 force) => jiggleChain.data.externalForce += force;
    public void SetForce(Vector3 force) => jiggleChain.data.externalForce = force;
}
