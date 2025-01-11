using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeItem : MonoBehaviour
{
    public Transform startNode;
    public Transform endNode;

    public RopeDir ropeDir;
    public float distance;

    void Start()
    {
        CalculateDistance();
    }
    private void CalculateDistance()
    {
        if (startNode == null || endNode == null)
            return;

        switch (ropeDir)
        {
            case RopeDir.X:
                distance = Mathf.Abs(endNode.position.x - startNode.position.x);
                break;
            case RopeDir.Y:
                distance = Mathf.Abs(endNode.position.y - startNode.position.y);
                break;
            case RopeDir.Z:
                distance = Mathf.Abs(endNode.position.z - startNode.position.z);
                break;
        }
    }

    // 获取起点在指定方向上的坐标
    public float GetStartPosition()
    {
        if (startNode == null)
            return 0;

        switch (ropeDir)
        {
            case RopeDir.X:
                return startNode.position.x;
            case RopeDir.Y:
                return startNode.position.y;
            case RopeDir.Z:
                return startNode.position.z;
            default:
                return 0;
        }
    }

    // 获取终点在指定方向上的坐标
    public float GetEndPosition()
    {
        if (endNode == null)
            return 0;

        switch (ropeDir)
        {
            case RopeDir.X:
                return endNode.position.x;
            case RopeDir.Y:
                return endNode.position.y;
            case RopeDir.Z:
                return endNode.position.z;
            default:
                return 0;
        }
    }
}

public enum RopeDir
{
    X,
    Y,
    Z
}