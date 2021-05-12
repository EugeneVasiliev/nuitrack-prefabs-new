using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using nuitrack.issues;

public class UserTrackerVisualization : MonoBehaviour
{
    #region Fields
    IssuesProcessor issuesProcessor;

    ulong lastFrameTimestamp = ulong.MaxValue;

    [SerializeField] int hRes;
    int frameStep;
    float depthToScale;

    //visualization fields
    [SerializeField] Color[] userCols;
    Color[] occludedUserCols;

    [SerializeField] Color defaultColor;
    [SerializeField] Mesh sampleMesh;
    [SerializeField] float meshScaling = 1f;
    [SerializeField] Material visualizationMaterial;

    int pointsPerVis;

    int vertsPerMesh, trisPerMesh;
    int[] sampleTriangles;
    Vector3[] sampleVertices;
    Vector3[] sampleNormals;
    Vector2[] sampleUvs;

    List<int[]> triangles;
    List<Vector3[]> vertices;
    List<Vector3[]> normals;
    List<Vector2[]> uvs;
    List<Vector2[]> uv2s;
    List<Vector2[]> uv3s;
    //List<Vector2[]> uv4s;
    List<Color[]> colors;

    Color[] userCurrentCols;

    GameObject visualizationPart;
    Mesh visualizationMesh;

    RenderTexture depthTexture, rgbTexture, segmentationTexture;

    ExceptionsLogger exceptionsLogger;

    bool active = false;
    bool initialized = false;

    #endregion

    public void SetActive(bool _active)
    {
        active = _active;
    }

    public void SetShaderProperties(Color newZeroColor, bool showBorders)
    {
        StartCoroutine(WaitSetShaderProperties(newZeroColor, showBorders));
    }

    IEnumerator WaitSetShaderProperties(Color newZeroColor, bool showBorders)
    {
        while (!NuitrackManager.Instance.nuitrackInitialized)
        {
            yield return null;
        }

        if (!initialized) Initialize();

        userCols[0] = newZeroColor;
        userCurrentCols[0] = newZeroColor;
        occludedUserCols[0] = newZeroColor;
        visualizationMaterial.SetColor("_SegmZeroColor", newZeroColor);
        visualizationMaterial.SetInt("_ShowBorders", showBorders ? 1 : 0);
    }

    IEnumerator WaitInit()
    {
        while (!NuitrackManager.Instance.nuitrackInitialized)
        {
            yield return null;
        }

        Initialize();
    }

    void Initialize()
    {
        if (initialized)
            return;

        initialized = true;
        occludedUserCols = new Color[userCols.Length];
        userCurrentCols = new Color[userCols.Length];
        for (int i = 0; i < userCols.Length; i++)
        {
            userCurrentCols[i] = userCols[i];
            float[] hsv = new float[3];
            Color.RGBToHSV(userCols[i], out hsv[0], out hsv[1], out hsv[2]);
            hsv[2] *= 0.25f;
            occludedUserCols[i] = Color.HSVToRGB(hsv[0], hsv[1], hsv[2]);
            occludedUserCols[i].a = userCols[i].a;
        }

        issuesProcessor = IssuesProcessor.Instance;
        nuitrack.OutputMode mode = NuitrackManager.DepthSensor.GetOutputMode();
        frameStep = mode.XRes / hRes;
        if (frameStep <= 0) frameStep = 1; // frameStep should be greater then 0
        hRes = mode.XRes / frameStep;

        depthToScale = meshScaling * 2f * Mathf.Tan(0.5f * mode.HFOV) / hRes;

        InitMeshes(
          ((mode.XRes / frameStep) + (mode.XRes % frameStep == 0 ? 0 : 1)),
          ((mode.YRes / frameStep) + (mode.YRes % frameStep == 0 ? 0 : 1)),
          mode.HFOV
        );
    }

    void Start()
    {
        StartCoroutine(WaitInit());
    }

