using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void CallBack(Vector3[] vecs);

public class MeshManager : MonoBehaviour {


    private Dictionary<Vector3, int> pointToMeshPointIndex = new Dictionary<Vector3, int>();
    private Dictionary<int, int> HullIndexToMeshIndex = new Dictionary<int, int>();

    private Mesh meshCopy;
    private Vector3[] meshPoints;
    private bool isBusy = false;
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
	}
	
	// Update is called once per frame
	void Update () {
        if (updateMesh)
        {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.vertices = meshPoints;
            mesh.RecalculateNormals();
            GetComponent<MeshCollider>().sharedMesh = mesh;
            updateMesh = false;
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        Vector3[] points = new Vector3[collision.contacts.Length];
        Vector3[] normals = new Vector3[points.Length];
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < points.Length; i++)
        {

            points[i] = transform.InverseTransformPoint(contactPoints[i].point);
            normals[i] = transform.InverseTransformDirection(contactPoints[i].normal) / transform.localScale.x * 0.001f;

            //Debug.DrawRay(contact.point, contact.normal, Color.red);
            //print("Enter: " + contact.point.ToString("F6"));
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
        foreach (ContactPoint contact in collision.contacts)
        {
            //Debug.DrawRay(contact.point, contact.normal, Color.red);
            //print("Exit: " + contact.point.ToString("F6"));
        }
    }



    void DeformMesh(Vector3[] points, Vector3[] normals)
    {
        if (isBusy)
        {
            return;
        }

        isBusy = true;
        print("DeformMesh!");

        DeformMeshJob job = new DeformMeshJob(meshPoints, points, normals, DeformMeshCallBack);
        threadJobs.Add(job);
        job.Start();
    }

    public void DeformMeshCallBack(Vector3[] vecs)
    {
        print("DeformMeshCallBack!");
        meshPoints = vecs;
        isBusy = false;
        updateMesh = true;
    }
}
