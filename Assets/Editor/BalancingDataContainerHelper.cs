/*
|    Balancing data decoder/encoder for decomp - made by Heroic
|    This script will automatically encode the balancing data/loca whenever you edit a JSON file,
|    meaning you don't need to manually encode or decode anymore (except sometimes when there might be issues)
|    
|    In Unity, you'll find a menu called "Balancing Data" on the menu bar (at the top)
|    In the menu, there are a few options:
|    
|    1. Decode
|    * Deserialize balancing container (convert all classes in live_SerializedBalancingDataContainer_3.0.1.bytes to JSON)
|    * Deserialize event balancing container (convert all classes in live_SerializedEventBalancingDataContainer.bytes to JSON)
|    
|    2. Encode
|    * Serialize balancing container (convert all JSONs in BalancingData/ to live_SerializedBalancingDataContainer_3.0.1.bytes)
|    * Serialize event balancing container (convert all JSONs in BalancingData/EventBalancingData to live_SerializedEventBalancingDataContainer.bytes)
|
|    3. Loca
|    * Serialize all loca (convert all loca JSONs in BalancingData/Loca to their .bytes container)
|    * Deserialize all loca (convert all loca .bytes to JSON)
|    * Find untranslated entries (finds all entries in live_English that aren't translated in other languages, and outputs them to BalancingData/Loca/UntranslatedLoca)
|      - You can change the base language it checks on line 508
|
|    4. Watch for file changes
|    * Toggles automatically encoding balancing data/loca
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ABH.Shared.BalancingData;
using ABH.Shared.Models;
using Chimera.Library.Components.Models;
using Chimera.Library.Components.Services;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;

[InitializeOnLoad]
public static class BalancingDataContainerHelper
{
    private const string Tag = "[BalancingDataContainerHelper] ";
    
    private static FileSystemWatcher m_balancingFileWatcher;
    
    private static FileSystemWatcher m_eventBalancingFileWatcher;
    
    private static FileSystemWatcher m_locaFileWatcher;
    
    public static readonly string BalancingDataJsonPath = Path.Combine(Application.dataPath, "BalancingData");
    
    public static readonly string EventBalancingDataJsonPath = Path.Combine(BalancingDataJsonPath, "EventBalancingData");

    public static readonly string LocaJsonPath = Path.Combine(BalancingDataJsonPath, "Loca");
    
    public static readonly string UntranslatedLocaJsonPath = Path.Combine(LocaJsonPath, "UntranslatedLoca");
    
    public static readonly string StreamingAssetsPath = Path.Combine(Application.streamingAssetsPath, "local");
    
    public static readonly string BalancingContainerPath = Path.Combine(StreamingAssetsPath, "live_SerializedBalancingDataContainer_3.0.1.bytes");

    public static readonly string EventBalancingContainerPath = Path.Combine(StreamingAssetsPath, "live_SerializedEventBalancingDataContainer.bytes");

    private static Dictionary<string, bool> m_cooldownDictionary = new(); // so it doesn't spam reimport when decoding whole containers
    
    private static bool m_isWatchingChanges = true;
    
    static BalancingDataContainerHelper()
    {
        Init();
    }
    
    private static void Init()
    {
        if (!Directory.Exists(BalancingDataJsonPath))
        {
            Directory.CreateDirectory(BalancingDataJsonPath);
            CreateJsonsFromNonEventContainer();
        }

        if (!Directory.Exists(EventBalancingDataJsonPath))
        {
            Directory.CreateDirectory(EventBalancingDataJsonPath);
            CreateJsonsFromEventContainer();
        }

        if (!Directory.Exists(LocaJsonPath))
        {
            Directory.CreateDirectory(LocaJsonPath);
            CreateJsonsFromLocaContainers();
        }

        m_balancingFileWatcher = new FileSystemWatcher
        {
            Path = BalancingDataJsonPath,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "*.json",
            EnableRaisingEvents = true
        };
        
        m_eventBalancingFileWatcher = new FileSystemWatcher
        {
            Path = EventBalancingDataJsonPath,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "*.json",
            EnableRaisingEvents = true
        };
        
        m_locaFileWatcher = new FileSystemWatcher
        {
            Path = LocaJsonPath,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "*.json",
            EnableRaisingEvents = true
        };
        
        StartWatchingFileChanges();
    }
    
    [MenuItem("Balancing Data/Loca/Serialize all loca", false, 0)]
    public static void CreateLocaContainersFromJsons()
    {
        var supportedLocas = GetAllSupportedLocaFileNames();

        var sw = Stopwatch.StartNew();
        foreach (var locaContainer in supportedLocas)
        {
            CreateLocaContainerFromJson(locaContainer, locaContainer, false);
        }
        sw.Stop();
        
        Debug.Log(Tag + $"Serialized {supportedLocas.Count} locas in {sw.Elapsed}");
    }

    [MenuItem("Balancing Data/Loca/Deserialize all loca", false, 0)]
    public static void CreateJsonsFromLocaContainers()
    {
        var supportedLocas = GetAllSupportedLocaFileNames();

        var sw = Stopwatch.StartNew();
        foreach (var locaContainer in supportedLocas)
        {
            DeserializeLocaToJson(locaContainer, false);
        }
        sw.Stop();
        
        Debug.Log(Tag + $"Deserialized {supportedLocas.Count} locas in {sw.Elapsed}");
    }
    
    [MenuItem("Balancing Data/Decode/Balancing container", false, 0)]
    public static void CreateJsonsFromNonEventContainer()
    {
        CreateJsonsFromContainer(false);
    }
    
    [MenuItem("Balancing Data/Decode/Event balancing container", false, 0)]
    public static void CreateJsonsFromEventContainer()
    {
        CreateJsonsFromContainer(true);
    }
    
    [MenuItem("Balancing Data/Encode/Balancing container", false, 0)]
    public static void CreateNonEventContainerFromJsons()
    {
        CreateContainerFromJsons(false);
    }
    
    [MenuItem("Balancing Data/Encode/Event balancing container", false, 0)]
    public static void CreateEventContainerFromJsons()
    {
        CreateContainerFromJsons(true);
    }
    
    private static SerializedLocalizedTexts DeserializeLoca(string locaPath)
    {
        var compressedLoca = File.ReadAllBytes(locaPath);
        var decompressedLoca = DIContainerInfrastructure.GetCompressionService().DecompressIfNecessary(compressedLoca);
        var serializer = DIContainerInfrastructure.GetLocaSerializer();
        serializer.Log = null; // shut up
        var deserializedLoca = serializer.Deserialize<SerializedLocalizedTexts>(decompressedLoca);

        return deserializedLoca;
    }

    private static byte[] SerializeLoca(SerializedLocalizedTexts container)
    {
        var serializedLoca = DIContainerInfrastructure.GetLocaSerializer().SerializeToBytes(container);
        var compressedLoca = DIContainerInfrastructure.GetCompressionService().Compress(serializedLoca);

        return compressedLoca;
    }
    
    private static List<string> GetAllSupportedLocaFileNames()
    {
        return Resources.LoadAll<TextAsset>("Loca")
            .Where(t => !t.name.Contains("dev") && !t.name.Contains("LocaBalancingData"))
            .Select(t => "live_" + t.name)
            .ToList();
    }
    
    private static void CreateLocaContainerFromJson(string locaContainer, string jsonPath, bool log = true)
    {
        var sw = Stopwatch.StartNew();

        var locaJsonPath = Path.Combine(LocaJsonPath, locaContainer + ".json");
        
        if (!File.Exists(locaJsonPath))
        {
            Debug.LogError($"Could not find the {locaContainer} loca JSON at {locaJsonPath}!");
            return;
        }
        
        var locaJson = File.ReadAllText(locaJsonPath);
        SerializedLocalizedTexts deserializedJson;
        try
        {
            deserializedJson = JsonConvert.DeserializeObject<SerializedLocalizedTexts>(locaJson);
        }
        catch (Exception err)
        {
            EditorApplication.delayCall += () =>
            {
                HandleException(err, locaContainer, jsonPath);
            };
            return;
        }

        var locaContainerPath = Path.Combine(StreamingAssetsPath, locaContainer + ".bytes");
        File.WriteAllBytes(locaContainerPath, SerializeLoca(deserializedJson));
        
        sw.Stop();
        
        if (log)
            Debug.Log(Tag + $"Serialized {locaContainer} loca in {sw.Elapsed}");
    }
    
    private static void DeserializeLocaToJson(string locaContainer, bool log = true)
    {
        var sw = Stopwatch.StartNew();
        
        if (!Directory.Exists(LocaJsonPath))
            Directory.CreateDirectory(LocaJsonPath);

        var locaPath = Path.Combine(StreamingAssetsPath, locaContainer + ".bytes");
        
        if (!File.Exists(locaPath))
        {
            Debug.LogError($"Could not find the {locaContainer} loca container at {locaPath}!");
            return;
        }
        
        var loca = DeserializeLoca(locaPath);
        
        var serializedJson = JsonConvert.SerializeObject(loca, Formatting.Indented, new JsonConverter[]
        {
            new IsoDateTimeConverter(),
            new StringEnumConverter(),
            GetNullableBooleanConverter()
        });

        var outputPath = Path.Combine(LocaJsonPath, locaContainer + ".json");
        
        if (m_isWatchingChanges)
            m_cooldownDictionary.Add(locaContainer, true);
        
        File.WriteAllText(outputPath, serializedJson);
        
        sw.Stop();
        
        if (log) 
            Debug.Log(Tag + $"Deserialized {locaContainer} loca in {sw.Elapsed}");
    }
    
    [CanBeNull]
    private static SerializedBalancingDataContainer DeserializeContainer(string containerPath)
    {
        if (!File.Exists(containerPath))
        {
            Debug.LogError($"Could not find {Path.GetFileName(containerPath)} at {containerPath}!");
            return null;
        }
        
        var compressedContainer = File.ReadAllBytes(containerPath);
        var decompressedContainer = DIContainerInfrastructure.GetCompressionService().DecompressIfNecessary(compressedContainer);
        var serializer = DIContainerInfrastructure.GetBalancingDataSerializer();
        serializer.Log = null; // shut up
        var deserializedContainer = serializer.Deserialize<SerializedBalancingDataContainer>(decompressedContainer);

        return deserializedContainer;
    }

    private static byte[] SerializeContainer(SerializedBalancingDataContainer container)
    {
        var serializedContainer = DIContainerInfrastructure.GetBalancingDataSerializer().SerializeToBytes(container);
        var compressedContainer = DIContainerInfrastructure.GetCompressionService().Compress(serializedContainer);
        
        return compressedContainer;
    }

    private static void LocaJsonChanged(object sender, FileSystemEventArgs e)
    {
        var locaContainer = Path.GetFileNameWithoutExtension(e.FullPath);
        if (m_cooldownDictionary.TryGetValue(locaContainer, out var shouldWait) && shouldWait)
        {
            m_cooldownDictionary.Remove(locaContainer);
            return;
        }
        CreateLocaContainerFromJson(locaContainer, e.FullPath);
    }

    private static void BalancingJsonChanged(object sender, FileSystemEventArgs e)
    {
        var balancingClassName = "ABH.Shared.BalancingData." + Path.GetFileNameWithoutExtension(e.FullPath);
        if (m_cooldownDictionary.TryGetValue(balancingClassName, out var shouldWait) && shouldWait)
        {
            m_cooldownDictionary.Remove(balancingClassName);
            return;
        }
        
        var json = File.ReadAllText(e.FullPath!);
        var container = DeserializeContainer(BalancingContainerPath);
        if (container == null)
            return;
        
        ReimportClass(balancingClassName, json, e.FullPath, container);
        
        File.WriteAllBytes(BalancingContainerPath, SerializeContainer(container));
    }
    
    private static void EventBalancingJsonChanged(object sender, FileSystemEventArgs e)
    {
        var balancingClassName = "ABH.Shared.Events.BalancingData." + Path.GetFileNameWithoutExtension(e.FullPath);
        if (m_cooldownDictionary.TryGetValue(balancingClassName, out var shouldWait) && shouldWait)
        {
            m_cooldownDictionary.Remove(balancingClassName);
            return;
        }
        
        var json = File.ReadAllText(e.FullPath!);
        var container = DeserializeContainer(EventBalancingContainerPath);
        if (container == null)
            return;
        
        ReimportClass(balancingClassName, json, e.FullPath, container);
        
        File.WriteAllBytes(EventBalancingContainerPath, SerializeContainer(container));
    }
    
    public static void CreateContainerFromJsons(bool isEvent)
    {
        var balancingDataPath = isEvent ? EventBalancingDataJsonPath : BalancingDataJsonPath;

        if (!Directory.Exists(balancingDataPath))
        {
            Debug.LogError(Tag + $"No {(isEvent ? "event " : string.Empty)}JSONs found at path {balancingDataPath}!");
            return;
        }
        
        var jsonPaths = Directory.GetFiles(balancingDataPath, "*.json").ToList();
        
        var outputContainerPath = isEvent ? EventBalancingContainerPath : BalancingContainerPath;
        var container = new SerializedBalancingDataContainer
        {
            AllBalancingData = new Dictionary<string, byte[]>(),
            Version = "0001"
        };

        var totalStopwatch = Stopwatch.StartNew();
        foreach (var jsonPath in jsonPaths)
        {
            var balancingClassName = $"ABH.Shared.{(isEvent ? "Events." : string.Empty)}BalancingData." + Path.GetFileNameWithoutExtension(jsonPath);
            var balancingDataJson = File.ReadAllText(jsonPath);
            
            var success = ReimportClass(balancingClassName, balancingDataJson, jsonPath, container, false);

            if (!success)
            {
                // reimport failed, don't break the container and return
                return;
            }
        }
        
        File.WriteAllBytes(outputContainerPath, SerializeContainer(container));
        
        totalStopwatch.Stop();
        Debug.Log(Tag + $"Serialized {container.AllBalancingData.Count} classes to the {(isEvent ? "event " : string.Empty)}balancing container in {totalStopwatch.Elapsed}");
    }

    private static void HandleException(Exception err, string balancingClassName, string jsonPath)
    {
        Debug.LogError(Tag + $"Failed to reimport {balancingClassName}! JSON is invalid!");
        Debug.LogException(err);
        
        var gotoError = EditorUtility.DisplayDialog(
            "Reimport failed!",
            $"Failed to reimport {balancingClassName.Split('.').Last()}!\nJSON is invalid!\n\n" +
            $"{err.Message}",
            "Go to error"
        );

        if (!gotoError)
            return;

        if (err is JsonReaderException jsonErr)
        {
            InternalEditorUtility.OpenFileAtLineExternal(jsonPath, jsonErr.LineNumber);
        }
    }

    private static bool ReimportClass(string balancingClassName, string json, string jsonPath, SerializedBalancingDataContainer container, bool log = true)
    {
        var sw = Stopwatch.StartNew();
        
        var balancingDataType = typeof(GameConstantsBalancingData).Assembly.GetType(balancingClassName);

        object deserializedJson;
        try
        {
            deserializedJson = JsonConvert.DeserializeObject(json, typeof(List<>).MakeGenericType(balancingDataType));
        }
        catch (Exception err)
        {
            EditorApplication.delayCall += () =>
            {
                HandleException(err, balancingClassName, jsonPath);
            };
            return false;
        }

        var serializedBalancingData = DIContainerInfrastructure.GetBalancingDataSerializer().SerializeToBytes(deserializedJson);
        container.AllBalancingData[balancingClassName] = serializedBalancingData;
        
        sw.Stop();
        
        if (log)
            Debug.Log(Tag + $"Reimported {balancingClassName} in {sw.Elapsed}");
        
        return true;
    }

    public static void CreateJsonsFromContainer(bool isEvent)
    {
        var containerPath = isEvent ? EventBalancingContainerPath : BalancingContainerPath;
        var outputJsonPath = isEvent ? EventBalancingDataJsonPath : BalancingDataJsonPath;

        if (!File.Exists(containerPath))
        {
            Debug.LogError($"Could not find the {(isEvent ? "event " : string.Empty)}container at {containerPath}!");
            return;
        }
        
        if (!Directory.Exists(outputJsonPath))
            Directory.CreateDirectory(outputJsonPath);
        
        // load container
        var container = DeserializeContainer(containerPath);
        if (container == null)
            return;

        var totalStopwatch = Stopwatch.StartNew();
        foreach (var balancingDataPair in container.AllBalancingData)
        {
            var balancingDataType = typeof(GameConstantsBalancingData).Assembly.GetType(balancingDataPair.Key);
            var balancingDataBytes = balancingDataPair.Value;
            var outputPath = Path.Combine(outputJsonPath, balancingDataType.Name + ".json");
            
            var balancingData = DIContainerInfrastructure.GetBalancingDataSerializer().Deserialize(balancingDataBytes, typeof(List<>).MakeGenericType(balancingDataType));
            
            var serializedJson = JsonConvert.SerializeObject(balancingData, Formatting.Indented, new JsonConverter[]
            {
                new IsoDateTimeConverter(),
                new StringEnumConverter(),
                GetNullableBooleanConverter()
            });
            
            if (m_isWatchingChanges) 
                m_cooldownDictionary.Add(balancingDataPair.Key, true);
            
            File.WriteAllText(outputPath, serializedJson);
        }
        totalStopwatch.Stop();
        
        Debug.Log(Tag + $"Deserialized {container.AllBalancingData.Count} {(isEvent ? "event " : string.Empty)}classes in {totalStopwatch.Elapsed}");
    }

    public static JsonConverter GetNullableBooleanConverter()
    {
        // nullable boolean converter is internal 😡💢
        var nbcType = typeof(StringSerializerNewtonSoftImpl).Assembly.GetType("Chimera.Library.Components.Services.NullableBooleanConverter");
        return Activator.CreateInstance(nbcType) as JsonConverter;
    }
    
    private class UntranslatedLocalizedTexts
    {
        public string LanguageId { get; set; }
        public List<string> UntranslatedTexts { get; set; }
    }

    [MenuItem("Balancing Data/Loca/Find untranslated entries", false, 1)]
    public static void FindUntranslatedEntries()
    {
        // Edit the following line to change the base language to check
        const string baseLanguage = "live_English";

        if (!Directory.Exists(UntranslatedLocaJsonPath))
            Directory.CreateDirectory(UntranslatedLocaJsonPath);

        var supportedLocas = GetAllSupportedLocaFileNames();
        supportedLocas.Remove(baseLanguage);
        
        var baseLocaPath = Path.Combine(StreamingAssetsPath, baseLanguage + ".bytes");

        if (!File.Exists(baseLocaPath))
        {
            Debug.Log(Tag + $"{baseLanguage}.bytes does not exist!");
            return;
        }

        var sw = Stopwatch.StartNew();
        
        var baseLoca = DeserializeLoca(baseLocaPath);

        var untranslated = new List<UntranslatedLocalizedTexts>();
        foreach (var locaContainer in supportedLocas)
        {
            var translatedLocaPath = Path.Combine(StreamingAssetsPath, locaContainer + ".bytes");
            var translatedLoca = DeserializeLoca(translatedLocaPath);

            untranslated.Add(GetUntranslatedEntries(baseLoca, translatedLoca));
        }
        
        var untranslatedJsonPath = Path.Combine(UntranslatedLocaJsonPath, "UntranslatedLoca.json");
        var untranslatedJson = JsonConvert.SerializeObject(untranslated, Formatting.Indented);
        File.WriteAllText(untranslatedJsonPath, untranslatedJson);
        
        sw.Stop();
        
        Debug.Log(Tag + $"Checked {supportedLocas.Count} locas for untranslated entries in {sw.Elapsed}, wrote results to {untranslatedJsonPath}");
    }

    private static UntranslatedLocalizedTexts GetUntranslatedEntries(SerializedLocalizedTexts baseTexts, SerializedLocalizedTexts translatedTexts)
    {
        var untranslatedEntries = new UntranslatedLocalizedTexts
        {
            LanguageId = translatedTexts.LanguageId,
            UntranslatedTexts = new List<string>()
        };

        foreach (var text in baseTexts.Texts.Keys)
        {
            if (!translatedTexts.Texts.ContainsKey(text))
                untranslatedEntries.UntranslatedTexts.Add(text);
        }

        return untranslatedEntries;
    }

    [MenuItem("Balancing Data/Watch for file changes")]
    public static void ToggleWatchingFileChanges()
    {
        m_isWatchingChanges = !m_isWatchingChanges;
        
        if (m_isWatchingChanges)
            StartWatchingFileChanges();
        else
            StopWatchingFileChanges();
    }
    
    [MenuItem("Balancing Data/Watch for file changes", true)]
    private static bool ValidateToggleAutoReload()
    {
        Menu.SetChecked("Balancing Data/Watch for file changes", m_isWatchingChanges);
        return true;
    }
    
    private static void StartWatchingFileChanges()
    {
        StopWatchingFileChanges();
        
        m_balancingFileWatcher.Changed += BalancingJsonChanged;
        m_eventBalancingFileWatcher.Changed += EventBalancingJsonChanged;
        m_locaFileWatcher.Changed += LocaJsonChanged;
    }
    
    private static void StopWatchingFileChanges()
    {
        m_balancingFileWatcher.Changed -= BalancingJsonChanged;
        m_eventBalancingFileWatcher.Changed -= EventBalancingJsonChanged;
        m_locaFileWatcher.Changed -= LocaJsonChanged;
    }
}