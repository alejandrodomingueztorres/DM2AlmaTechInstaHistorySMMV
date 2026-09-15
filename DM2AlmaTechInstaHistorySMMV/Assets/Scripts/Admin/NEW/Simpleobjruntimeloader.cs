using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
public class Simpleobjruntimeloader : MonoBehaviour
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

        string materialLibName = null;

        string[] lines = File.ReadAllLines(path);

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("mtllib "))
            {
                materialLibName = line.Substring(7).Trim();
            }
            else if (line.StartsWith("v "))
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

        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        Texture2D texture = LoadFirstTexture(path, materialLibName);
        if (texture != null)
            material.mainTexture = texture;

        mr.material = material;

        return obj;
    }

    /// <summary>
    /// Busca el .mtl referenciado por el .obj (mtllib) y carga la primera
    /// textura difusa (map_Kd) que encuentre. Solo soporta una textura por
    /// modelo: si el .obj tiene varios materiales, se usa el primero.
    /// </summary>
    private static Texture2D LoadFirstTexture(string objPath, string materialLibName)
    {
        string textureFileName = GetFirstTextureFileName(objPath, materialLibName);
        if (string.IsNullOrEmpty(textureFileName))
            return null;

        string texturePath = Path.Combine(Path.GetDirectoryName(objPath), textureFileName);

        if (!File.Exists(texturePath))
        {
            Debug.LogWarning("No se encontró la textura junto al modelo: " + texturePath);
            return null;
        }

        Texture2D texture = new Texture2D(2, 2);
        if (!texture.LoadImage(File.ReadAllBytes(texturePath)))
        {
            Debug.LogWarning("No se pudo leer la textura (usa .png o .jpg): " + texturePath);
            return null;
        }

        return texture;
    }

    /// <summary>Nombre del archivo .mtl que un .obj referencia, o null si no tiene.</summary>
    public static string GetMaterialLibFileName(string objPath)
    {
        foreach (string rawLine in File.ReadAllLines(objPath))
        {
            string line = rawLine.Trim();
            if (line.StartsWith("mtllib "))
                return line.Substring(7).Trim();
        }
        return null;
    }

    /// <summary>Nombre del primer archivo de textura (map_Kd) que un .mtl referencia, o null.</summary>
    public static string GetFirstTextureFileName(string objPath, string materialLibName = null)
    {
        materialLibName ??= GetMaterialLibFileName(objPath);
        if (string.IsNullOrEmpty(materialLibName))
            return null;

        string mtlPath = Path.Combine(Path.GetDirectoryName(objPath), materialLibName);
        if (!File.Exists(mtlPath))
        {
            Debug.LogWarning("El .obj referencia un .mtl que no está junto a él: " + mtlPath);
            return null;
        }

        foreach (string rawLine in File.ReadAllLines(mtlPath))
        {
            string line = rawLine.Trim();
            if (line.StartsWith("map_Kd "))
                return line.Substring(7).Trim();
        }

        return null;
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
