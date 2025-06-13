using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Net;
using Alex.OcularVergenceLibrary;

public class Data : MonoBehaviour
{
    [Header("Network Configuration")]
    public string serverIP = "192.168.110.72";
    public int dataPort = 5006;

    [Header("Data Collection Settings")]
    public float dataCollectionRate = 30f;
    public Camera targetCamera;

    private UdpClient udpDataClient;
    private IPEndPoint dataEndPoint;

    private float lastDataTime;
    private int frameCounter = 0;
    private float recordingStartTime = -1f; 

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        InitializeNetwork();
    }

    void InitializeNetwork()
    {
        try
        {
            udpDataClient = new UdpClient();
            dataEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), dataPort);
            Debug.Log("Network initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize network: " + e.Message);
        }
    }

    void Update()
    {
        if (RecordingState.IsRecording && recordingStartTime < 0)
        {
            recordingStartTime = Time.time;
            frameCounter = 0;
            Debug.Log("Recording started - timestamp reset to 0");
        }

        if (!RecordingState.IsRecording && recordingStartTime >= 0)
        {
            recordingStartTime = -1f;
            Debug.Log("Recording stopped");
        }

        if (RecordingState.IsRecording && Time.time - lastDataTime >= 1f / dataCollectionRate)
        {
            CollectAndSendData();
            lastDataTime = Time.time;
        }
    }

    void CollectAndSendData()
    {
        var heatmapData = new HeatmapDataPoint();

        heatmapData.timestamp = Time.time - recordingStartTime;
        heatmapData.frameNumber = frameCounter++;
        heatmapData.deltaTime = Time.deltaTime;

        if (targetCamera != null)
        {
            heatmapData.headPosition = targetCamera.transform.position;
            heatmapData.headRotation = targetCamera.transform.rotation;
            heatmapData.cameraFOV = targetCamera.fieldOfView;
        }

        CollectEyeTrackingData(ref heatmapData);
        CollectGazeScreenPosition(ref heatmapData);
        SendData(heatmapData);
    }

    void CollectEyeTrackingData(ref HeatmapDataPoint data)
    {
        if (EyeData.TryGetWorldEyeData(out EyeTrackingData eyeData))
        {
            data.hasEyeTracking = true;
            data.leftEyeOrigin = eyeData.leftEyeOrigin;
            data.leftEyeDirection = eyeData.leftEyeDirection;
            data.rightEyeOrigin = eyeData.rightEyeOrigin;
            data.rightEyeDirection = eyeData.rightEyeDirection;
            data.combinedEyeOrigin = eyeData.combinedEyeOrigin;
            data.combinedEyeDirection = eyeData.combinedEyeDirection;
        }
        else
        {
            data.hasEyeTracking = false;
            if (targetCamera != null)
            {
                data.combinedEyeOrigin = targetCamera.transform.position;
                data.combinedEyeDirection = targetCamera.transform.forward;
            }
        }
    }

    void CollectGazeScreenPosition(ref HeatmapDataPoint data)
    {
        Vector3 gazeOrigin = data.hasEyeTracking ? data.combinedEyeOrigin : targetCamera.transform.position;
        Vector3 gazeDirection = data.hasEyeTracking ? data.combinedEyeDirection : targetCamera.transform.forward;

        Ray ray = new Ray(gazeOrigin, gazeDirection);
        Vector3 fallbackPoint = ray.GetPoint(10f);

        Vector3 screenPoint = targetCamera.WorldToScreenPoint(fallbackPoint);
        data.gazeScreenPosition = new Vector2(screenPoint.x, screenPoint.y);
        data.gazeScreenDepth = screenPoint.z;
        data.gazeNormalizedPosition = new Vector2(screenPoint.x / Screen.width, screenPoint.y / Screen.height);
        data.screenWidth = Screen.width;
        data.screenHeight = Screen.height;
    }

    void SendData(HeatmapDataPoint data)
    {
        try
        {
            string jsonData = JsonUtility.ToJson(data);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
            udpDataClient.Send(bytes, bytes.Length, dataEndPoint);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to send data: " + e.Message);
        }
    }

    void OnDestroy()
    {
        udpDataClient?.Close();
    }

    public static class RecordingState
    {
        public static bool IsRecording = false;
    }
}

[System.Serializable]
public class HeatmapDataPoint
{
    public float timestamp;
    public int frameNumber;
    public float deltaTime;

    public Vector3 headPosition;
    public Quaternion headRotation;
    public float cameraFOV;

    public bool hasEyeTracking;
    public Vector3 leftEyeOrigin;
    public Vector3 leftEyeDirection;
    public Vector3 rightEyeOrigin;
    public Vector3 rightEyeDirection;
    public Vector3 combinedEyeOrigin;
    public Vector3 combinedEyeDirection;

    public Vector2 gazeScreenPosition;
    public Vector2 gazeNormalizedPosition;
    public float gazeScreenDepth;
    public int screenWidth;
    public int screenHeight;
}

