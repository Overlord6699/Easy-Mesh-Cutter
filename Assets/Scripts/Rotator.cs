using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 Axis = Vector3.up;
    public Vector3 Pivot = Vector3.zero;
    public float Speed = 1;

    private float angle;

    private void Update()
    {
        angle += Speed * Time.smoothDeltaTime;

        var transformLocalRotation = Quaternion.AngleAxis(angle, Axis);
        transform.localRotation = transformLocalRotation;
        transform.localPosition = transformLocalRotation * Pivot;
    }
}