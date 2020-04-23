using UnityEngine;
using UnityEditor;

[AddComponentMenu("Freedom Engine/Objects/Moving Platform")]
public class MovingPlatform : MonoBehaviour
{
    public enum MoveType
    {
        Vertical,
        Horizontal,
        Diagonal,
        Rotate
    }

    public MoveType moveType;
    public float amplitude;
    public float period;

    private Vector3 center;

    private void Start()
    {
        center = transform.position;
    }

    private void Update()
    {
        var time = Time.time;
        var position = transform.position;

        if (moveType == MoveType.Vertical || moveType == MoveType.Diagonal)
        {
            position.y = center.y + amplitude * Mathf.Sin(period * time);

            if (moveType == MoveType.Diagonal)
            {
                position.x = center.x + amplitude * Mathf.Sin(period * time);
            }
        }
        else if (moveType == MoveType.Horizontal)
        {
            position.x = center.x + amplitude * Mathf.Sin(period * time);
        }
        else if (moveType == MoveType.Rotate)
        {
            position.x = center.x + amplitude * Mathf.Cos(period * time);
            position.y = center.y + amplitude * Mathf.Sin(period * time);
        }

        transform.position = position;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var center = Application.isPlaying ? this.center : transform.position;

        switch(moveType)
        {
            case MoveType.Rotate:
                Handles.DrawWireDisc(center, Vector3.forward, amplitude);
                break;
            case MoveType.Horizontal:
                Gizmos.DrawLine(center + Vector3.left * amplitude, center + Vector3.right * amplitude);
                break;
            case MoveType.Vertical:
                Gizmos.DrawLine(center + Vector3.down * amplitude, center + Vector3.up * amplitude);
                break;
            case MoveType.Diagonal:
                Gizmos.DrawLine(center + Vector3.left * amplitude + Vector3.down * amplitude, center + Vector3.up * amplitude + Vector3.right * amplitude);
                break;
        }
        
        Gizmos.DrawSphere(center, 0.1f);
    }
#endif
}
