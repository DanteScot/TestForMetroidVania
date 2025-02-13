using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

public class ColumnGravityController : MonoBehaviour
{
    [SerializeField] CommonValues commonValues;
    [SerializeField] Transform target;
    Rigidbody rb;

    WaitUntil waitUntil;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        waitUntil = new WaitUntil(() => rb.useGravity);

        GetComponent<SplineProjector>().spline = commonValues.currentSpline;

        StartCoroutine(EnableGravity());
    }

    IEnumerator EnableGravity(){
        while(true){
            yield return waitUntil;

            target.SetPositionAndRotation(transform.position, transform.rotation);
        }
    }


    void OnCollisionStay(Collision collision)
    {
        if(rb.useGravity) return;

        StopAllCoroutines();
        target.GetComponent<ColumnController>().Reset();
    }
}
