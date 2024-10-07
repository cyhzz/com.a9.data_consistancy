using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Com.A9.A9019;
using Com.A9.FileReader;
using Com.A9.Singleton;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace Com.A9.DataConsistancy
{
    class DataEntryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DataEntry).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            var ar = (JArray)obj["data"];

            Type list_type = Type.GetType((string)obj["type"]);
            Type myType = list_type.GetGenericArguments()[0];

            var new_etr = typeof(DataEntry<>).MakeGenericType(list_type);
            var ob = Activator.CreateInstance(new_etr) as DataEntry;


            var constructedListType = typeof(List<>).MakeGenericType(myType);
            var item = Activator.CreateInstance(constructedListType);
            var lst = item as IList;

            foreach (JToken token in ar)
            {
                var curTile = JsonConvert.DeserializeObject(token.ToString(), myType);
                lst.Add(curTile);
            }

            new_etr.GetField("id").SetValue(ob, (int)obj["id"]);
            new_etr.GetField("type").SetValue(ob, (string)obj["type"]);
            new_etr.GetField("data").SetValue(ob, lst);
            // serializer.Populate(obj.CreateReader(), ob);
            return ob;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

        }
    }

    class PlayerDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(PlayerData).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            string guid = (string)obj["guid"];

            bool initialized = false;
            if (obj["initialized"] != null)
                initialized = (bool)obj["initialized"];

            var item = new PlayerData();
            item.guid = guid;
            item.initialized = initialized;
            item.data_entries = new List<DataEntry>();

            var ar = (JArray)obj["data_entries"];
            if (ar != null)
            {
                foreach (JToken token in ar)
                {
                    JsonSerializerSettings jss = new JsonSerializerSettings();
                    jss.Converters.Add(new DataEntryConverter());

                    var curTile = JsonConvert.DeserializeObject<DataEntry>(token.ToString(), jss);
                    item.data_entries.Add(curTile);
                    Debug.Log("add_entry");
                }
            }


            // serializer.Populate(obj.CreateReader(), item);
            return item;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

        }
    }

    public class DataEntry
    {
        public int id;
        public string type;
    }

    public class DataEntry<T> : DataEntry
    {
        public T data;
        public DataEntry() { }
        public DataEntry(int id, T data)
        {
            this.id = id;
            this.data = data;
            this.type = data.GetType().FullName;
        }

        public DataEntry(int id, T data, string load_address)
        {
            this.id = id;
            this.data = data;
            this.type = data.GetType().FullName;
        }

        public DataEntry(int id, T data, string load_address, string save_address)
        {
            this.id = id;
            this.data = data;
            this.type = data.GetType().FullName;
        }
    }

    public class PlayerData
    {
        public string guid;
        public List<DataEntry> data_entries = new List<DataEntry>();
        public bool initialized;

        public static DateTime IdCardtoDate(string cardno)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cardno) || cardno.Length != 18)
                {
                    return new DateTime(2099, 1, 1);
                }
                else
                {
                    int year = Convert.ToInt32(cardno.Substring(6, 4));
                    int month = Convert.ToInt32(cardno.Substring(10, 2));
                    int day = Convert.ToInt32(cardno.Substring(12, 2));
                    return new DateTime(year, month, day);
                }
            }
            catch
            {
                return new DateTime(2099, 1, 1);
            }
        }

        public PlayerData() { }
        public PlayerData(string guid)
        {
            this.guid = guid;
        }
    }

    public class FetchRequest<T> : FetchRequest
    {
        public DataEntry<T> dt;
        public Action<T> OnSucc;

        public override void Fetch()
        {
            ResourceManager.instance.io_strategy.FetchEntryExist(dt, OnSucc);
            Debug.Log("Derived Fetch");
        }
    }

    public class SaveRequest { }

    public abstract class FetchRequest
    {
        public virtual void Fetch() { }
    }

    public class ResourceManager : Singleton<ResourceManager>
    {
        public PlayerData player_data = new PlayerData();
        public IOStrategy io_strategy;
        public string remote_load;
        public string remote_save;
        public UnityEvent OnTryRemoteFetch;
        public UnityEvent OnTryRemoteSucc;
        public UnityEvent OnTryRemoteFailed;

        public bool initialized;

        List<FetchRequest> fetch_requests = new List<FetchRequest>();
        List<SaveRequest> save_requests = new List<SaveRequest>();

        public void InitLocal()
        {
            io_strategy = new LocalStrategy(out player_data);
            initialized = true;
            OnTryRemoteSucc?.Invoke();
        }

        public void InitRemote()
        {
            OnTryRemoteFetch?.Invoke();
            io_strategy = new RemoteStrategy(remote_save, remote_load, (c) =>
            {
                player_data = c;
                initialized = true;
                Debug.Log("<color=#00FF00FF>Fetch Remote Success</color>");
                OnTryRemoteSucc?.Invoke();
            },
            () => { OnTryRemoteFailed?.Invoke(); Debug.LogError("Fetch Remote Failed"); });
        }

        public void FetchEntryExist<T>(DataEntry<T> dt, Action<T> OnSucc = null)
        {
            fetch_requests.Add(new FetchRequest<T> { dt = dt, OnSucc = OnSucc });
        }

        void Update()
        {
            if (initialized)
            {
                if (save_requests.Count > 0)
                {
                    io_strategy.SavePlayerData();
                    save_requests.Clear();
                }

                if (fetch_requests.Count > 0)
                {
                    foreach (var req in fetch_requests)
                    {
                        req.Fetch();
                    }
                    fetch_requests.Clear();
                }
            }
        }

        public void SavePlayerData()
        {
            save_requests.Add(new SaveRequest());
        }

        public void FetchEntryNoSave<T>(string load_address, object req = null, Action<T> OnSucc = null)
        {
            StartCoroutine(FetchNoSave(load_address, req, OnSucc));
        }

        IEnumerator FetchNoSave<T>(string load_address, object req, Action<T> callback)
        {
            yield return null;
            NetworkManager.instance.SendRequest(load_address, req, false, (json) =>
            {
                var lst = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                callback?.Invoke(lst);
            });
        }
    }
}


