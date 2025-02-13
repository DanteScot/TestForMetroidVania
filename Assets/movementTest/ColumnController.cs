using System.Collections;
using System.Threading.Tasks;
using Dreamteck.Splines;
using UnityEngine;

public enum ColumnDirection{
    Up,
    Down,
    Left,
    Right
}

public class ColumnController : MonoBehaviour
{
    [SerializeField] private CommonValues commonValues;
    [SerializeField] private float _generationSpeed;
    [SerializeField] private float _lenght;
    [SerializeField] private Rigidbody _gravityController;
    [SerializeField] SplineProjector _splineProjector;

    ColumnDirection _columnDirection;
    [HideInInspector] public bool isGenerated = false;

    public bool IsDestroyed { get; private set; }
    int visiblePartitions;

    void Awake()
    {
        IsDestroyed = true;
        visiblePartitions = 0;
    }

    public IEnumerator GenerateColumn(Vector3 position, Quaternion rotation, ColumnDirection columnDirection){
        Reset();
        IsDestroyed = false;

        transform.parent.SetPositionAndRotation(position, rotation);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _gravityController.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        _columnDirection = columnDirection;

        while (true){
            yield return new WaitForFixedUpdate();

            if (IsDestroyed) yield break;

            transform.localScale += new Vector3(0, _generationSpeed * Time.deltaTime, 0);
            
            if(transform.localScale.y >= _lenght){
                transform.localScale = new Vector3(transform.localScale.x, _lenght, transform.localScale.z);
                isGenerated = true;
                StartCoroutine(EnableGravity());
                break;
            }
        }
    }

    IEnumerator EnableGravity(){
        yield return new WaitForSeconds(1);
        _gravityController.useGravity = true;
    }

    public void OnColumnHit(ColumnPartition columnType, Collision other){
        switch(_columnDirection){
            case ColumnDirection.Up:
                other.gameObject.GetComponent<Rigidbody>().AddForce(_splineProjector.result.up * 25, ForceMode.Impulse);
                break;
            case ColumnDirection.Down:
                other.gameObject.GetComponent<Rigidbody>().AddForce(-_splineProjector.result.up * 25, ForceMode.Impulse);
                break;
            case ColumnDirection.Left:
                other.gameObject.GetComponent<Rigidbody>().AddForce(-_splineProjector.result.forward * 50, ForceMode.Impulse);
                break;
            case ColumnDirection.Right:
                other.gameObject.GetComponent<Rigidbody>().AddForce(_splineProjector.result.forward * 50, ForceMode.Impulse);
                break;
        }
    }

    public void Reset(){
        StopAllCoroutines();
        isGenerated = false;
        IsDestroyed = true;
        visiblePartitions = 0;

        transform.parent.position = new Vector3(0, -100, 0);
        
        _gravityController.useGravity = false;
        _gravityController.linearVelocity = Vector3.zero;
        _gravityController.angularVelocity = Vector3.zero;

        transform.localScale = new Vector3(transform.localScale.x, 0.1f, transform.localScale.z);
        _splineProjector.spline = commonValues.currentSpline;
    }

    public void BecameInvisible(){
        visiblePartitions--;
        if(visiblePartitions <= 0){
            Debug.Log("Destroying column");
            Reset();
        }
    }

    public void BecameVisible(){
        visiblePartitions++;
    }

    private void OnDestroy() {
        IsDestroyed = true;
        StopAllCoroutines();
    }
}