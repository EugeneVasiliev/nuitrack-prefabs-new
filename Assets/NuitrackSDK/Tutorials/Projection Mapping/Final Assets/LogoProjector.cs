using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoProjector : MonoBehaviour
{
    [SerializeField] RectTransform logoNuitrack;
    [SerializeField] RectTransform debugImage;
    [SerializeField] float zoom = 1;

    Vector2 startLogoSize;

    private void Start()
    {
        startLogoSize = logoNuitrack.sizeDelta;
    }

    void Update()
    {
        debugImage.localScale = Vector3.one * zoom;

        if (CurrentUserTracker.CurrentSkeleton == null)
            return;

        nuitrack.Skeleton skel = CurrentUserTracker.CurrentSkeleton;

        nuitrack.Joint torsoJoint = skel.GetJoint(nuitrack.JointType.Torso);
        nuitrack.Joint waistJoint = skel.GetJoint(nuitrack.JointType.Waist);
        //nuitrack.Joint leftShoulder = skel.GetJoint(nuitrack.JointType.LeftShoulder);
        //nuitrack.Joint rightShoulder = skel.GetJoint(nuitrack.JointType.RightShoulder);


        //logo.position = new Vector2(j.Proj.X * Screen.width, Screen.height - j.Proj.Y * Screen.height);
        logoNuitrack.anchoredPosition = GetUIPos(torsoJoint);
        logoNuitrack.rotation = torsoJoint.ToQuaternion();
        float newWidth = Vector2.Distance(GetUIPos(waistJoint), GetUIPos(torsoJoint));
        float newHeight = newWidth/startLogoSize.x*startLogoSize.y;
        logoNuitrack.sizeDelta = new Vector2(newWidth, newHeight);
    }

    Vector2 GetUIPos(nuitrack.Joint joint)
    {
        return new Vector2(-Screen.width / 2 * zoom + joint.Proj.X * Screen.width * zoom, Screen.height / 2 * zoom - joint.Proj.Y * Screen.height * zoom);
    }
}
