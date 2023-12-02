using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct OneVoxel
{
    public Vector3 origin;
    public OneVoxel(Vector3 origin)
    {
        this.origin = origin;
    }

    private List<Vector3> XYQuadLocalVertices()
    {
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));

        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f));
        return vertices;
    }
    public List<Vector3> QuadVertices(Quaternion q)
    {
        List<Vector3> vertices = XYQuadLocalVertices();
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = q * vertices[i] + origin;
        }
        return vertices;
    }

    public List<int> QuadTriangles(int startIndex)
    {
        List<int> triangles = new List<int>();
        triangles.Add(startIndex);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);

        triangles.Add(startIndex + 3);
        triangles.Add(startIndex + 4);
        triangles.Add(startIndex + 5);
        return triangles;
    }
}

public class VoxelComponent : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    public Material material;

    ushort[] voxels;
    //block data
    // 0: air
    // 1: stone

    int width = 8;
    int height = 32;
    int depth = 8;
    // Start is called before the first frame update
    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

        //initialize world
        voxels = new ushort[width * height * depth];
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    voxels[calculateIndex(x, y, z)] = 1;
                }
            }
        }

        voxels[calculateIndex(4, 11, 4)] = 1;
        voxels[calculateIndex(4, 12, 4)] = 1;
        voxels[calculateIndex(4, 13, 4)] = 1;
    }
    public Mesh mesh;
    void Start()
    {
        mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();// vertices's coordinates
        List<int> triangles = new List<int>(); //indices of vertices which constist triangles of the mesh

        List<Vector3> directions = new List<Vector3>();
        directions.Add(new Vector3(1, 0, 0));
        directions.Add(new Vector3(-1, 0, 0));
        directions.Add(new Vector3(0, 1, 0));
        directions.Add(new Vector3(0, -1, 0));
        directions.Add(new Vector3(0, 0, 1));
        directions.Add(new Vector3(0, 0, -1));

        List<Quaternion> quarternions = directions.Select((v) => Quaternion.LookRotation(v)).ToList();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Vector3 origin = new Vector3(x, y, z);
                    OneVoxel cube = new OneVoxel(origin);

                    if (voxels[calculateIndex(x, y, z)] != 0)
                    {
                        for (int i = 0; i < quarternions.Count; i++)
                        {
                            Quaternion q = quarternions[i];
                            Vector3Int v = Vector3Int.FloorToInt(directions[i]);
                            Vector3Int o1 = new Vector3Int(x, y, z) + v;

                            if ((!(o1.x == width || o1.x == -1 || o1.y == height || o1.y == -1 || o1.z == -1 || o1.z == depth)) &&
                                (voxels[calculateIndex(x + v.x, y + v.y, z + v.z)] == 0))
                            {
                                vertices.AddRange(cube.QuadVertices(q));
                                triangles.AddRange(cube.QuadTriangles(triangles.Count));
                            }
                        }
                    }
                }
            }
        }
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

    int calculateIndex(int x, int y, int z)
    {
        //change this according to compression performance
        return x + z * width + y * width * depth;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
