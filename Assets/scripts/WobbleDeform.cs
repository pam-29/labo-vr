using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WobbleDeform : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private float noiseScale = 0.5f;

    private MeshFilter meshFilter;
    private Mesh originalMesh;
    private Mesh runtimeMesh;
    private Vector3[] baseVertices;
    private Vector3[] workingVertices;
    private bool actif = false;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.sharedMesh;

        runtimeMesh = Instantiate(originalMesh);
        meshFilter.mesh = runtimeMesh;

        baseVertices = originalMesh.vertices;
        workingVertices = new Vector3[baseVertices.Length];
    }

    public void SetActif(bool value)
    {
        actif = value;
        if (!actif)
        {
            runtimeMesh.vertices = baseVertices;
            runtimeMesh.RecalculateNormals();
        }
    }

    private void Update()
    {
        if (!actif) return;

        float t = Time.time * frequency;

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 v = baseVertices[i];
            float noise = Mathf.PerlinNoise(v.x * noiseScale + t, v.y * noiseScale + t) - 0.5f;
            workingVertices[i] = v + v.normalized * noise * amplitude;
        }

        runtimeMesh.vertices = workingVertices;
        runtimeMesh.RecalculateNormals();
        runtimeMesh.RecalculateBounds();
    }
}
