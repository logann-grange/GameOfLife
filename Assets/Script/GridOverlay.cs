using System.Collections.Generic;
using UnityEngine;

public class GridOverlay : MonoBehaviour
{
    public int gridSizeX = 64;
    public int gridSizeY = 48;
    public float startX = -0.5f;
    public float startY = -0.5f;

    public bool showSub = true;
    public int smallStep = 1;
    public int largeStep = 8;

    public Color subColor = new Color(0f, 0.5f, 0f, 0.6f);
    public Color mainColor = new Color(0f, 1f, 0f, 1f);

    void Start()
    {
        Build();
    }

    public void Rebuild(int sizeX, int sizeY)
    {
        gridSizeX = sizeX;
        gridSizeY = sizeY;
        Build();
    }

    void Build()
    {
        var vertices = new List<Vector3>();
        var colors = new List<Color>();
        var indices = new List<int>();

        if (showSub) AddLines(smallStep, subColor.linear, -1f, vertices, colors, indices);
            AddLines(largeStep, mainColor.linear, -1.1f, vertices, colors, indices);

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices, MeshTopology.Lines, 0);

        var mf = GetComponent<MeshFilter>();
        if (mf == null)
        {
            mf = gameObject.AddComponent<MeshFilter>();
            var mr = gameObject.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Sprites/Default"));
            mr.sortingOrder = 10;
        }
        if (mf.sharedMesh != null) Destroy(mf.sharedMesh);
            mf.mesh = mesh;
    }

    void AddLines(int step, Color color, float z, List<Vector3> v, List<Color> c, List<int> i)
    {
        if (step <= 0) return;

        for (int y = 0; y <= gridSizeY; y += step)
        {
            AddSegment(new Vector3(startX, startY + y, z), new Vector3(startX + gridSizeX, startY + y, z), color, v, c, i);
        }

        for (int x = 0; x <= gridSizeX; x += step)
        {
            AddSegment(new Vector3(startX + x, startY, z), new Vector3(startX + x, startY + gridSizeY, z), color, v, c, i);
        }
            
    }

    void AddSegment(Vector3 a, Vector3 b, Color color, List<Vector3> v, List<Color> c, List<int> i)
    {
        int n = v.Count;
        v.Add(a); v.Add(b);
        c.Add(color); c.Add(color);
        i.Add(n); i.Add(n + 1);
    }
}