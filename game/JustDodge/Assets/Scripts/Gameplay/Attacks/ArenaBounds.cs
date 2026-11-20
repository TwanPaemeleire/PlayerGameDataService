using UnityEngine;

public enum ArenaSide { Top, Right, Bottom, Left }

public class ArenaBounds : MonoBehaviour
{
    [SerializeField] private Vector2 _size = new Vector2(10f, 6f);

    public Vector2 Size => _size;
    public Vector2 Center => transform.position;
    public Vector2 Min => Center - _size / 2f;   // bottom-left corner
    public Vector2 Max => Center + _size / 2f;   // top-right corner
    public float Perimeter => 2f * (_size.x + _size.y);

    public Vector2 GetSideMidpoint(ArenaSide side)
    {
        return side switch
        {
            ArenaSide.Top    => new Vector2(Center.x, Max.y),
            ArenaSide.Right  => new Vector2(Max.x, Center.y),
            ArenaSide.Bottom => new Vector2(Center.x, Min.y),
            ArenaSide.Left   => new Vector2(Min.x, Center.y),
            _ => Center
        };
    }

    public Vector2 GetInwardDirection(ArenaSide side)
    {
        return side switch
        {
            ArenaSide.Top    => Vector2.down,
            ArenaSide.Right  => Vector2.left,
            ArenaSide.Bottom => Vector2.up,
            ArenaSide.Left   => Vector2.right,
            _ => Vector2.zero
        };
    }

    public Vector2 GetPointOnSide(ArenaSide side, float t)
    {
        GetSideEnds(side, out Vector2 start, out Vector2 end);
        return Vector2.Lerp(start, end, Mathf.Clamp01(t));
    }

    /// <summary>
    /// Walks clockwise around the whole rectangle, starting at the top-left corner.
    /// t = 0 and t = 1 are both the top-left corner; values outside 0..1 wrap around,
    /// so you can keep increasing t to loop forever.
    /// </summary>
    public Vector2 GetPointOnPerimeter(float t)
    {
        return GetPointOnPerimeter(t, out _);
    }

    /// <summary>Same as above, but also tells you which side the point is on.</summary>
    public Vector2 GetPointOnPerimeter(float t, out ArenaSide side)
    {
        float distance = Mathf.Repeat(t, 1f) * Perimeter;

        ArenaSide[] order = { ArenaSide.Top, ArenaSide.Right, ArenaSide.Bottom, ArenaSide.Left };
        foreach (ArenaSide current in order)
        {
            float length = (current == ArenaSide.Top || current == ArenaSide.Bottom) ? _size.x : _size.y;
            if (distance <= length || current == ArenaSide.Left)
            {
                side = current;
                float along = length > 0f ? distance / length : 0f;
                return GetPointOnSide(current, along);
            }
            distance -= length;
        }

        side = ArenaSide.Top;
        return Min;
    }

    public bool Contains(Vector2 point)
    {
        return point.x >= Min.x && point.x <= Max.x && point.y >= Min.y && point.y <= Max.y;
    }

    public Vector2 ClampPoint(Vector2 point, float padding)
    {
        return new Vector2(
            Mathf.Clamp(point.x, Min.x + padding, Max.x - padding),
            Mathf.Clamp(point.y, Min.y + padding, Max.y - padding));
    }

    private void GetSideEnds(ArenaSide side, out Vector2 start, out Vector2 end)
    {
        Vector2 topLeft = new Vector2(Min.x, Max.y);
        Vector2 topRight = Max;
        Vector2 bottomRight = new Vector2(Max.x, Min.y);
        Vector2 bottomLeft = Min;

        switch (side)
        {
            case ArenaSide.Top:
                start = topLeft;
                end = topRight;
                break;
            case ArenaSide.Right:
                start = topRight;    
                end = bottomRight; 
                break;
            case ArenaSide.Bottom: 
                start = bottomRight; 
                end = bottomLeft;  
                break;
            default:               
                start = bottomLeft; 
                end = topLeft;     
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, _size);

        Gizmos.color = Color.yellow;
        foreach (ArenaSide side in System.Enum.GetValues(typeof(ArenaSide)))
        {
            Gizmos.DrawSphere(GetSideMidpoint(side), 0.08f);
        }
    }
}
