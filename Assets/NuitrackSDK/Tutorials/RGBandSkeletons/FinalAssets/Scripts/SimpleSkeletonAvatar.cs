using System.Collections.Generic;
using UnityEngine;

public class SimpleSkeletonAvatar : MonoBehaviour
{
    public bool autoProcessing = true;
    public float scale = 1;
    public Vector3 offset = Vector3.zero;
    [SerializeField] GameObject jointPrefab = null, connectionPrefab = null;

    [SerializeField] RectTransform rightShoulder;
    [SerializeField] RectTransform leftShoulder;

    int clicks = 0;

    nuitrack.JointType[] jointsInfo = new nuitrack.JointType[]
    {
        nuitrack.JointType.Head,
        nuitrack.JointType.Neck,
        nuitrack.JointType.LeftCollar,
        nuitrack.JointType.Torso,
        nuitrack.JointType.Waist,
        nuitrack.JointType.LeftShoulder,
        nuitrack.JointType.RightShoulder,
        nuitrack.JointType.LeftElbow,
        nuitrack.JointType.RightElbow,
        nuitrack.JointType.LeftWrist,
        nuitrack.JointType.RightWrist,
        nuitrack.JointType.LeftHand,
        nuitrack.JointType.RightHand,
        nuitrack.JointType.LeftHip,
        nuitrack.JointType.RightHip,
        nuitrack.JointType.LeftKnee,
        nuitrack.JointType.RightKnee,
        nuitrack.JointType.LeftAnkle,
        nuitrack.JointType.RightAnkle
    };

    nuitrack.JointType[,] connectionsInfo = new nuitrack.JointType[,]
    { //Right and left collars are currently located at the same point, that's why we use only 1 collar,
        //it's easy to add rightCollar, if it ever changes
        {nuitrack.JointType.Neck,           nuitrack.JointType.Head},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Neck},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.LeftShoulder},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.RightShoulder},
        {nuitrack.JointType.LeftCollar,     nuitrack.JointType.Torso},
        {nuitrack.JointType.Waist,          nuitrack.JointType.Torso},
        {nuitrack.JointType.Waist,          nuitrack.JointType.LeftHip},
        {nuitrack.JointType.Waist,          nuitrack.JointType.RightHip},
        {nuitrack.JointType.LeftShoulder,   nuitrack.JointType.LeftElbow},
        {nuitrack.JointType.LeftElbow,      nuitrack.JointType.LeftWrist},
        {nuitrack.JointType.LeftWrist,      nuitrack.JointType.LeftHand},
        {nuitrack.JointType.RightShoulder,  nuitrack.JointType.RightElbow},
        {nuitrack.JointType.RightElbow,     nuitrack.JointType.RightWrist},
        {nuitrack.JointType.RightWrist,     nuitrack.JointType.RightHand},
        {nuitrack.JointType.LeftHip,        nuitrack.JointType.LeftKnee},
        {nuitrack.JointType.LeftKnee,       nuitrack.JointType.LeftAnkle},
        {nuitrack.JointType.RightHip,       nuitrack.JointType.RightKnee},
        {nuitrack.JointType.RightKnee,      nuitrack.JointType.RightAnkle}
    };

    GameObject[] connections;
    Dictionary<nuitrack.JointType, GameObject> joints;

    void Start()
    {
        CreateSkeletonParts();
    }

    void CreateSkeletonParts()
    {
        joints = new Dictionary<nuitrack.JointType, GameObject>();

        for (int i = 0; i < jointsInfo.Length; i++)
        {
            if (jointPrefab != null)
            {
                GameObject joint = Instantiate(jointPrefab, transform, true);
                joint.SetActive(false);
                joints.Add(jointsInfo[i], joint);
            }
        }

        connections = new GameObject[connectionsInfo.GetLength(0)];

        for (int i = 0; i < connections.Length; i++)
        {
            if (connectionPrefab != null)
            {
                GameObject connection = Instantiate(connectionPrefab, transform, true);
                connection.SetActive(false);
                connections[i] = connection;
            }
        }
    }

    void Update()
    {
        if (autoProcessing)
        {
            ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);

            if (skel == null)
            {
                leftShoulder.gameObject.SetActive(false);
                rightShoulder.gameObject.SetActive(false);

                return;
            }

            leftShoulder.gameObject.SetActive(true);
            rightShoulder.gameObject.SetActive(true);

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                clicks++;
                Vector2 mousePos = Input.mousePosition - new Vector3(Screen.width / 2, Screen.height / 2);

                Vector2 rightPos = GetUIPos(skel.GetJoint(nuitrack.JointType.RightShoulder), scale, Vector2.zero);
                Vector2 leftPos = GetUIPos(skel.GetJoint(nuitrack.JointType.LeftShoulder), scale, Vector2.zero);

                if (clicks % 2 != 0)
                {
                    rightShoulder.anchoredPosition = mousePos;
                    offset = rightShoulder.anchoredPosition - rightPos;
                }
                else
                {
                    leftShoulder.anchoredPosition = mousePos;
                    float oldDist = Vector2.Distance(leftPos, rightPos);
                    float newDist = Vector2.Distance(mousePos, rightShoulder.anchoredPosition);
                    scale = newDist/oldDist;
                }
            }

            rightShoulder.anchoredPosition = GetUIPos(skel.GetJoint(nuitrack.JointType.RightShoulder), scale, offset);
            leftShoulder.anchoredPosition = GetUIPos(skel.GetJoint(nuitrack.JointType.LeftShoulder), scale, offset);
        }
            
    }

    nuitrack.Skeleton skel;

    public void ProcessSkeleton(nuitrack.Skeleton skeleton)
    {
        skel = skeleton;
        if (skeleton == null)
            return;

        for (int i = 0; i < jointsInfo.Length; i++)
        {
            nuitrack.Joint j = skeleton.GetJoint(jointsInfo[i]);
            if (j.Confidence > 0.01f)
            {
                joints[jointsInfo[i]].SetActive(true);
                joints[jointsInfo[i]].GetComponent<RectTransform>().anchoredPosition = GetUIPos(j, scale, new Vector2(offset.x, offset.y));
                    //new Vector2(j.Proj.X * Screen.width, Screen.height - j.Proj.Y * Screen.height) * scale;
            }
            else
            {
                joints[jointsInfo[i]].SetActive(false);
            }
        }

        for (int i = 0; i < connectionsInfo.GetLength(0); i++)
        {
            GameObject startJoint = joints[connectionsInfo[i, 0]];
            GameObject endJoint = joints[connectionsInfo[i, 1]];

            if (startJoint.activeSelf && endJoint.activeSelf)
            {
                connections[i].SetActive(true);

                connections[i].transform.position = startJoint.transform.position;
                connections[i].transform.right = endJoint.transform.position - startJoint.transform.position;
                float distance = Vector3.Distance(endJoint.transform.position, startJoint.transform.position);
                connections[i].transform.localScale = new Vector3(distance, 1f, 1f);
            }
            else
            {
                connections[i].SetActive(false);
            }
        }
    }

    public Vector2 GetUIPos(nuitrack.Joint joint, float zoom, Vector2 offset)
    {
        return new Vector2(-Screen.width / 2 * zoom + joint.Proj.X * Screen.width * zoom, Screen.height / 2 * zoom - joint.Proj.Y * Screen.height * zoom) + offset;
    }

    public Vector2 GetUIPos(Vector2 pos, float zoom, Vector2 offset)
    {
        return new Vector2(-Screen.width / 2 * zoom + pos.x * Screen.width * zoom, Screen.height / 2 * zoom - pos.y * Screen.height * zoom) + offset;
    }
}
