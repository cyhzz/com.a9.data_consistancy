using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Com.A9.DataConsistancy
{

    public class PlayerPrefsV2 : MonoBehaviour
    {
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public static float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
    }

}