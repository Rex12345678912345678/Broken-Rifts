using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ActionTreeEditor.Editor
{
    public class ActionTreePreviewLogger : ILogHandler
    {
        public ILogHandler OriginalHandler { get; private set; }

        private static HashSet<string> BlockedLogs = new()
        {
            "FX_Smoke_Inactive", 
            "GetTimingService", 
            "Next id < 0"
        };
        
        private static HashSet<string> BlockedEvents = new()
        {
            "Awake", 
            "OnDisable", 
            "OnDestroy", 
            "OnApplicationPause"
        };
        
        public ActionTreePreviewLogger(ILogHandler originalHandler)
        {
            OriginalHandler = originalHandler;
        }
        
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            if (args.Any(o => o is string s && BlockedLogs.Any(blocked => s.Contains(blocked))))
                return;
            
            if (logType == LogType.Log)
            {
                var trace = new StackTrace();
                var isTreeLog = trace.GetFrames()?
                    .Any(f => f.HasMethod() && f.GetMethod().DeclaringType?.Name == "ActionTree")
                        ?? false;
                
                if (!isTreeLog)
                    return;
            }

            OriginalHandler.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            var trace = new StackTrace(exception);
            
            var ignore = trace.GetFrames()?
                .Select(f => f.GetMethod())
                .Any(m => m != null && BlockedEvents.Contains(m.Name)) 
                         ?? false;
            
            if (ignore)
                return;
            
            OriginalHandler.LogException(exception, context);
        }
    }
}