using System;
using System.Collections;
using System.Collections.Generic;
using Com.A9.A9019;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
namespace Com.A9.DataConsistancy
{
    public class PlayerAccount
    {
        public string guid;
        public string pwd;
    }

    public class RemoteStrategy : IOStrategy
    {
        public PlayerData player_data;
        public string save_address;
        public string load_address;

        public RemoteStrategy(string save_address, string load_address, Action<PlayerData> OnSucc, Action OnFail)
        {
            this.save_address = save_address;
            this.load_address = load_address;

            var l = PlayerPrefsV2.GetString("LastLogin", "");
            if (string.IsNullOrEmpty(l))
            {
                OnFail?.Invoke();
                Debug.LogError("LastLogin not in playerprefs");
                return;
            }

            var last = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerAccount>(l);

            if (string.IsNullOrEmpty(last.guid))
            {
                OnFail?.Invoke();
                Debug.LogError("LastLogin info not in playerprefs");
                return;
            }

            Debug.Log($"ResourceManager Sending LastLogin {last.guid} to NetworkManager");

            NetworkManager.instance.SendRequest(load_address, new
            {
                guid = last.guid,
            },
            true,
            (json) =>
            {
                if (json == "0")
                {
                    OnFail?.Invoke();
                }
                else
                {
                    var st = new Newtonsoft.Json.JsonSerializerSettings();
                    st.Converters.Add(new PlayerDataConverter());

                    player_data = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerData>(json, st);
                    if (player_data == null)
                    {
                        OnFail?.Invoke();
                        return;
                    }
                    OnSucc?.Invoke(player_data);
                }
            },

            () =>
            {
                OnFail?.Invoke();
            });
        }

        void AddDataEntry<T>(DataEntry<T> data)
        {
            player_data.data_entries.Add(data);
        }

        public void FetchEntryExist<T>(DataEntry<T> dt, Action<T> OnSucc = null)
        {
            if (player_data.data_entries == null)
                player_data.data_entries = new List<DataEntry>();

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
            NetworkManager.instance.SendRequest(save_address, player_data, false, (json) =>
            {
                Debug.Log("ResourceManager save remote data success");
            },
            () =>
            {
                Debug.LogError("ResourceManager save remote data failed");
            });
        }

        public void DeletePlayerData()
        {
            NetworkManager.instance.SendRequest(save_address, new
            {
                guid = player_data.guid,
                operation = "delete"
            }, false, (json) =>
            {
                Debug.Log("ResourceManager delete remote data success");
            },
            () =>
            {
                Debug.LogError("ResourceManager delete remote data failed");
            });
        }
    }
}

