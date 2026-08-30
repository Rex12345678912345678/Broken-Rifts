#if UNITY_EDITOR

using Chimera.Library.Components.Interfaces;

namespace ActionTreeEditor.Runtime
{
    public class NullPlayerPrefsService : IStorageService
    {
        private StorageUnityImpl m_unityImpl = new StorageUnityImpl();

        public bool SetInt(string key, int value)
        {
            // no-op
            return false;
        }

        public int GetInt(string key)
        {
            return m_unityImpl.GetInt(key);
        }

        public int GetInt(string key, int standardValue)
        {
            return m_unityImpl.GetInt(key, standardValue);
        }

        public bool SetFloat(string key, float value)
        {
            // no-op
            return false;
        }

        public float GetFloat(string key)
        {
            return m_unityImpl.GetFloat(key);
        }

        public float GetFloat(string key, float standardValue)
        {
            return m_unityImpl.GetFloat(key, standardValue);
        }

        public bool SetString(string key, string value)
        {
            // no-op
            return false;
        }

        public string GetString(string key)
        {
            return m_unityImpl.GetString(key);
        }

        public string GetString(string key, string standardValue)
        {
            return m_unityImpl.GetString(key, standardValue);
        }

        public bool HasKey(string key)
        {
            return m_unityImpl.HasKey(key);
        }

        public bool DeleteKey(string key)
        {
            // no-op
            return false;
        }

        public bool DeleteAll()
        {
            // no-op
            return false;
        }

        public bool Save()
        {
            // no-op
            return false;
        }
    }
}
#endif