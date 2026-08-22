#if UNITY_IOS
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Chimera.Library.Components.Interfaces;
#endif

public class AnalyticsSystemFlurryiOSImpl
#if !UNITY_IOS || !ENABLE_IOS_NATIVE_CODE
{
#else
	: IAnalyticsSystem
{
	private string m_currentAppKey;

	public IAnalyticsSystem Init(string appKey)
    {
        m_currentAppKey = appKey;
        return this;
    }
    
    [DllImport("__Internal")] // dllimports in rcssdk should also be this on ios but i cant be bothered doing that
	private static extern bool _StartFlurrySession(string apiKey, bool enableCrashReporting);
    
	public bool StartSession(string appKey)
    {
        m_currentAppKey = appKey;
        return _StartFlurrySession(appKey, false);
    }
    
	public bool StartSession()
	{
        if (string.IsNullOrEmpty(m_currentAppKey))
        {
            DebugLog.Error("[AnalyticsSystemFlurryiOSImpl] App key not set!");
            return false;
        }

        return _StartFlurrySession(m_currentAppKey, false);
    }
    
	public void EndSession()
	{
	}
    
    [DllImport("__Internal")]
	private static extern bool _LogFlurryEvent(string eventName, bool isTimed = false);
    
	public bool LogEvent(string eventName, bool isTimed)
    {
        return _LogFlurryEvent(eventName, isTimed);
    }
    
	public bool LogEventWithParameter(string eventName, string parameterName, string parameterValue, bool isTimed = false)
	{
		DebugLog.Log("track flurry event: " + eventName + " with parameters!");
        var dict = new Dictionary<string, string> { { parameterName, parameterValue } };
        return _LogFlurryEventWithParameters(eventName, DictionaryToString(dict), isTimed);
    }
    
    [DllImport("__Internal")]
	private static extern bool _LogFlurryEventWithParameters(string eventName, string parameters, bool isTimed = false);
    
	public bool LogEventWithParameters(string eventName, Dictionary<string, string> parameters, bool isTimed)
	{
        DebugLog.Log("track flurry event: " + eventName + " with parameters!");

        if (parameters.Count <= 10)
            return _LogFlurryEventWithParameters(eventName, DictionaryToString(parameters), isTimed);
        
        DebugLog.Log("Flurry event has too many parameters (" + parameters.Count + "), splitting up the event.");

        var dict = new Dictionary<string, string>();

        var i = 0;
        var amountOfSplitEvents = 0;
        var success = true;
        foreach (var param in parameters)
        {
            dict.Add(param.Key, param.Value);
            if ((i++ % 10) == 0)
            {
                success &= LogEventWithParameters(eventName + amountOfSplitEvents, dict, isTimed); // amountOfSplitEvents might not be the actual param to string.concat, ida couldn't figure it out so i guessed
                dict.Clear();
                amountOfSplitEvents++;
            }
        }

        if (dict.Count > 0)
            success &= LogEventWithParameters(eventName + amountOfSplitEvents, dict, isTimed);
        
        return success;
    }
    
    [DllImport("__Internal")]
	private static extern bool _EndTimedFlurryEventWithParameters(string eventName, string parameters);
    
    [DllImport("__Internal")]
	private static extern bool _EndTimedFlurryEvent(string eventName);
    
	public bool EndTimedEvent(string eventName)
    {
        return _EndTimedFlurryEvent(eventName);
    }
    
	public bool EndTimedEvent(string eventName, Dictionary<string, string> parameters)
    {
        if (parameters != null)
            return _EndTimedFlurryEventWithParameters(eventName, DictionaryToString(parameters));

        return _EndTimedFlurryEvent(eventName);
    }
    
	private string DictionaryToString(Dictionary<string, string> dict)
    {
        var list = new List<string>();
        foreach (var param in dict)
        {
            list.Add(string.Format("{0}||{1}", param.Key, param.Value));
        }

        return string.Join("|||", list.ToArray());
    }
    
    [DllImport("__Internal")]
	private static extern void _SetAge(int age);

	public void SetAge(int age)
	{
        _SetAge(age);
	}
    
    [DllImport("__Internal")]
	private static extern void _SetGender(string gender);
    
	public void SetGenderFemale(bool isFemale)
	{
        _SetGender(isFemale ? "f" : "m");
	}
	#endif
}
