using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public enum BallStatus
{
    Unknown, // uninitialized
    Receiving, // receiving updates from a remote server authority
    Simulating, // spawned and physics simulating locally as an authority
    GivingAway, // local to remote, still simulating locally
    TakingOver // remote to local, still receiving from remote
}

public delegate void OnBallStateChanged(BallStatus from, BallStatus to);

public class BallScript : MonoBehaviour
{
    public static BallScript[] Instances { get; private set; } = Array.Empty<BallScript>();
    private static List<BallScript> s_Instances = new();

    public int OwnerServer { get; set; } = -1;

    private BallStatus m_State = BallStatus.Unknown;

    public BallStatus State
    {
        get => m_State;
        set
        {
            var from = m_State;
            m_State = value;

            StateChanged(from, m_State);
        }
    }

    public event OnBallStateChanged StateChanged = (from, to) => { };

    private static Camera s_MainCamera;

    public uint id;
    public Rigidbody rb;

    private void Awake()
    {
        if (s_MainCamera == null)
        {
            s_MainCamera = Camera.main;
        }

        rb = GetComponent<Rigidbody>();

        s_Instances.Add(this);
        Instances = s_Instances.ToArray();
    }

    private void OnDestroy()
    {
        s_Instances.Remove(this);
        Instances = s_Instances.ToArray();
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -10.0f)
        {
            rb.isKinematic = true;
        }
    }

    private void OnGUI()
    {
        var hudWorldPos = transform.position;
        var hudScreenPos = s_MainCamera.WorldToScreenPoint(hudWorldPos);

        var defaultColor = GUI.color;
        GUI.color = Color.magenta;
        GUI.Label(new Rect(hudScreenPos.x, Screen.height - hudScreenPos.y, 250, 20), $"OwnerServer: {OwnerServer} | State: {State}");
        GUI.Label(new Rect(hudScreenPos.x, Screen.height - hudScreenPos.y + 20, 250, 20), hudWorldPos.ToString());
        GUI.color = defaultColor;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(this);
        var globalObjectIdHash = globalObjectId.ToString().Hash32();

        id = globalObjectIdHash;
    }
#endif // UNITY_EDITOR
}
