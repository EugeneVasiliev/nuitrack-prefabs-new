#if ENABLE_AR_TUTORIAL
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarSync : NetworkBehaviour
{

    [SerializeField]
    Transform[] syncBones;
    [SerializeField]
    Transform avatar;

    [ClientRpc] //The server sends to all clients
    public void RpcOnBonesTransformUpdate(BonesInfoMessage boneMsg)
    {
        for (int i = 0; i < boneMsg.bonesRot.Length; i++)
        {
            syncBones[i].localRotation = boneMsg.bonesRot[i];
        }

        avatar.localPosition = boneMsg.avatarPos;
    }

    public void BoneUpdate(Transform[] bones)
    {
        List<Quaternion> rotations = new List<Quaternion>();

        for (int i = 0; i < bones.Length; i++)
            rotations.Add(bones[i].localRotation);

        BonesInfoMessage msg = new BonesInfoMessage
        {
            bonesRot = rotations.ToArray(), //bone turns
            avatarPos = avatar.position,
        };

        RpcOnBonesTransformUpdate(msg); //send
    }

    private void FixedUpdate()
    {
        if (isServer)
        {
            BoneUpdate(syncBones);
        }
    }

    public class BonesInfoMessage : MessageBase
    {
        public Quaternion[] bonesRot;  //bone turns
        public Vector3 avatarPos;  //avatar position
    }
}
#endif