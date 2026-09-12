using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public static class SimpleOBJRuntimeLoader
{
    public static GameObject LoadOBJ(string path)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        List<Vector3> finalVertices = new List<Vector3>();
        List<Vector3> finalNormals = new List<Vector3>();
        List<Vector2> finalUvs = new List<Vector2>();

        string[] lines = File.ReadAllLines(path);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("v "))
            {
                string[] p = line.Split(' ');
                vertices.Add(new Vector3(Parse(p[1]), Parse(p[2]), Parse(p[3])));
            }
            else if (line.StartsWith("vn "))
            {
                string[] p = line.Split(' ');
                normals.Add(new Vector3(Parse(p[1]), Parse(p[2]), Parse(p[3])));
            }
            else if (line.StartsWith("vt "))
            {
                string[] p = line.Split(' ');
                uvs.Add(new Vector2(Parse(p[1]), Parse(p[2])));
            }
            else if (line.StartsWith("f "))
            {
                string[] p = line.Split(' ');

                for (int i = 1; i < p.Length - 2; i++)
                {
                    AddFaceVertex(p[1], vertices, normals, uvs, finalVertices, finalNormals, finalUvs, triangles);
                    AddFaceVertex(p[i + 1], vertices, normals, uvs, finalVertices, finalNormals, finalUvs, triangles);
                    AddFaceVertex(p[i + 2], vertices, normals, uvs, finalVertices, finalNormals, finalUvs, triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(finalVertices);
        mesh.SetTriangles(triangles, 0);

        if (finalNormals.Count == finalVertices.Count)
            mesh.SetNormals(finalNormals);
        else
            mesh.RecalculateNormals();

        if (finalUvs.Count == finalVertices.Count)
            mesh.SetUVs(0, finalUvs);

        mesh.RecalculateBounds();

        GameObject obj = new GameObject(Path.GetFileNameWithoutExtension(path));
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        mf.mesh = mesh;
        mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        return obj;
    }

    private static void AddFaceVertex(
        string face,
        List<Vector3> vertices,
        List<Vector3> normals,
        List<Vector2> uvs,
        List<Vector3> finalVertices,
        List<Vector3> finalNormals,
        List<Vector2> finalUvs,
        List<int> triangles)
    {
        string[] indices = face.Split('/');

        int vIndex = int.Parse(indices[0]) - 1;
        finalVertices.Add(vertices[vIndex]);

        if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]))
        {
            int uvIndex = int.Parse(indices[1]) - 1;
            if (uvIndex >= 0 && uvIndex < uvs.Count)
                finalUvs.Add(uvs[uvIndex]);
        }

        if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]))
        {
            int nIndex = int.Parse(indices[2]) - 1;
            if (nIndex >= 0 && nIndex < normals.Count)
                finalNormals.Add(normals[nIndex]);
        }

        triangles.Add(finalVertices.Count - 1);
    }

    private static float Parse(string value)
    {
        return float.Parse(value, CultureInfo.InvariantCulture);
    }
}