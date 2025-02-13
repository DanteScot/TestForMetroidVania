using System.Collections.Generic;
using UnityEngine;


public enum ColumnPartition{
    Top,
    Middle,
    Bottom
}
public class ColumnComponent : MonoBehaviour
{
    [SerializeField] private ColumnPartition _columnPatition;
    [SerializeField] private ColumnController _columnController;

    private List<string> hittedObjects;

    private void Start() {
        _columnController = GetComponentInParent<ColumnController>();
        hittedObjects = new List<string>();
    }

    private void OnCollisionEnter(Collision other) {
        if(!_columnController.isGenerated && _columnPatition != ColumnPartition.Top) return;
        if(other.gameObject.CompareTag("Column")) return;
        // if(hittedObjects.Contains(other.gameObject.name)) return;


        if(other.gameObject.CompareTag("Player")){
            _columnController.OnColumnHit(_columnPatition, other);
        }
    }

    void OnBecameInvisible()
    {
        _columnController.BecameInvisible();
    }

    void OnBecameVisible()
    {
        _columnController.BecameVisible();
    }
}
