using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Com.A9.DataConsistancy
{
    public class UniqueIDGetterTest : MonoBehaviour
    {
        UniqueIDGetter getter;
        public Text txt;
        public UnityEvent OnFetchStart;
        public UnityEvent OnFetchEnd;
        public string code2open_id_address;

        void Start()
        {
            getter = new UniqueIDGetter(false, code2open_id_address);
        }

        public void GetUniqueID()
        {
            OnFetchStart?.Invoke();
            getter.GetUniqueID((c) =>
            {
                Debug.Log($"test shoudl be {c}");
                txt.text = c;
            }, (c) =>
            {
                txt.text = "fail";
            }, () =>
            {
                OnFetchEnd?.Invoke();
            });
        }
    }
}

