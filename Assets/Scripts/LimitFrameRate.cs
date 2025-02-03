using UnityEngine;

public class LimitFrameRate : MonoBehaviour
{
    [SerializeField] private int frameRate = 60;
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
