using UnityEngine;
using System;
using System.Collections.Generic;
public class DeformMeshJob : ThreadedJob
{
    public float accuracy = 0.1f;

    private List<Vector3> meshPoints, meshNormals;
    private List<int> meshTriangles;
    private List<Vector4> meshTangents;
    private List<Vector2> meshUVs;
    private Vector3[] contactPoints, contactNormals;
    
    private CallBack DeformMeshCallBack;

    public DeformMeshJob(Vector3[] meshPoints, int[] meshTriangles, Vector3[] meshNormals, Vector4[] meshTangents, 
        Vector2[] meshUVs, Vector3[] contactPoints, Vector3[] contactNormals, CallBack DeformMeshCallBack)
    {
        this.meshPoints = new List<Vector3>(meshPoints);
        this.meshTriangles = new List<int>(meshTriangles);
        this.meshNormals = new List<Vector3>(meshNormals);
        this.meshTangents = new List<Vector4>(meshTangents);
        this.meshUVs = new List<Vector2>(meshUVs);
        this.contactPoints = contactPoints;
        this.contactNormals = contactNormals;
        this.DeformMeshCallBack = DeformMeshCallBack;
    }

    protected override void ThreadFunction()
    {
        Debug.Log("ThreadFunction Start");
        float accuracySquare = accuracy * accuracy;

        // get triangles affected
        List<int> affectedTriangles = new List<int>();
        for (int i = 0; i < meshTriangles.Count; i+=3)
        {
            for (int j = 0; j < contactPoints.Length; j++)
            {
                if (IsTriangleIndexIntersected(i, contactPoints[j], accuracy))
                {
                    affectedTriangles.Add(i);
                }
            }
        }

        // break affected triangles if necessary
        HashSet<int> subdiviedAffectedVertexIndices = new HashSet<int>();
        foreach (int affectedTriangle in affectedTriangles)
        {
            subdiviedAffectedVertexIndices.UnionWith(BreakDownTriangle(affectedTriangle));
        }

        // modify points
        foreach (int i in subdiviedAffectedVertexIndices)
        {
            for (int j = 0; j < contactPoints.Length; j++)
            {
                //float mag = (meshPoints[i] - contactPoints[j]).sqrMagnitude;
                while ((meshPoints[i] - contactPoints[j]).sqrMagnitude < accuracySquare)
                {
                    meshPoints[i] += contactNormals[j] * accuracy;
                    //Debug.Log("new mesh point: " + meshPoints[i]);
                }
                
            }
        }
        Debug.Log("ThreadFunction End");
    }
    protected override void OnFinished()
    {
        Debug.Log("OnFinished!");
        DeformMeshCallBack(meshPoints.ToArray(), meshTriangles.ToArray(), meshNormals.ToArray(), 
            meshTangents.ToArray(), meshUVs.ToArray());
    }

    // Unity asset Meshinator's Hull.cs code
    private bool IsTriangleIndexIntersected(int triangleIndex, Vector3 impactPoint, float impactRadius)
    {
        // Make sure we've got a good triangle index
        if (triangleIndex % 3 != 0)
        {
            Debug.LogError("Invalid Triangle index: " + triangleIndex + "  Must be a multiple of 3!");
            return false;
        }

        // Get the vectors for our triangle
        Vector3 A = meshPoints[meshTriangles[triangleIndex]] - impactPoint;
        Vector3 B = meshPoints[meshTriangles[triangleIndex + 1]] - impactPoint;
        Vector3 C = meshPoints[meshTriangles[triangleIndex + 2]] - impactPoint;

        // Is the impact sphere outside the triangle plane?
        float rr = impactRadius * impactRadius;
        Vector3 V = Vector3.Cross(B - A, C - A);
        float d = Vector3.Dot(A, V);
        float e = Vector3.Dot(V, V);
        bool sep1 = d * d > rr * e;
        if (sep1)
            return false;

        // Is the impact sphere outside a triangle vertex?
        float aa = Vector3.Dot(A, A);
        float ab = Vector3.Dot(A, B);
        float ac = Vector3.Dot(A, C);
        float bb = Vector3.Dot(B, B);
        float bc = Vector3.Dot(B, C);
        float cc = Vector3.Dot(C, C);
        bool sep2 = (aa > rr) && (ab > aa) && (ac > aa);
        bool sep3 = (bb > rr) && (ab > bb) && (bc > bb);
        bool sep4 = (cc > rr) && (ac > cc) && (bc > cc);
        if (sep2 || sep3 || sep4)
            return false;

        // Is the impact sphere outside a triangle edge?
        Vector3 AB = B - A;
        Vector3 BC = C - B;
        Vector3 CA = A - C;
        float d1 = ab - aa;
        float d2 = bc - bb;
        float d3 = ac - cc;
        float e1 = Vector3.Dot(AB, AB);
        float e2 = Vector3.Dot(BC, BC);
        float e3 = Vector3.Dot(CA, CA);
        Vector3 Q1 = AB * e1 - d1 * AB;
        Vector3 Q2 = BC * e2 - d2 * BC;
        Vector3 Q3 = CA * e3 - d3 * CA;
        Vector3 QC = C * e1 - Q1;
        Vector3 QA = A * e2 - Q2;
        Vector3 QB = B * e3 - Q3;
        bool sep5 = (Vector3.Dot(Q1, Q1) > rr * e1 * e1) && (Vector3.Dot(Q1, QC) > 0);
        bool sep6 = (Vector3.Dot(Q2, Q2) > rr * e2 * e2) && (Vector3.Dot(Q2, QA) > 0);
        bool sep7 = (Vector3.Dot(Q3, Q3) > rr * e3 * e3) && (Vector3.Dot(Q3, QB) > 0);
        if (sep5 || sep6 || sep7)
            return false;

        // If we've gotten here, then this impact force DOES intersect this triangle.
        return true;
    }

