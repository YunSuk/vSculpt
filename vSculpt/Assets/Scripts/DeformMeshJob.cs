using UnityEngine;
using System;
public class DeformMeshJob : ThreadedJob
{
    public float accuracy = 0.1f;

    private Vector3[] meshPoints, contactPoints, normals;
    private CallBack DeformMeshCallBack;

    public DeformMeshJob(Vector3[] meshPoints, Vector3[] contactPoints, Vector3[] normals, CallBack DeformMeshCallBack)
    {
        this.meshPoints = meshPoints;
        this.contactPoints = contactPoints;
        this.normals = normals;
        this.DeformMeshCallBack = DeformMeshCallBack;
    }

    protected override void ThreadFunction()
    {
        Debug.Log("ThreadFunction Start");
        float accuracySquare = accuracy * accuracy;
        for (int i = 0; i < meshPoints.Length; i++)
        {
            for (int j = 0; j < contactPoints.Length; j++)
            {
                while ((meshPoints[i] - contactPoints[j]).sqrMagnitude < accuracySquare)
                {
                    meshPoints[i] += normals[j] * accuracy;
                    //Debug.Log("new mesh point: " + meshPoints[i]);
                }
                
            }
        }
        Debug.Log("ThreadFunction End");
    }
    protected override void OnFinished()
    {
        Debug.Log("OnFinished!");
        DeformMeshCallBack(meshPoints);
    }
}