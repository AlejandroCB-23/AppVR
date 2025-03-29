using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Android;
using Wave.Essence.Eye;

public class EyeDataCollector : MonoBehaviour
{
    private Queue<EyeData> pendingSaveQueue = new Queue<EyeData>();
    private string normalDataFilePath;
    private string transformedDataFilePath;
    private string heatMapFilePath;

    private Dictionary<GameObject, int> heatMap = new Dictionary<GameObject, int>();

    private LineRenderer lineRenderer;

    // Para eliminar ruido
    private float captureInterval = 0.05f; // Intervalo de 50ms (20 veces por segundo)
    private float lastCaptureTime = 0f;

    void Start()
    {
        RequestStoragePermissions(); // Solicitar permisos de almacenamiento
        string downloadsPath = Application.persistentDataPath;
        normalDataFilePath = Path.Combine(downloadsPath, "EyeTrackingNormalData.json");
        transformedDataFilePath = Path.Combine(downloadsPath, "EyeTrackingTransformedData.json");
        heatMapFilePath = Path.Combine(downloadsPath, "EyeTrackingHeatMap.json");

        if (EyeManager.Instance != null) // Verificar si tenemos en la escena un objeto de seguimiento ocular
        {
            EyeManager.Instance.EnableEyeTracking = true;
            CheckEyeTrackingAvailability(); // Verificar si el seguimiento ocular está disponible
        }

        InitializeLineRenderer(); // Inicializar el renderizador de líneas
    }

    void Update()
    {
        // Verificar si el seguimiento ocular está disponible
        if (EyeManager.Instance == null || !EyeManager.Instance.IsEyeTrackingAvailable())
            return;

        if (Time.time - lastCaptureTime >= captureInterval)
        {
            lastCaptureTime = Time.time;
            CaptureEyeTrackingData();
        }
    }

    void CaptureEyeTrackingData()
    {
        Vector3 leftEyeOrigin = Vector3.zero, leftEyeDirection = Vector3.forward;
        Vector3 rightEyeOrigin = Vector3.zero, rightEyeDirection = Vector3.forward;
        Vector3 combinedEyeOrigin = Vector3.zero, combinedEyeDirection = Vector3.forward;

        bool leftEyeDataAvailable = EyeManager.Instance.GetLeftEyeOrigin(out leftEyeOrigin) &&
                                    EyeManager.Instance.GetLeftEyeDirectionNormalized(out leftEyeDirection);
        bool rightEyeDataAvailable = EyeManager.Instance.GetRightEyeOrigin(out rightEyeOrigin) &&
                                     EyeManager.Instance.GetRightEyeDirectionNormalized(out rightEyeDirection);
        bool combinedEyeDataAvailable = EyeManager.Instance.GetCombinedEyeOrigin(out combinedEyeOrigin) &&
                                        EyeManager.Instance.GetCombindedEyeDirectionNormalized(out combinedEyeDirection);

        if (leftEyeDataAvailable || rightEyeDataAvailable || combinedEyeDataAvailable)
        {
            Transform headTransform = Camera.main.transform;

            // Transformación de las coordenadas de los ojos
            Vector3 leftEyeTransformed = leftEyeDataAvailable ? headTransform.TransformPoint(leftEyeOrigin) : Vector3.zero;
            Vector3 rightEyeTransformed = rightEyeDataAvailable ? headTransform.TransformPoint(rightEyeOrigin) : Vector3.zero;
            Vector3 combinedEyeTransformed = combinedEyeDataAvailable ? headTransform.TransformPoint(combinedEyeOrigin) : Vector3.zero;

            // Dirección de los ojos en el mundo
            Vector3 leftEyeWorldDirection = leftEyeDataAvailable ? headTransform.TransformDirection(leftEyeDirection) : Vector3.zero;
            Vector3 rightEyeWorldDirection = rightEyeDataAvailable ? headTransform.TransformDirection(rightEyeDirection) : Vector3.zero;
            Vector3 combinedEyeWorldDirection = combinedEyeDataAvailable ? headTransform.TransformDirection(combinedEyeDirection) : Vector3.zero;

            // Crear un objeto EyeData con los datos capturados
            EyeData eyeData = new EyeData(leftEyeOrigin, leftEyeDirection, rightEyeOrigin, rightEyeDirection, combinedEyeOrigin, combinedEyeDirection);
            pendingSaveQueue.Enqueue(eyeData);

            // Realizar el raycast con los datos de los ojos
            if (combinedEyeDataAvailable)
            {
                PerformRaycastWithEyeData(combinedEyeTransformed, combinedEyeWorldDirection);
            }

            // Guardar los datos en un hilo secundario
            Task.Run(async () =>
            {
                await SaveEyeDataAsync();  // Guardado de datos de ojos
                SaveHeatMapData();         // Guardado de datos del heatmap
            });
        }
    }

