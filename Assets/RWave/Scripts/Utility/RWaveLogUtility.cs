using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
namespace RWave.Utility
{
    public static class RWaveLogUtility
    {
        private const string RWAVE_RUNTIME_LOG_ENABLE_SYMBOL = "ENABLE_RWAVE_LOG_SYMBOL";
        private const string RWAVE_HEADER = "RWAVE:";
#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void Log(object message)
        {
            Debug.Log($"{RWAVE_HEADER}{message}");
        }
        
#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void Log(object message,Object context)
        {
            Debug.Log($"{RWAVE_HEADER}{message}",context);
        }
       
#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void LogWarning(object message)
        {
            Debug.LogWarning($"{RWAVE_HEADER}{message}");
        }
        
#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void LogWarning(object message,Object context)
        {
            Debug.LogWarning($"{RWAVE_HEADER}{message}",context);
        }
        
#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void LogError(object message)
        {
            Debug.LogError($"{RWAVE_HEADER}{message}");
        }

#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void LogError(object message,Object context)
        {
            Debug.LogError($"{RWAVE_HEADER}{message}",context);
        }
        
#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void LogAssertion(object message)
        {
            Debug.LogAssertion($"{RWAVE_HEADER}{message}");
        }
        
#if DEBUG && DISABLE_RWAVE_LOG
        [Conditional(RWAVE_RUNTIME_LOG_ENABLE_SYMBOL)]
#endif
        public static void LogAssertion(object message,Object context)
        {
            Debug.LogAssertion($"{RWAVE_HEADER}{message}",context);
        }

        [Conditional("ENABLE_R_WAVE_AUDIO_CLIP_CONTAINER_LOG")]
        public static void DrawAudioClipContainerLog(string text) { Log(text); }
    }
}