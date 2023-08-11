using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjOrientation : MonoBehaviour
{
    public bool showDebug;

    /// <summary>
    /// Returns global direcion that this objs forward is facing.
    /// </summary>
    public Direction GetCurrentDirection => GetObjectDirection();

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        Upright
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (showDebug)
            Debug.Log(gameObject.name + " " + GetObjectDirection());
    }
#endif

    private Direction GetObjectDirection()
    {
        if (Vector3.Dot(transform.forward, Vector3.down) > 0.5f)
        {
            return Direction.Down;
        }
        else if (Vector3.Dot(transform.forward, Vector3.up) > 0.5f)
        {
            return Direction.Up;
        }
        else if (Vector3.Dot(transform.right, Vector3.down) > 0.5f)
        {
            return Direction.Right;
        }
        else if (Vector3.Dot(-transform.right, Vector3.down) > 0.5f)
        {
            return Direction.Left;
        }
        else
        {
            return Direction.Upright;
        }
    }

    public bool CompareCurrentDirection(Direction direction)
    {
        if (GetObjectDirection() == direction) return true;

        return false;
    }
}
