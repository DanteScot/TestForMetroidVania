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
    [SerializeField] private float _generationSpeed;
    [SerializeField] private float _lenght;

    ColumnDirection _columnDirection;
    public bool isGenerated = false;

    [SerializeField] SplineProjector _splineProjector;

    private void OnValidate() {
        _splineProjector = GetComponent<SplineProjector>();
    }

    public void SetSpline(SplineComputer spline){
        _splineProjector.spline = spline;
    }
    

    public IEnumerator GenerateColumn(Vector3 position, Quaternion rotation, ColumnDirection columnDirection){
        Reset();
        transform.SetPositionAndRotation(position, rotation);
        _columnDirection = columnDirection;

        while (true){
            yield return new WaitForFixedUpdate();

            transform.localScale += new Vector3(0, _generationSpeed * Time.deltaTime, 0);
            
            if(transform.localScale.y >= _lenght){
                transform.localScale = new Vector3(transform.localScale.x, _lenght, transform.localScale.z);
                isGenerated = true;
                break;
            }
        }
    }

    public void OnColumnHit(ColumnPartition columnType, Collision other){
        Debug.Log(_columnDirection.ToString());
        switch(_columnDirection){
            case ColumnDirection.Up:
                Debug.Log("Up");
                other.gameObject.GetComponent<Rigidbody>().AddForce(_splineProjector.result.up * 25, ForceMode.Impulse);
                break;
            case ColumnDirection.Down:
                Debug.Log("Down");
                other.gameObject.GetComponent<Rigidbody>().AddForce(-_splineProjector.result.up * 25, ForceMode.Impulse);
                break;
            case ColumnDirection.Left:
                Debug.Log("Left");
                other.gameObject.GetComponent<Rigidbody>().AddForce(-_splineProjector.result.forward * 50, ForceMode.Impulse);
                break;
            case ColumnDirection.Right:
                Debug.Log("Right");
                other.gameObject.GetComponent<Rigidbody>().AddForce(_splineProjector.result.forward * 50, ForceMode.Impulse);
                break;
        }
    }

    private void Reset(){
        isGenerated = false;
        transform.localScale = new Vector3(transform.localScale.x, 0.1f, transform.localScale.z);
        _splineProjector.spline = CombatSystem._splineProjector.spline;
    }

}