using System;
using System.Collections;
using System.Collections.Generic;
using Com.A9.FileReader;
using Newtonsoft.Json;
using UnityEngine;

namespace Com.A9.DataConsistancy
{
    public interface IOStrategy
    {
        void FetchEntryExist<T>(DataEntry<T> dt, Action<T> OnSucc = null);
        void SavePlayerData();
    }

    public class LocalStrategy : IOStrategy
    {
        public PlayerData player_data;

        public LocalStrategy(out PlayerData sc)
        {
            var st = new JsonSerializerSettings();
            st.Converters.Add(new PlayerDataConverter());
            xmlReader.ReadJson<PlayerData>("player_data.json", out player_data, st);

            if (player_data == null)
            {
                player_data = new PlayerData(System.Guid.NewGuid().ToString());
                SavePlayerData();
            }

            sc = player_data;
        }

        void AddDataEntry<T>(DataEntry<T> data)
        {
            player_data.data_entries.Add(data);
        }

        public void FetchEntryExist<T>(DataEntry<T> dt, Action<T> OnSucc = null)
        {
            var m = player_data.data_entries.Find(c => c.id == dt.id) as DataEntry<T>;
            if (m == null)
            {
                AddDataEntry(dt);
                OnSucc?.Invoke(dt.data);
                return;
            }
            OnSucc?.Invoke(m.data);
        }

        public void SavePlayerData()
        {
            xmlReader.SaveAsJson<PlayerData>("player_data.json", player_data);
            Debug.Log("save local");
        }
    }

}