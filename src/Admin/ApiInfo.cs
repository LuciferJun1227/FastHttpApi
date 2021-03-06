﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Admin
{

    class UrlInfo : IComparable
    {
        public UrlInfo(ActionHandler handler)
        {
            Url = handler.SourceUrl;
            Http = new HttpInvoke();
            Http.Build(handler);
            WebSocket = new WebSocketInvoke();
            WebSocket.Build(handler);
            Remark = handler.Remark;
            Handler = handler;
        }

        internal ActionHandler Handler { get; set; }

        public string Remark { get; set; }

        public string Url { get; set; }

        public HttpInvoke Http { get; set; }

        public WebSocketInvoke WebSocket { get; set; }

        public int Compare(UrlInfo x, UrlInfo y)
        {
            return x.Url.CompareTo(y.Url);
        }

        public int CompareTo(object obj)
        {
            return this.Url.CompareTo(((UrlInfo)obj).Url);
        }
    }

    abstract class InvokeInfo
    {
        public string Url { get; set; }

        public string Body { get; set; }

        public string Response { get; set; }

        public abstract void Build(ActionHandler handler);


    }

    class HttpInvoke : InvokeInfo
    {
        public override void Build(ActionHandler handler)
        {
            Url = handler.SourceUrl;
            int k = 0;
            foreach (ParameterBinder item in handler.Parameters)
            {
                if (!item.DataParameter)
                    continue;
                ParameterInfo pi = item.GetInfo();
                if (pi.IsBody)
                {
                    Body = Newtonsoft.Json.JsonConvert.SerializeObject(pi.Value);
                    if (pi.Value is IEnumerable && pi.Value.GetType().IsGenericType)
                    {
                        try
                        {
                            System.Collections.ArrayList list = new ArrayList();
                            Type[] subtype = pi.Value.GetType().GetGenericArguments();
                            list.Add(Activator.CreateInstance(subtype[0]));
                            list.Add(Activator.CreateInstance(subtype[0]));
                            Body = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                        }
                        catch
                        { }

                    }
                }
                else
                {
                    if (k == 0)
                    {
                        Url += "?";
                    }
                    else
                    {
                        Url += "&";
                    }
                    Url += pi.Name + "=" + pi.Value.ToString();
                    k++;
                }
            }
            Response = Newtonsoft.Json.JsonConvert.SerializeObject(new ActionResult());
        }
    }

    class WebSocketInvoke : InvokeInfo
    {
        public override void Build(ActionHandler handler)
        {
            Url = handler.SourceUrl;
            Dictionary<string, object> mParams = new Dictionary<string, object>();
            foreach (ParameterBinder item in handler.Parameters)
            {
                if (item.DataParameter)
                {
                    ParameterInfo pi = item.GetInfo();

                    if (pi.IsBody)
                    {
                        mParams["body"] = pi.Value;
                        if (pi.Value is IEnumerable && pi.Value.GetType().IsGenericType)
                        {
                            try
                            {
                                System.Collections.ArrayList list = new ArrayList();
                                Type[] subtype = pi.Value.GetType().GetGenericArguments();
                                list.Add(Activator.CreateInstance(subtype[0]));
                                list.Add(Activator.CreateInstance(subtype[0]));
                                mParams["body"] = list;
                            }
                            catch
                            { }

                        }
                    }
                    else
                    {
                        mParams[pi.Name] = pi.Value;
                    }
                }
            }
            Dictionary<string, object> msg = new Dictionary<string, object>();
            msg["url"] = handler.SourceUrl;
            msg["params"] = mParams;
            Body = Newtonsoft.Json.JsonConvert.SerializeObject(msg);
            Response = Newtonsoft.Json.JsonConvert.SerializeObject(new ActionResult());
        }
    }

    public class ParameterInfo
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public bool IsBody { get; set; }

        public Type Type { get; set; }

        public override string ToString()
        {
            string value = Newtonsoft.Json.JsonConvert.SerializeObject(Value);
            if (Value is IEnumerable && Value.GetType().IsGenericType)
            {
                try
                {
                    System.Collections.ArrayList list = new ArrayList();
                    Type[] subtype = Value.GetType().GetGenericArguments();
                    list.Add(Activator.CreateInstance(subtype[0]));
                    list.Add(Activator.CreateInstance(subtype[0]));
                    value = Newtonsoft.Json.JsonConvert.SerializeObject(list);
                }
                catch
                { }
            }
            return value.ToString();
        }

    }
}
