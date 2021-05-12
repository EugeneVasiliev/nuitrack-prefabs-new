using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserTrackerVisMesh : MonoBehaviour
{
    ulong lastFrameTimestamp = ulong.MaxValue;

    //  List<int[]> triangles;
    //  List<Vector3[]> vertices;
    //  List<Vector2[]> uvs;
    //  List<Vector2[]> uv2s;
    //  List<Vector2[]> uv3s;
    //  List<Vector2[]> uv4s;

    [SerializeField] Color defaultColor;
    Mesh[] meshes;
    GameObject[] visualizationParts;
    [SerializeField] Material meshMaterial;
    [SerializeField] Color[] userCols;

    Texture2D depthTexture, rgbTexture, segmentationTexture;
    Color[] depthColors;

    bool active = false;

    public void SetActive(bool _active)
    {
        active = _active;
    }

    public void SetShaderProperties(bool showBackground, bool showBorders)
    {
        meshMaterial.SetInt("_ShowBackground", showBackground ? 1 : 0);
        meshMaterial.SetInt("_ShowBorders", showBorders ? 1 : 0);
    }

    void Start()
    {
        StartCoroutine(WaitInit());
    }

    IEnumerator WaitInit()
    {
        while (!NuitrackManager.Instance.nuitrackInitialized)
        {
            yield return null;
        }

        Initialize();
    }

    private void Initialize()
    {
        nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();
        int xRes = mode.XRes;
        int yRes = mode.YRes;

        InitMeshes(xRes, yRes, mode.HFOV);
    }

    void InitMeshes(int cols, int rows, float hfov)
    {
        depthColors = new Color[cols * rows];

        depthTexture = new Texture2D(cols, rows, TextureFormat.RFloat, false);
        depthTexture.filterMode = FilterMode.Point;
        depthTexture.wrapMode = TextureWrapMode.Clamp;
        depthTexture.Apply();

        meshMaterial.SetTexture("_DepthTex", depthTexture);

        float fX, fY;
        fX = 0.5f / Mathf.Tan(0.5f * hfov);
        fY = fX * cols / rows;

        meshMaterial.SetFloat("fX", fX);
        meshMaterial.SetFloat("fY", fY);

        int numMeshes;
        const int maxVertices = int.MaxValue;

        numMeshes = (cols * rows) / maxVertices + (((cols * rows) % maxVertices == 0) ? 0 : 1);

        Debug.Log("Num meshes: " + numMeshes.ToString());

        visualizationParts = new GameObject[numMeshes];
        meshes = new Mesh[numMeshes];

        for (int i = 0; i < numMeshes; i++)
        {
            Debug.Log("Mesh #" + i.ToString());
            int xLow = (i * cols) / numMeshes;
            int xHigh = (((i + 1) * cols) / numMeshes) + (((i + 1) == numMeshes) ? 0 : 1);
            int numVerts = rows * (xHigh - xLow);
            int numTris = 2 * (rows - 1) * (xHigh - xLow - 1);

            Debug.Log("xLow = " + xLow.ToString() + "; xHigh = " + xHigh.ToString() + "; verts = " + numVerts.ToString() + "; tris = " + numTris.ToString());

            int[] partTriangles = new int[3 * numTris];
            Vector3[] partVertices = new Vector3[numVerts];
            Vector2[] partUvs = new Vector2[numVerts];

            int index = 0;
            int trisIndex = 0;

            for (int y = 0; y < rows; y++)
            {
                for (int x = xLow; x < xHigh; x++, index++)
                {
                    Vector2 depthTextureUV = new Vector2(((float)x + 0.5f) / cols, ((float)y + 0.5f) / rows);
                    partVertices[index] = Vector3.zero;
                    partUvs[index] = depthTextureUV;

                    if ((x < (xHigh - 1)) && (y < (rows - 1)))
                    {
                        partTriangles[trisIndex + 0] = index;
                        partTriangles[trisIndex + 1] = index + (xHigh - xLow);
                        partTriangles[trisIndex + 2] = index + (xHigh - xLow) + 1;

                        partTriangles[trisIndex + 3] = index;
                        partTriangles[trisIndex + 4] = index + (xHigh - xLow) + 1;
                        partTriangles[trisIndex + 5] = index + 1;

                        trisIndex += 6;
                    }
                }
            }

            meshes[i] = new Mesh();
            meshes[i].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshes[i].vertices = partVertices;
            meshes[i].uv = partUvs;
            meshes[i].triangles = partTriangles;

            Bounds meshBounds = new Bounds(500f * new Vector3(0f, 0f, 1f), 2000f * Vector3.one);
            meshes[i].bounds = meshBounds;

            visualizationParts[i] = new GameObject();
            visualizationParts[i].name = "Visualization_" + i.ToString();
            visualizationParts[i].transform.position = Vector3.zero;
            visualizationParts[i].transform.rotation = Quaternion.identity;
            visualizationParts[i].AddComponent<MeshFilter>();
            visualizationParts[i].GetComponent<MeshFilter>().mesh = meshes[i];
            visualizationParts[i].AddComponent<MeshRenderer>();
            visualizationParts[i].GetComponent<Renderer>().sharedMaterial = meshMaterial;

        }
    }

    void Update()
    {
        if (!NuitrackManager.Instance.nuitrackInitialized)
            return;

        if ((NuitrackManager.DepthFrame != null) && active)
        {
            nuitrack.DepthFrame depthFrame = NuitrackManager.DepthFrame;
            nuitrack.ColorFrame colorFrame = NuitrackManager.ColorFrame;
            nuitrack.UserFrame userFrame = NuitrackManager.UserFrame;

            bool haveNewFrame = (lastFrameTimestamp != depthFrame.Timestamp);
            if (haveNewFrame)
            {
                ProcessFrame(depthFrame, colorFrame, userFrame);
                lastFrameTimestamp = depthFrame.Timestamp;
            }
        }
        else
        {
            HideVisualization();
        }
    }

    void HideVisualization()
    {
        if (visualizationParts == null)
            return;

        for (int i = 0; i < visualizationParts.Length; i++)
        {
            if (visualizationParts[i].activeSelf) visualizationParts[i].SetActive(false);
        }
    }

    void ProcessFrame(nuitrack.DepthFrame depthFrame, nuitrack.ColorFrame colorFrame, nuitrack.UserFrame userFrame)
    {
        for (int i = 0; i < visualizationParts.Length; i++)
        {
            if (!visualizationParts[i].activeSelf) visualizationParts[i].SetActive(true);
        }

        System.DateTime t1 = System.DateTime.Now;
        for (int i = 0, pointIndex = 0; i < depthFrame.Rows; i++)
        {
            for (int j = 0; j < depthFrame.Cols; j++, ++pointIndex)
            {
                depthColors[pointIndex].r = depthFrame[i, j] / 16384f;
            }
        }
        
        rgbTexture = FrameProvider.ColorFrame.GetTexture2D();
        segmentationTexture = FrameProvider.UserFrame.GetTexture2D();

        meshMaterial.SetTexture("_SegmentationTex", segmentationTexture);
        meshMaterial.SetTexture("_RGBTex", rgbTexture);

        depthTexture.SetPixels(depthColors);
        depthTexture.Apply();

    }
}