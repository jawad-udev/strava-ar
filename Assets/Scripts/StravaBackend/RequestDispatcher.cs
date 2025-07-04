using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

namespace Backend
{
    public class RequestDispatcher : MonoBehaviour
    {
        private RequestMessage _request;
        private Action<object> _responseListener;
        private object _response;

        public void Request<T>(RequestMessage request, Action<T> callback)
        {
            _request = request;
            _responseListener = (obj) =>
            {
                if (obj is T typedResponse)
                    callback?.Invoke(typedResponse);
                else
                    Debug.LogError($"Response type mismatch: Expected {typeof(T)}, got {obj.GetType()}");
            };

            PrepareUrlWithParameters(request);

            new Thread(() =>
            {
                try
                {
                    var webRequest = WebRequest.Create(request._requestPath);
                    foreach (var header in request._headers)
                        webRequest.Headers[header.Key] = header.Value;

                    webRequest.ContentType = "application/json";
                    webRequest.Method = request._requestType.ToString();
                    webRequest.Timeout = 10000;

                    if ((request._requestType & (RequestMessage.RequestType.POST | RequestMessage.RequestType.PUT)) != 0
                        && !string.IsNullOrEmpty(request._body))
                    {
                        using var dataStream = webRequest.GetRequestStream();
                        var bytes = System.Text.Encoding.UTF8.GetBytes(request._body);
                        dataStream.Write(bytes, 0, bytes.Length);
                    }

                    ServicePointManager.ServerCertificateValidationCallback = CertificateValidation;

                    using var responseStream = webRequest.GetResponse().GetResponseStream();
                    using var reader = new System.IO.StreamReader(responseStream);
                    string responseText = reader.ReadToEnd();

                    // Deserialize responseText directly into T (Strava response model)
                    _response = JsonUtility.FromJson<T>(responseText);
                }
                catch (Exception e)
                {
                    Debug.LogError($"RequestDispatcher Exception: {e.Message}");
                    _response = default(T);
                }
            }).Start();
        }

        private void PrepareUrlWithParameters(RequestMessage request)
        {
            if (request._requestParameters.Count > 0)
            {
                var query = string.Join("&", request._requestParameters.Select(kv => $"{kv.Key}={kv.Value}"));
                request._requestPath += (request._requestPath.Contains("?") ? "&" : "?") + query;
            }
        }

        private bool CertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslErrors)
        {
            if (sslErrors == SslPolicyErrors.None) return true;
            foreach (var status in chain.ChainStatus)
            {
                if (status.Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = TimeSpan.FromMinutes(1);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    return chain.Build((X509Certificate2)certificate);
                }
            }
            return false;
        }

        private void Update()
        {
            if (_response != null)
            {
                _responseListener?.Invoke(_response);
                Destroy(gameObject);
            }
        }
    }
}
