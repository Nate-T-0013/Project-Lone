using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float ySmoothTime = 0.15f;
    public float xSmoothTime = 0.15f;

    private float yVelocity;
    private float xVelocity;

    private Vector2 minXMaxX = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
    private Vector2 minYMaxY = new Vector2(float.NegativeInfinity, float.PositiveInfinity);

    private void LateUpdate()
    {
        if (!target) return;

        float targetY = target.position.y;
        float targetX = target.position.x;

        float yPos = Mathf.SmoothDamp(transform.position.y, targetY, ref yVelocity, ySmoothTime);
        float xPos = Mathf.SmoothDamp(transform.position.x, targetX, ref xVelocity, xSmoothTime);

        xPos = Mathf.Clamp(xPos, minXMaxX.x, minXMaxX.y);
        yPos = Mathf.Clamp(yPos, minYMaxY.x, minYMaxY.y);

        //make sure we have -z
        transform.position = new Vector3(xPos, yPos, -10);
    }

    public void clampCameraPosition(Vector2 minAndMaxX, Vector2 minAndMaxY)
    {
        minXMaxX = minAndMaxX;
        minYMaxY = minAndMaxY;
    }

    public void setVelocity(Vector2 vec)
    {
        xVelocity = vec.x;
        yVelocity = vec.y;
    }
}

