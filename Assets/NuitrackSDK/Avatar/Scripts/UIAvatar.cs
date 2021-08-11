using System.Collections.Generic;
using UnityEngine;

public class UIAvatar : MonoBehaviour
{
    [Header("Skeleton")]
    public bool autoProcessing = true;
    [SerializeField] GameObject jointPrefab = null, connectionPrefab = null;
    RectTransform parentRect;

    [Header("Facetracking")]
    public bool facetracking;

    JsonInfo faceInfo;
    Instances[] faces;

    nuitrack.JointType[] jointsInfo;

    List<RectTransform> connections;
    Dictionary<nuitrack.JointType, RectTransform> joints;

    void Start()
    {
        jointsInfo = CurrentUserTracker.CurrentSkeleton.GetJoints();
        CreateSkeletonParts();
        parentRect = GetComponent<RectTransform>();
    }

    void CreateSkeletonParts()
    {
        joints = new Dictionary<nuitrack.JointType, RectTransform>();
        connections = new List<RectTransform>();

        for (int i = 0; i < jointsInfo.Length; i++)
        {
            if (jointPrefab != null)
            {
                GameObject joint = Instantiate(jointPrefab, transform);
                joint.SetActive(false);

                RectTransform jointRectTransform = joint.GetComponent<RectTransform>();
                joints.Add(jointsInfo[i], jointRectTransform);
            }

            if (connectionPrefab != null)
            {
                GameObject connection = Instantiate(connectionPrefab, transform);
                connection.SetActive(false);

                RectTransform connectionRectTransform = connection.GetComponent<RectTransform>();
                connections.Add(connectionRectTransform);
            }
        }
    }

    void Update()
    {
        if (autoProcessing)
            ProcessSkeleton(CurrentUserTracker.CurrentSkeleton);
    }

    public void ProcessSkeleton(nuitrack.Skeleton skeleton)
    {
        if (skeleton == null)
            return;

        for (int i = 0; i < jointsInfo.Length; i++)
        {
            nuitrack.Joint j = skeleton.GetJoint(jointsInfo[i]);
            if (j.Confidence > 0.01f)
            {
                joints[jointsInfo[i]].gameObject.SetActive(true);

                Vector2 newPosition = new Vector2(
                    parentRect.rect.width * (Mathf.Clamp01(j.Proj.X) - 0.5f),
                    parentRect.rect.height * (0.5f - Mathf.Clamp01(j.Proj.Y)));

                joints[jointsInfo[i]].anchoredPosition = newPosition;
            }
            else
            {
                joints[jointsInfo[i]].gameObject.SetActive(false);
            }

            if(jointsInfo[i].GetParent() != nuitrack.JointType.None)
            {
                RectTransform startJoint = joints[jointsInfo[i]];
                RectTransform endJoint = joints[jointsInfo[i].GetParent()];

                if (startJoint.gameObject.activeSelf && endJoint.gameObject.activeSelf)
                {
                    connections[i].gameObject.SetActive(true);

                    connections[i].anchoredPosition = startJoint.anchoredPosition;
                    connections[i].transform.right = endJoint.position - startJoint.position;
                    float distance = Vector3.Distance(endJoint.anchoredPosition, startJoint.anchoredPosition);
                    connections[i].transform.localScale = new Vector3(distance, 1f, 1f);
                }
                else
                {
                    connections[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