    // Unity asset Meshinator's Hull.cs code
    private List<int> BreakDownTriangle(int triangleIndex)
    {
        List<int> newTriangleIndices = new List<int>();
        newTriangleIndices.Add(triangleIndex);

        // If we have 64988 vertices or more, we can't add any more or we risk going over the
        // 65000 limit, which causes problems for unity.
        if (meshPoints.Count > 64988)
            return newTriangleIndices;

        // Get the vertex indices and store them here
        int indexA = meshTriangles[triangleIndex];
        int indexB = meshTriangles[triangleIndex + 1];
        int indexC = meshTriangles[triangleIndex + 2];

        // Get the 3 vertices for this triangle
        Vector3 vertexA = meshPoints[indexA];
        Vector3 vertexB = meshPoints[indexB];
        Vector3 vertexC = meshPoints[indexC];

        // Find the center points of this triangle sides. We'll be adding these as a new vertices.
        Vector3 centerAB = (vertexA + vertexB) / 2f;
        Vector3 centerAC = (vertexA + vertexC) / 2f;
        Vector3 centerBC = (vertexB + vertexC) / 2f;

        float accuracySquare = accuracy * accuracy;

        // Check if triangle points satisfy accuracy requirement
        if (centerAB.sqrMagnitude < accuracySquare && centerAC.sqrMagnitude < accuracySquare 
            && centerBC.sqrMagnitude < accuracySquare)
        {
            return newTriangleIndices;
        }

        // Does not meet the requirement. Break down triangle!

        // Add new verticies
        meshPoints.Add(centerAB);
        meshPoints.Add(centerAC);
        meshPoints.Add(centerBC);

        int indexAB = meshPoints.Count - 3;
        int indexAC = meshPoints.Count - 2;
        int indexBC = meshPoints.Count - 1;

        // Adjust the old triangle to use one of the new vertices
        meshTriangles[indexB] = indexAB;
        meshTriangles[indexC] = indexAC;

        // Add 3 new vertices for the other triangles
        meshTriangles.Add(indexBC);
        meshTriangles.Add(indexC);
        meshTriangles.Add(indexAC);
        newTriangleIndices.Add(meshTriangles.Count - 3);

        // Add 3 new vertices for the other triangles
        meshTriangles.Add(indexB);
        meshTriangles.Add(indexBC);
        meshTriangles.Add(indexAB);
        newTriangleIndices.Add(meshTriangles.Count - 3);

        // Add 3 new vertices for the other triangles
        meshTriangles.Add(indexBC);
        meshTriangles.Add(indexAC);
        meshTriangles.Add(indexAB);
        newTriangleIndices.Add(meshTriangles.Count - 3);

        


        // Add new normals. These MUST be added in the same order as the vertices above!
        if (meshNormals.Count > 0)
        {
            Vector3 normalA = meshNormals[indexA];
            Vector3 normalB = meshNormals[indexB];
            Vector3 normalC = meshNormals[indexC];
            Vector3 normalAB = (normalA + normalB) / 2;
            Vector3 normalAC = (normalA + normalC) / 2;
            Vector3 normalBC = (normalB + normalC) / 2;

            meshNormals.Add(normalAB);
            meshNormals.Add(normalAC);
            meshNormals.Add(normalBC);
        }

        // Add new tangents. These MUST be added in the same order as the vertices above!
        if (meshTangents.Count > 0)
        {
            Vector4 tangentA = meshTangents[indexA];
            Vector4 tangentB = meshTangents[indexB];
            Vector4 tangentC = meshTangents[indexC];
            Vector4 tangentAB = (tangentA + tangentB) / 2;
            Vector4 tangentAC = (tangentA + tangentC) / 2;
            Vector4 tangentBC = (tangentB + tangentC) / 2;

            meshTangents.Add(tangentAB);
            meshTangents.Add(tangentAC);
            meshTangents.Add(tangentBC);
        }

        // Add new UVs. These MUST be added in the same order as the vertices above!
        if (meshUVs.Count > 0)
        {
            Vector2 uvA = meshUVs[indexA];
            Vector2 uvB = meshUVs[indexB];
            Vector2 uvC = meshUVs[indexC];
            Vector2 uvAB = (uvA + uvB) / 2;
            Vector2 uvAC = (uvA + uvC) / 2;
            Vector2 uvBC = (uvB + uvC) / 2;

            meshUVs.Add(uvAB);
            meshUVs.Add(uvAC);
            meshUVs.Add(uvBC);
        }

        int i = 0;
        while (i < newTriangleIndices.Count)
        {
            List<int> newSubTriangles = BreakDownTriangle(newTriangleIndices[i]);
            if (newSubTriangles.Count == 1)
            {
                // returned same triangle. no sub division could be done
                i++;
            }
            else
            {
                newTriangleIndices.AddRange(newSubTriangles);
            }
        }

        return newTriangleIndices;
    }
}