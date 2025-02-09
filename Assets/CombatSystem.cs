using System.Collections;
using Dreamteck.Splines;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] public static SplineProjector _splineProjector;
    [SerializeField] private ColumnController _columnPrefab;

    bool canAttack = true;

    ColumnController _instance;

    private void Awake() {
        _splineProjector = GetComponent<SplineProjector>();
        _instance = Instantiate(_columnPrefab, new Vector3(0,-100,0), Quaternion.identity);
    }

    private Vector2 rightStickInput;

    private void Start() {
        InputManager.Instance.OnRightStick += (input) => rightStickInput = input;

        // _columnPrefab.SetSpline(_splineProjector.spline);

        StartCoroutine(GenerateColumn());
    }

    IEnumerator AttackTimer(){
        canAttack = false;
        yield return new WaitForSeconds(1);
        canAttack = true;
    }

    IEnumerator GenerateColumn(){
        while(true){
            yield return new WaitUntil(() => rightStickInput != Vector2.zero && canAttack);

            if(rightStickInput.x == 1){
                StartCoroutine(_instance.GenerateColumn(transform.position - _splineProjector.result.forward * 1.5f, Quaternion.LookRotation(_splineProjector.result.up, _splineProjector.result.forward), ColumnDirection.Right));
            } else if(rightStickInput.x == -1){
                StartCoroutine(_instance.GenerateColumn(transform.position + _splineProjector.result.forward * 1.5f, Quaternion.LookRotation(_splineProjector.result.up, -_splineProjector.result.forward), ColumnDirection.Left));
            } else if(rightStickInput.y == 1){
                StartCoroutine(_instance.GenerateColumn(transform.position - _splineProjector.result.up * 1.5f, Quaternion.LookRotation(-_splineProjector.result.forward, -_splineProjector.result.up), ColumnDirection.Down));
            } else if(rightStickInput.y == -1){
                StartCoroutine(_instance.GenerateColumn(transform.position + _splineProjector.result.up * 1.5f, Quaternion.LookRotation(_splineProjector.result.forward, _splineProjector.result.up), ColumnDirection.Up));
            }

            StartCoroutine(AttackTimer());
        }
    }
}
