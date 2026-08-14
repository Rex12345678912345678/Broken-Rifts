/*
|    Runtime balancing data reloader for decomp - made by Olexandr_43
|    Reloads balancing data, event balancing, and localization while the game is running in the editor,
|    without requiring exiting Play Mode
|
|    In Unity, you'll find the option under "Balancing Data/Reload" on the menu bar (at the top)
|
|    1. Reload Runtime Balancing Data
|    * Re-loads main balancing bytes only; does not reset event balancing or localization
|    * Clears runtime balancing caches (shop, sales, daily login, power level, etc)
|
|    2. Reload Runtime Event Balancing
|    * Reloads event balancing via FinishWithEventBalancingInit
|    * Resets runtime event state caches
|
|    3. Reload Runtime Localization
|    * Reinitializes localization services and waits for them to finish
|    * Reapplies localization and refreshes loaded UI labels
|
|    4. Watch for file changes
|    * Toggles automatically reloading all runtime data when BalancingDataContainerHelper writes a .bytes file
*/

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Chimera.Library.Components.Interfaces;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BalancingDataRuntimeReload
{
    private const string Tag = "[BalancingDataRuntimeReload] ";

    private const string ReloadBalancingMenuPath = "Balancing Data/Reload/Reload Runtime Balancing Data";
    private const string ReloadEventBalancingMenuPath = "Balancing Data/Reload/Reload Runtime Event Balancing";
    private const string ReloadLocalizationMenuPath = "Balancing Data/Reload/Reload Runtime Localization";
    private const string WatchMenuPath = "Balancing Data/Reload/Watch for file changes";

    private static readonly FieldInfo m_eventBalancingServiceField = typeof(DIContainerBalancing).GetField(
        "m_eventBalancingService",
        BindingFlags.NonPublic | BindingFlags.Static);

    private static bool m_isWatchingChanges
    {
        get => EditorPrefs.GetBool("BalancingDataRuntimeReload.isWatchingChanges", true);
        set => EditorPrefs.SetBool("BalancingDataRuntimeReload.isWatchingChanges", value);
    }

    private static FileSystemWatcher m_bytesFileWatcher;

    private static volatile bool m_reloadPending; // set from the FileSystemWatcher thread, consumed on the main thread

    static BalancingDataRuntimeReload()
    {
        Init();
    }

    private static void Init()
    {
        m_bytesFileWatcher = new FileSystemWatcher
        {
            Path = BalancingDataContainerHelper.StreamingAssetsPath,
            NotifyFilter = NotifyFilters.LastWrite,
            Filter = "*.bytes",
            EnableRaisingEvents = true
        };

        StartWatchingFileChanges();
        EditorApplication.update += OnEditorUpdate;
    }

    private static void StartWatchingFileChanges()
    {
        StopWatchingFileChanges();
        m_bytesFileWatcher.Changed += OnBytesFileChanged;
    }

    private static void StopWatchingFileChanges()
    {
        m_bytesFileWatcher.Changed -= OnBytesFileChanged;
    }

    private static void OnEditorUpdate()
    {
        if (!m_reloadPending || !m_isWatchingChanges || !EditorApplication.isPlaying)
            return;

        m_reloadPending = false;
        ReloadAllRuntimeData();
    }

    private static void OnBytesFileChanged(object sender, FileSystemEventArgs e)
    {
        m_reloadPending = true;
    }

    [MenuItem(WatchMenuPath, false, 99)]
    public static void ToggleWatchingChanges()
    {
        m_isWatchingChanges = !m_isWatchingChanges;
    }

    [MenuItem(WatchMenuPath, true)]
    private static bool ValidateToggleWatchingChanges()
    {
        Menu.SetChecked(WatchMenuPath, m_isWatchingChanges);
        return true;
    }

    [MenuItem(ReloadBalancingMenuPath, false, 100)]
    public static void ReloadRuntimeBalancingData()
    {
        if (!EnsurePlayMode("balancing data"))
        {
            return;
        }

        try
        {
            if (!TryReloadBalancingData(preserveEventBalancing: true))
            {
                return;
            }

            RefreshBalancingRuntimeCaches();
            Debug.Log(Tag + "Reloaded runtime balancing data while the game is running (event balancing and loca unchanged).");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem(ReloadEventBalancingMenuPath, false, 101)]
    public static void ReloadRuntimeEventBalancing()
    {
        if (!EnsurePlayMode("event balancing"))
        {
            return;
        }

        try
        {
            if (!ReloadEventBalancingData())
            {
                Debug.LogWarning(Tag + "Event balancing reload did not complete.");
                return;
            }

            RefreshEventRuntimeCaches();
            Debug.Log(Tag + "Reloaded runtime event balancing.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem(ReloadLocalizationMenuPath, false, 102)]
    public static async void ReloadRuntimeLocalization()
    {
        if (!EnsurePlayMode("localization"))
        {
            return;
        }

        try
        {
            if (!await ReloadLocalizationAsync())
            {
                Debug.LogWarning(Tag + "Localization reload did not complete.");
                return;
            }

            RefreshLocalizedUi();
            Debug.Log(Tag + "Reloaded runtime localization.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    [MenuItem(ReloadBalancingMenuPath, true)]
    private static bool ValidateReloadRuntimeBalancingData()
    {
        return CanReloadRuntimeData();
    }

    [MenuItem(ReloadEventBalancingMenuPath, true)]
    private static bool ValidateReloadRuntimeEventBalancing()
    {
        return CanReloadRuntimeData();
    }

    [MenuItem(ReloadLocalizationMenuPath, true)]
    private static bool ValidateReloadRuntimeLocalization()
    {
        return CanReloadRuntimeData();
    }

    private static async void ReloadAllRuntimeData()
    {
        if (!EnsurePlayMode("runtime data"))
        {
            return;
        }

        try
        {
            if (!TryReloadBalancingData())
            {
                return;
            }

            if (!ReloadEventBalancingData())
            {
                Debug.LogWarning(Tag + "Event balancing reload did not complete.");
            }

            if (!await ReloadLocalizationAsync())
            {
                Debug.LogWarning(Tag + "Localization reload did not complete.");
            }

            RefreshBalancingRuntimeCaches();
            RefreshEventRuntimeCaches();
            RefreshLocalizedUi();
            Debug.Log(Tag + "Reloaded balancing, event balancing, and localization data.");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static bool EnsurePlayMode(string dataDescription)
    {
        if (EditorApplication.isPlaying)
        {
            return true;
        }

        Debug.LogWarning(Tag + $"Enter Play Mode before reloading {dataDescription}.");
        return false;
    }

    private static bool CanReloadRuntimeData()
    {
        return EditorApplication.isPlaying;
    }

    private static bool TryReloadBalancingData(bool preserveEventBalancing = false)
    {
        object preservedEventService = null;
        if (preserveEventBalancing)
        {
            if (m_eventBalancingServiceField == null)
            {
                Debug.LogWarning(
                    Tag + "Could not reflect DIContainerBalancing.m_eventBalancingService; event data may break until a full reload.");
            }
            else
            {
                preservedEventService = m_eventBalancingServiceField.GetValue(null);
            }
        }

        DIContainerBalancing.Reset();

        if (!DIContainerBalancing.Init(null, true))
        {
            Debug.LogError(Tag + "Balancing data reload failed.");
            return false;
        }

        if (preserveEventBalancing && m_eventBalancingServiceField != null)
        {
            m_eventBalancingServiceField.SetValue(null, preservedEventService);
        }

        return true;
    }

    private static void RefreshBalancingRuntimeCaches()
    {
        var gameConstantsProvider = DIContainerBalancing.GameConstantsBalancingDataProvider;
        if (gameConstantsProvider != null)
        {
            gameConstantsProvider.ResetCache();
        }

        var lootTableProvider = DIContainerBalancing.LootTableBalancingDataPovider;
        if (lootTableProvider != null)
        {
            lootTableProvider.ResetCache();
        }

        var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
        if (coreStateMgr == null)
        {
            return;
        }

        // shop and sales services are internal
        InvokeInternalStaticMethod(typeof(DIContainerLogic), "GetShopService", "ClearShopBalancingCache");
        InvokeInternalStaticMethod(typeof(DIContainerLogic), "GetSalesManagerService", "ClearSalesCache");
        DIContainerLogic.DailyLoginLogic.ClearDailyRewardCache();

        if (coreStateMgr.m_DailyLoginUi != null)
        {
            coreStateMgr.m_DailyLoginUi.ClearItemDisplayCache();
        }

        if (coreStateMgr.m_GenericUI != null)
        {
            coreStateMgr.m_GenericUI.ReInitialize();
        }

        DIContainerInfrastructure.GetPowerLevelCalculator().ClearCache();
    }

    private static void RefreshEventRuntimeCaches()
    {
        var eventSystemStateManager = DIContainerInfrastructure.EventSystemStateManager;
        if (eventSystemStateManager != null)
        {
            eventSystemStateManager.ResetEventManager();
        }

        var pvpSeasonStateMgr = DIContainerInfrastructure.PvPSeasonStateMgr;
        if (pvpSeasonStateMgr != null)
        {
            pvpSeasonStateMgr.ResetPvPSystem();
        }
    }

    private static async Task<bool> ReloadLocalizationAsync()
    {
        var startupLocaService = DIContainerInfrastructure.GetStartupLocaService();
        var startupLanguage = string.IsNullOrEmpty(startupLocaService.CurrentLanguageKey) ? null : startupLocaService.CurrentLanguageKey;
        startupLocaService.InitDefaultLoca(null, startupLanguage);

        var locaService = DIContainerInfrastructure.GetLocaService();
        var language = string.IsNullOrEmpty(locaService.CurrentLanguageKey) ? null : locaService.CurrentLanguageKey;
        locaService.InitDefaultLoca(ContentLoader.Instance, language);

        // poll until loca finishes loading, give up after 1sec 
        for (var i = 0; i < 1000; i++)
        {
            if (locaService.Initialized)
            {
                SyncNguiLocalization(locaService);
                return true;
            }

            await Task.Delay(1);
        }

        return false;
    }

    private static void RefreshLocalizedUi()
    {
        foreach (LocaScript locaScript in Resources.FindObjectsOfTypeAll(typeof(LocaScript)))
        {
            if (IsLoadedSceneComponent(locaScript))
            {
                locaScript.ReloadLoca();
            }
        }

        foreach (UILabel label in Resources.FindObjectsOfTypeAll(typeof(UILabel)))
        {
            if (IsLoadedSceneComponent(label))
            {
                label.MarkAsChanged();
            }
        }

        var coreStateMgr = DIContainerInfrastructure.GetCoreStateMgr();
        if (coreStateMgr == null)
        {
            return;
        }

        if (coreStateMgr.m_InfoOverlays != null)
        {
            coreStateMgr.m_InfoOverlays.HideAllTooltips();
        }
    }

    private static bool IsLoadedSceneComponent(Component component)
    {
        if (component == null || component.gameObject == null)
        {
            return false;
        }

        var scene = component.gameObject.scene;
        return scene.IsValid() && scene.isLoaded;
    }

    private static void SyncNguiLocalization(ABHLocaService locaService)
    {
        if (locaService == null || locaService.LocaConfig == null || locaService.LocaConfig.LocaDictionary == null)
        {
            return;
        }

        var language = string.IsNullOrEmpty(locaService.CurrentLanguageKey)
            ? ABHLocaService.DefaultLanguageName
            : locaService.CurrentLanguageKey;
        var dictionaryCopy = new System.Collections.Generic.Dictionary<string, string>(locaService.LocaConfig.LocaDictionary);
        Localization.Set(language, dictionaryCopy);
    }

    private static void InvokeInternalStaticMethod(Type targetType, string accessorName, string methodName)
    {
        var accessor = targetType.GetMethod(accessorName, BindingFlags.NonPublic | BindingFlags.Static);
        if (accessor == null)
        {
            Debug.LogWarning(Tag + $"Missing accessor: {targetType.Name}.{accessorName}");
            return;
        }

        var service = accessor.Invoke(null, null);
        if (service == null)
        {
            Debug.LogWarning(Tag + $"{targetType.Name}.{accessorName} returned null.");
            return;
        }

        var method = service.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (method == null)
        {
            Debug.LogWarning(Tag + $"Missing method: {service.GetType().Name}.{methodName}");
            return;
        }

        method.Invoke(service, null);
    }

    private static bool ReloadEventBalancingData()
    {
        // FinishWithEventBalancingInit is internal, so we go through reflection
        var reloadMethod = typeof(DIContainerBalancing).GetMethod(
            "FinishWithEventBalancingInit",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (reloadMethod == null)
        {
            Debug.LogWarning(Tag + "Could not find event balancing reload method.");
            return false;
        }

        var loadedService = default(IBalancingDataLoaderService);
        var callback = new Action<IBalancingDataLoaderService>(service => loadedService = service);

        try
        {
            var result = reloadMethod.Invoke(null, new object[] { callback });
            if (result is bool boolResult && !boolResult)
            {
                return false;
            }
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex; // unwrap to see actual exception
        }

        return loadedService != null || DIContainerBalancing.EventBalancingService != null;
    }
}