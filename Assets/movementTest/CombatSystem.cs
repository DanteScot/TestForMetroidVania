using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] CommonValues commonValues;
    [SerializeField] private GameObject _columnPrefab;

    private SplineProjector _splineProjector;

    bool canAttack = true;

    [SerializeField] int _maxColumns = 5;
    ColumnController[] _instances;

    private void Awake() {
        _splineProjector = GetComponent<SplineProjector>();

        _instances = new ColumnController[_maxColumns];

        for (int i = 0; i < _maxColumns; i++)
            _instances[i] = Instantiate(_columnPrefab, new Vector3(0,-100,0), Quaternion.identity).GetComponentInChildren<ColumnController>();

        commonValues.currentSpline = _splineProjector.spline;
    }

    private Vector2 rightStickInput;

    private void Start() {
        InputManager.Instance.OnRightStick += (input) => rightStickInput = input;

        StartCoroutine(GenerateColumn());
    }

    IEnumerator AttackTimer(){
        canAttack = false;
        yield return new WaitForSeconds(.5f);
        canAttack = true;
    }

    IEnumerator GenerateColumn(){
        while(true){
            yield return new WaitUntil(() => rightStickInput != Vector2.zero && canAttack);

            for (int i = 0; i < _maxColumns; i++){
                if(!_instances[i].IsDestroyed) continue;

                if(rightStickInput.x == 1){
                    StartCoroutine(_instances[i].GenerateColumn(transform.position - _splineProjector.result.forward * 1.5f, Quaternion.LookRotation(_splineProjector.result.up, _splineProjector.result.forward), ColumnDirection.Right));
                } else if(rightStickInput.x == -1){
                    StartCoroutine(_instances[i].GenerateColumn(transform.position + _splineProjector.result.forward * 1.5f, Quaternion.LookRotation(_splineProjector.result.up, -_splineProjector.result.forward), ColumnDirection.Left));
                } else if(rightStickInput.y == 1){
                    StartCoroutine(_instances[i].GenerateColumn(transform.position - _splineProjector.result.up * 1.5f, Quaternion.LookRotation(-_splineProjector.result.forward, -_splineProjector.result.up), ColumnDirection.Down));
                } else if(rightStickInput.y == -1){
                    StartCoroutine(_instances[i].GenerateColumn(transform.position + _splineProjector.result.up * 1.5f, Quaternion.LookRotation(_splineProjector.result.forward, _splineProjector.result.up), ColumnDirection.Up));
                }

                StartCoroutine(AttackTimer());

                break;
            }
        }
    }
}
