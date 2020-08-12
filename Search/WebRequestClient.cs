using JapanSaber.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace JapanSaber.Search
{
    /// <summary>
    /// WebRequest、Getのみ対応
    /// </summary>
    public sealed class WebRequestClient : MonoBehaviour
    {
        private static WebRequestClient _Instance;
        private readonly List<IWebRequestContext> SearchQueue = new List<IWebRequestContext>();
        private readonly List<IWebRequestContext> ActiveContexts = new List<IWebRequestContext>();
    
        private readonly object LockObject = new object();
        private readonly int MaxActiveCount = 3;

        public static WebRequestClient Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new GameObject("WebRequestClient").AddComponent<WebRequestClient>();
                }
                return _Instance;
            }
        }

        /// <summary>
        /// リクエストキューに追加
        /// </summary>
        /// <param name="context"></param>
        public void EnqueueRequest(IWebRequestContext context)
        {
            lock (LockObject)
            {
                if (!SearchQueue.Contains(context))
                {
                    if (ActiveContexts.Count >= MaxActiveCount)
                    {
                        // 待ちに追加
                        SearchQueue.Add(context);
                    }
                    else
                    {
                        // 即実行
                        ActiveContexts.Add(context);
                        StartCoroutine(Progress(context));
                    }
                }
            }
        }

        private void OnFinished(IWebRequestContext finished)
        {
            lock (LockObject)
            {
                ActiveContexts.Remove(finished);
                if (SearchQueue.Count > 0 && ActiveContexts.Count < MaxActiveCount)
                {
                    // キューから実行
                    var first = SearchQueue.First();
                    SearchQueue.Remove(first);

                    ActiveContexts.Add(first);
                    StartCoroutine(Progress(first));
                }
            }
        }

        private IEnumerator Progress(IWebRequestContext context)
        {
            Logger.Debug("SendRequest()");

            UnityWebRequest request = null;
            try
            {
                Logger.Debug(context.Url);
            }
            catch { }

            if (context.Url != null)
            {
                request = UnityWebRequest.Get(context.Url);
                request.SetRequestHeader("user-agent", $"JSaberClient/{Plugin.Version}");
                var asyncOperation = request.SendWebRequest();

                while (true)
                {
                    // VRは120fps出るのでフレーム単位は使用しない
                    // 0.075s = 13.33fps
                    yield return new WaitForSeconds(0.075f);

                    if (context.Token != null && context.Token.IsCancellationRequested)
                    {
                        try
                        {
                            context.OnCancelling?.Invoke();
                        }
                        catch { }
                        throw new OperationCanceledException();
                    }

                    try
                    {
                        context.OnProgress?.Invoke(asyncOperation.progress);
                    }
                    catch { }

                    if (asyncOperation.isDone)
                    {
                        break;
                    }
                }
            }

            OnFinished(context);

            try
            {
                if (request == null)
                {
                    context.OnError(-1, null);
                }
                else if (request.isNetworkError || request.isHttpError)
                {
                    context.OnError?.Invoke(request.responseCode, request.error);
                }
                else
                {
                    context.OnSuccess?.Invoke(request.responseCode, request.downloadHandler.data);
                }
            }
            catch (Exception ex)
            {
                Logger.IPALogger.Error(ex);
            }
        }
    }

    public interface IWebRequestContext
    {
        CancellationToken Token { get; }
        string Url { get; }
        Action<long, byte[]> OnSuccess { get; }
        Action<long, string> OnError { get; }
        Action<float> OnProgress { get; }
        Action OnCancelling { get; }
    }

    public class DownloadContext : IWebRequestContext
    {
        public DownloadContext(string url)
        {
            this.Url = url;
        }
        public DownloadContext(string url, CancellationToken token)
        {
            this.Url = url;
            this.Token = token;
        }

        public CancellationToken Token { get; set; }
        public string Url { get; private set; }

        public Action<long, byte[]> OnSuccess { get; set; }

        public Action<long, string> OnError { get; set; }
        public Action<float> OnProgress { get; set; }
        public Action OnCancelling { get; set; }
    }
}