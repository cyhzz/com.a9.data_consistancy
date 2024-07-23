using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_WEBGL
using WeChatWASM;
#endif

namespace Com.A9.DataConsistancy
{
    public interface IUniqueIDGetter
    {
        void GetUniqueID(Action<string> succ = null, Action<string> fail = null, Action complete = null);
    }

    public class UniqueIDGetter : IUniqueIDGetter
    {
        GetUserInfoSuccessCallbackResult session;
        WXUserInfoResponse session2;
        string guid;
        public bool force_local;
        bool infoFlag = false;

#if UNITY_WEBGL
        public void GetUniqueID(Action<string> succ, Action<string> fail, Action complete = null)
        {
            WX.InitSDK((c) =>
            {
                GetWXCode(() =>
                {
                    if (session != null)
                    {
                        succ?.Invoke(session.userInfo.nickName);
                    }
                    else if (session2 != null)
                    {
                        succ?.Invoke(session2.userInfo.nickName);
                    }
                    else
                    {
                        fail?.Invoke("no session");
                    }
                    complete?.Invoke();
                });
            });
        }

        private void GetWXCode(Action complete)
        {
            Debug.Log("start get code..");
            // 1. 询问隐私协议授权情况
            WX.GetPrivacySetting(new GetPrivacySettingOption()
            {
                success = (res) =>
                {
                    Debug.Log("询问隐私协议成功");
                    if (res.needAuthorization)
                    {
                        WX.RequirePrivacyAuthorize(new RequirePrivacyAuthorizeOption()
                        {
                            success = (res) =>
                            {
                                Debug.Log("同意隐私协议：" + JsonUtility.ToJson(res, true));
                                this.GetScopeInfoSetting(complete);
                                this.infoFlag = true;
                            },
                            fail = (err) =>
                            {
                                Debug.Log("拒绝隐私协议：" + JsonUtility.ToJson(res, true));
                                complete?.Invoke();
                            },
                            complete = (res) =>
                            {
                                Debug.Log("询问隐私协议结束");
                            }
                        });
                    }
                    else
                    {
                        Debug.Log("无需询问隐私协议");
                    }
                },
                fail = (err) =>
                {
                    Debug.Log("询问隐私协议失败：" + JsonUtility.ToJson(err, true));
                    complete?.Invoke();
                },
                complete = (res) =>
                {
                    if (!this.infoFlag)
                    {
                        this.GetScopeInfoSetting(complete);
                        Debug.Log("询问隐私协议结束2");
                    }
                    else
                    {
                        Debug.Log("询问隐私协议结束3");
                    }
                }
            });
        }

        void GetScopeInfoSetting(Action complete)
        {
            // 询问用户信息授权情况
            WX.GetSetting(new GetSettingOption()
            {
                success = (res) =>
                {
                    Debug.Log("获取用户信息授权情况成功: " + JsonUtility.ToJson(res.authSetting, true));
                    // 判断用户信息的授权情况
                    if (!res.authSetting.ContainsKey("scope.userInfo") || !res.authSetting["scope.userInfo"])
                    {
                        this.CreateUserInfoButton(complete);
                    }
                    else
                    {
                        this.GetUserInfo(complete);
                    }
                },
                fail = (err) =>
                {
                    Debug.Log("获取用户信息授权情况失败：" + JsonUtility.ToJson(err, true));
                    complete?.Invoke();
                }
            });
        }

        void GetUserInfo(Action complete = null)
        {
            WX.GetUserInfo(new GetUserInfoOption()
            {
                lang = "zh_CN",
                success = (res) =>
                {
                    Debug.Log("获取用户信息成功(API): " + JsonUtility.ToJson(res.userInfo, true));
                    session = res;
                },
                fail = (err) =>
                {
                    Debug.Log("获取用户信息失败(API): " + JsonUtility.ToJson(err, true));
                },
                complete = (res) =>
                {
                    complete?.Invoke();
                }
            });
        }

        private void CreateUserInfoButton(Action complete = null)
        {
            Debug.Log("create userinfo button area");

            WXUserInfoButton btn = WX.CreateUserInfoButton(0, 0, Screen.width, Screen.height, "zh_CN", false);
            btn.OnTap((res) =>
            {
                Debug.Log("click userinfo btn: " + JsonUtility.ToJson(res, true));
                if (res.errCode == 0)
                {
                    Debug.Log("userinfo: " + JsonUtility.ToJson(res.userInfo, true));
                    session2 = res;
                }
                else
                {
                    Debug.Log("用户拒绝获取个人信息");
                }
                btn.Hide();
                Debug.Log("已隐藏热区");
                complete?.Invoke();
            });
        }

#else
        public IEnumerator GetUniqueID(Action<string> succ=null, Action<string> fail=null, Action complete = null)
        {
            if (string.IsNullOrEmpty(guid) == false)
            {
                succ?.Invoke(guid);
                complete?.Invoke();
                return;
            }
            guid = UnityEngine.SystemInfo.deviceUniqueIdentifier;
            succ?.Invoke(guid);
            complete?.Invoke();
        }
#endif
    }
}

