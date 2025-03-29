// Assets/Editor/WaveSDKPostProcessor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class WaveSDKPostProcessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        // Verificar si se import� alg�n archivo del SDK de Wave
        bool hasWaveSDK = false;
        foreach (string asset in importedAssets)
        {
            if (asset.Contains("Wave.Essence") || asset.Contains("WaveVR"))
            {
                hasWaveSDK = true;
                break;
            }
        }

        // Si se detect� el SDK, agregar el s�mbolo WAVE_SDK_IMPORTED
        if (hasWaveSDK)
        {
            AddWaveSDKDefineSymbol();
        }
    }

    private static void AddWaveSDKDefineSymbol()
    {
        // Obtener los s�mbolos actuales
        BuildTargetGroup buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);

        // Si WAVE_SDK_IMPORTED no est� definido, agregarlo
        if (!defines.Contains("WAVE_SDK_IMPORTED"))
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                buildTarget,
                defines + ";WAVE_SDK_IMPORTED");
            Debug.Log("Wave SDK detectado. S�mbolo WAVE_SDK_IMPORTED agregado.");
        }
    }
}
#endif