    #region Mesh generation and mesh update methods
    void InitMeshes(int cols, int rows, float hfov)
    {
        DebugDepth.depthMat.mainTexture = depthTexture;
        DebugDepth.segmentationMat.mainTexture = rgbTexture;

        int numPoints = cols * rows;

        vertsPerMesh = sampleMesh.vertices.Length;
        trisPerMesh = sampleMesh.triangles.Length;

        sampleVertices = sampleMesh.vertices;
        Vector4[] sampleVertsV4 = new Vector4[sampleVertices.Length];

        for (int i = 0; i < sampleVertices.Length; i++)
        {
            sampleVertices[i] *= depthToScale;
            sampleVertsV4[i] = sampleVertices[i];
            //visualizationMaterial.SetVector("_Offsets" + i.ToString(), sampleVertices[i]); //unity 5.3-
        }
        visualizationMaterial.SetVectorArray("_Offsets", sampleVertsV4); //unity 5.4+

        sampleTriangles = sampleMesh.triangles;
        sampleNormals = sampleMesh.normals;
        sampleUvs = sampleMesh.uv;

        vertices = new List<Vector3[]>();
        triangles = new List<int[]>();
        normals = new List<Vector3[]>();
        uvs = new List<Vector2[]>();
        uv2s = new List<Vector2[]>();
        uv3s = new List<Vector2[]>();

        colors = new List<Color[]>();

        pointsPerVis = int.MaxValue;

        visualizationMesh = new Mesh();
        visualizationMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        float fX, fY;
        fX = 0.5f / Mathf.Tan(0.5f * hfov);
        fY = fX * cols / rows;

        visualizationMaterial.SetFloat("fX", fX);
        visualizationMaterial.SetFloat("fY", fY);

        //generation of triangle indexes, vertices, uvs and normals for all visualization parts

        for (int i = 0, row = 0, col = 0; i < 1; i++)
        {
            int numPartPoints = cols * rows;

            int[] partTriangles = new int[numPartPoints * trisPerMesh];
            Vector3[] partVertices = new Vector3[numPartPoints * vertsPerMesh];
            Vector3[] partNormals = new Vector3[numPartPoints * vertsPerMesh];
            Vector2[] partUvs = new Vector2[numPartPoints * vertsPerMesh];
            Vector2[] partUv2s = new Vector2[numPartPoints * vertsPerMesh];
            Vector2[] partUv3s = new Vector2[numPartPoints * vertsPerMesh];
            Color[] partColors = new Color[numPartPoints * vertsPerMesh];

            for (int j = 0; j < numPartPoints; j++)
            {
                for (int k = 0; k < trisPerMesh; k++)
                {
                    partTriangles[j * trisPerMesh + k] = sampleTriangles[k] + j * vertsPerMesh;
                }
                Vector2 depthTextureUV = new Vector2(((float)col + 0.5f) / cols, ((float)row + 0.5f) / rows);
                for (int k = 0; k < vertsPerMesh; k++)
                {
                    partUv2s[j * vertsPerMesh + k] = depthTextureUV;
                    partUv3s[j * vertsPerMesh + k] = new Vector2(k, 0);
                }
                System.Array.Copy(sampleVertices, 0, partVertices, j * vertsPerMesh, vertsPerMesh);
                System.Array.Copy(sampleNormals, 0, partNormals, j * vertsPerMesh, vertsPerMesh);
                System.Array.Copy(sampleUvs, 0, partUvs, j * vertsPerMesh, vertsPerMesh);

                col++;
                if (col == cols)
                {
                    row++;
                    col = 0;
                }
            }

            triangles.Add(partTriangles);
            vertices.Add(partVertices);
            normals.Add(partNormals);
            uvs.Add(partUvs);
            uv2s.Add(partUv2s);
            uv3s.Add(partUv3s);
            colors.Add(partColors);

            visualizationMesh.vertices = vertices[i];
            visualizationMesh.triangles = triangles[i];
            visualizationMesh.normals = normals[i];
            visualizationMesh.uv = uvs[i];
            visualizationMesh.uv2 = uv2s[i];
            visualizationMesh.uv3 = uv3s[i];
            visualizationMesh.colors = colors[i];

            Bounds meshBounds = new Bounds(500f * new Vector3(0f, 0f, 1f), 2000f * Vector3.one);
            visualizationMesh.bounds = meshBounds;
            visualizationMesh.MarkDynamic();

            visualizationPart = new GameObject();
            visualizationPart.name = "Visualization_" + i.ToString();
            visualizationPart.transform.position = Vector3.zero;
            visualizationPart.transform.rotation = Quaternion.identity;
            visualizationPart.AddComponent<MeshFilter>();
            visualizationPart.GetComponent<MeshFilter>().mesh = visualizationMesh;
            visualizationPart.AddComponent<MeshRenderer>();
            visualizationPart.GetComponent<Renderer>().sharedMaterial = visualizationMaterial;
        }
    }
    #endregion

    void Update()
    {
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
        if (visualizationPart.activeSelf)
            visualizationPart.SetActive(false);
    }

    void ProcessFrame(nuitrack.DepthFrame depthFrame, nuitrack.ColorFrame colorFrame, nuitrack.UserFrame userFrame)
    {
        if (!visualizationPart.activeSelf)
            visualizationPart.SetActive(true);

        if (userFrame != null)
        {
            if (issuesProcessor.userIssues != null)
            {
                for (int i = 1; i < userCurrentCols.Length; i++)
                {
                    if (issuesProcessor.userIssues.ContainsKey(i))
                    {
                        userCurrentCols[i] =
                          (issuesProcessor.userIssues[i].isOccluded ||
                          issuesProcessor.userIssues[i].onBorderLeft ||
                          issuesProcessor.userIssues[i].onBorderRight ||
                          issuesProcessor.userIssues[i].onBorderTop) ?
                          occludedUserCols[i] : userCols[i];
                    }
                }
            }
        }

        if (colorFrame == null)
            rgbTexture = FrameProvider.DepthFrame.GetRenderTexture();
        else
            rgbTexture = FrameProvider.ColorFrame.GetRenderTexture();

        depthTexture = FrameProvider.DepthFrame.GetRenderTexture();
        segmentationTexture = FrameProvider.UserFrame.GetRenderTexture(userCurrentCols);

        visualizationMaterial.SetFloat("_maxSensorDepth", FrameProvider.DepthFrame.MaxSensorDepth);

        visualizationMaterial.SetTexture("_RGBTex", rgbTexture);
        visualizationMaterial.SetTexture("_DepthTex", depthTexture);
        visualizationMaterial.SetTexture("_SegmentationTex", segmentationTexture);
    }

    void OnDestroy()
    {
        if (depthTexture != null) Destroy(depthTexture);
        if (rgbTexture != null) Destroy(rgbTexture);

        DebugDepth.depthMat.mainTexture = null;
        DebugDepth.segmentationMat.mainTexture = null;

        if (visualizationPart != null)
            Destroy(visualizationPart);

        if (issuesProcessor != null) Destroy(issuesProcessor.gameObject);
    }
}