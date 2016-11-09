﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void CallBack(Vector3[] vecs);

public class MeshManager : MonoBehaviour {

    public GameObject colliderObject;

    private Dictionary<Vector3, int> pointToMeshPointIndex = new Dictionary<Vector3, int>();
    private Dictionary<int, int> HullIndexToMeshIndex = new Dictionary<int, int>();


    private Vector3[] meshToWorld;
    private Mesh colliderMesh;
    private Vector3[] colliderMeshPoints;
    private Vector3[] colliderMeshNormals;
    private Mesh meshCopy;
    private Vector3[] meshPoints;
    public bool isBusy = false;
    private bool updateMesh = false;
    private List<DeformMeshJob> threadJobs = new List<DeformMeshJob>();
    /*
    private static MeshManager _Instance = null;

    public static MeshManager Instance {
        get
        {
            if (_Instance == null)
            {
                _Instance = new MeshManager();
            }
            return _Instance;
        }
    }
    */
    // Use this for initialization
    void Start () {
        MeshFilter filter = GetComponent<MeshFilter>();
        meshCopy = Mesh.Instantiate(filter.sharedMesh);
        filter.sharedMesh = meshCopy;
        meshPoints = filter.sharedMesh.vertices;

        // ASSUMING THAT MESH IS NOT MOVED (TRANSFORM IS NOT AFFECTED)
        meshToWorld = new Vector3[meshPoints.Length];
        for (int i = 0; i < meshPoints.Length; i++)
        {
            meshToWorld[i] = transform.TransformPoint(meshPoints[i]);
        }

        filter = colliderObject.GetComponent<MeshFilter>();
        colliderMesh = filter.sharedMesh;
        colliderMeshPoints = filter.sharedMesh.vertices;
        colliderMeshNormals = filter.sharedMesh.normals;
    }
	
	// Update is called once per frame
	void Update () {
        if (updateMesh)
        {
            for (int i = 0; i < meshPoints.Length; i++)
            {
                meshPoints[i] = transform.InverseTransformPoint(meshToWorld[i]);
            }
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.vertices = meshPoints;
            mesh.RecalculateNormals();
            GetComponent<MeshCollider>().sharedMesh = mesh;
            updateMesh = false;
            isBusy = false;
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        
        Vector3[] points = new Vector3[colliderMeshPoints.Length];
        Vector3[] normals = new Vector3[colliderMeshNormals.Length];
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < points.Length; i++)
        {

            points[i] = colliderObject.transform.TransformPoint(colliderMeshPoints[i]);
            normals[i] = colliderObject.transform.TransformDirection(colliderMeshNormals[i]) / transform.localScale.x * 0.001f;

            //Debug.DrawRay(contact.point, contact.normal, Color.red);
            //print("Enter: " + points[i].ToString("F6"));
        }

        DeformMesh(points, normals);
    }

    void OnCollisionStay(Collision collision)
    {
        OnCollisionEnter(collision);
        /*
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.red);
            //print("Stay: " + contact.point.ToString("F6"));
        }
        */
    }

    void OnCollisionExit(Collision collision)
    {
        /*
        foreach (ContactPoint contact in collision.contacts)
        {
            //Debug.DrawRay(contact.point, contact.normal, Color.red);
            //print("Exit: " + contact.point.ToString("F6"));
        }
        */
    }



    void DeformMesh(Vector3[] points, Vector3[] normals)
    {
        if (isBusy)
        {
            return;
        }

        isBusy = true;
        print("DeformMesh!");

        
        DeformMeshJob job = new DeformMeshJob(meshToWorld, points, normals, DeformMeshCallBack);
        threadJobs.Add(job);
        job.Start();
    }

    public void DeformMeshCallBack(Vector3[] vecs)
    {
        print("DeformMeshCallBack!");
        
        meshToWorld = vecs;
        updateMesh = true;
    }
}
