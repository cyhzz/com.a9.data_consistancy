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

        void Start()
        {
            getter = new UniqueIDGetter();
        }

        public void GetUniqueID()
        {
            OnFetchStart?.Invoke();
            getter.GetUniqueID((c) =>
            {
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