    void PerformRaycastWithEyeData(Vector3 eyeTransformed, Vector3 eyeDirection)
    {
        Ray ray = new Ray(eyeTransformed, eyeDirection);
        float maxDistance = 50f;
        RaycastHit hit;

        // Usamos el LayerMask para restringir el raycast a la capa "Default" 
        int layerMask = LayerMask.GetMask("Default") & ~LayerMask.GetMask("ButtonLayers");

        // Raycast solo impacta objetos de la capa seleccionada
        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            LogHitObject(hit.collider.gameObject);
            DrawEyeRay(eyeTransformed, hit.point);  // Dibujar el rayo hasta el objeto impactado
        }
        else
        {
            DrawEyeRay(eyeTransformed, eyeTransformed + eyeDirection * maxDistance);  // Dibujar rayo sin colisión
        }
    }

    void LogHitObject(GameObject hitObject)
    {
        if (heatMap.ContainsKey(hitObject))
            heatMap[hitObject]++;
        else
            heatMap.Add(hitObject, 1);

        UpdateObjectColor(hitObject); // Aplicar el color actualizado
    }

    void UpdateObjectColor(GameObject obj)
    {
        if (obj.TryGetComponent<Renderer>(out Renderer renderer))
        {
            // Obtén el número de impactos acumulados con el objeto
            int hitCount = heatMap.ContainsKey(obj) ? heatMap[obj] : 0;

            // Escala de impacto que define cuántos impactos se necesitan para que el objeto cambie de color
            float impactScale = 125f; // Aumenta este valor para que se necesiten más impactos

            // Calcula la intensidad con un cambio gradual (ajustado por la escala de impactos)
            float intensity = Mathf.Clamp01(Mathf.Sqrt(hitCount) / impactScale);  // Se necesita más hits para llegar a 1

            // Generar un mapa de calor entre varios colores
            Color newColor = GetHeatMapColor(intensity);
            renderer.material.color = newColor;
        }
    }

    // Función para obtener el color correspondiente a la intensidad
    Color GetHeatMapColor(float intensity)
    {
        if (intensity < 0.33f)
            return Color.Lerp(Color.blue, Color.green, intensity / 0.33f);
        else if (intensity < 0.66f)
            return Color.Lerp(Color.green, Color.yellow, (intensity - 0.33f) / 0.33f);
        else
            return Color.Lerp(Color.yellow, Color.red, (intensity - 0.66f) / 0.34f);
    }

    async Task SaveEyeDataAsync()
    {
        // Usamos una lista local para evitar acceder a la cola en cada iteración.
        List<EyeData> dataToSave = new List<EyeData>(pendingSaveQueue);
        pendingSaveQueue.Clear();

        // Guardar en el archivo "normalDataFilePath"
        string normalJsonData = JsonUtility.ToJson(new EyeDataBatch(dataToSave), true);
        await File.AppendAllTextAsync(normalDataFilePath, normalJsonData);  // Asincrónico, no bloquea el hilo principal

        // Transformar los datos y guardarlos en otro archivo
        List<EyeData> transformedDataToSave = new List<EyeData>();
        foreach (var data in dataToSave)
        {
            EyeData transformedData = new EyeData(
                Camera.main.transform.TransformPoint(data.leftEyeOrigin),
                Camera.main.transform.TransformDirection(data.leftEyeDirection),
                Camera.main.transform.TransformPoint(data.rightEyeOrigin),
                Camera.main.transform.TransformDirection(data.rightEyeDirection),
                Camera.main.transform.TransformPoint(data.combinedEyeOrigin),
                Camera.main.transform.TransformDirection(data.combinedEyeDirection)
            );
            transformedDataToSave.Add(transformedData);
        }

        string transformedJsonData = JsonUtility.ToJson(new EyeDataBatch(transformedDataToSave), true);
        await File.AppendAllTextAsync(transformedDataFilePath, transformedJsonData);  // Asincrónico
    }

    void SaveHeatMapData()
    {
        List<HeatMapData> heatMapList = new List<HeatMapData>();
        foreach (var entry in heatMap)
            heatMapList.Add(new HeatMapData(entry.Key.name, entry.Value));

        string heatMapJsonData = JsonUtility.ToJson(new HeatMapBatch(heatMapList), true);
        File.WriteAllText(heatMapFilePath, heatMapJsonData);  // Escribir de manera síncrona
    }

    void RequestStoragePermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
    }

    void CheckEyeTrackingAvailability()
    {
        if (EyeManager.Instance.IsEyeTrackingAvailable())
        {
            Debug.Log("Eye tracking is available.");
        }
        else
        {
            Debug.LogWarning("Eye tracking is NOT available.");
        }
    }

    void InitializeLineRenderer()
    {
        GameObject lineObj = new GameObject("EyeTrackingRay");
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.002f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 2;
    }

    void DrawEyeRay(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }


}

// ---------------------- CLASES AUXILIARES ----------------------

[System.Serializable]
public class EyeData
{
    public Vector3 leftEyeOrigin, leftEyeDirection, rightEyeOrigin, rightEyeDirection, combinedEyeOrigin, combinedEyeDirection;
    public EyeData(Vector3 lo, Vector3 ld, Vector3 ro, Vector3 rd, Vector3 co, Vector3 cd)
    {
        leftEyeOrigin = lo; leftEyeDirection = ld;
        rightEyeOrigin = ro; rightEyeDirection = rd;
        combinedEyeOrigin = co; combinedEyeDirection = cd;
    }
}

[System.Serializable]
public class EyeDataBatch { public List<EyeData> eyeData; public EyeDataBatch(List<EyeData> data) { eyeData = data; } }
[System.Serializable]
public class HeatMapData { public string objectName; public int hitCount; public HeatMapData(string name, int count) { objectName = name; hitCount = count; } }
[System.Serializable]
public class HeatMapBatch { public List<HeatMapData> heatMapData; public HeatMapBatch(List<HeatMapData> data) { heatMapData = data; } }
