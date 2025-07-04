using System;
using UnityEngine;

namespace Backend
{
    [Serializable]
    public class ResponseMessage<T>
    {
        public T _entity;
        public bool _status;
        public int _statusCode;
        public string _message;
        public long _timeStamp;
        public string _payload;
        public RequestMessage _request;

        public override string ToString()
        {
            return $"[ResponseMessage] StatusCode={_statusCode}, Status={_status}, Message={_message}, Entity={_entity}";
        }

        // Helper to parse raw JSON into ResponseMessage<T>
        public static ResponseMessage<T> FromJson(string json, RequestMessage request = null)
        {
            try
            {
                // Deserialize JSON into BackendResponse<T> wrapper
                BackendResponse<T> backendResp = JsonUtility.FromJson<BackendResponse<T>>(json);

                ResponseMessage<T> resp = new ResponseMessage<T>();
                resp._entity = backendResp.data;
                resp._status = backendResp.status;
                resp._statusCode = backendResp.statusCode;
                resp._message = backendResp.message;
                resp._timeStamp = backendResp.timeStamp;
                resp._payload = backendResp.payload;
                resp._request = request;

                return resp;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse ResponseMessage<{typeof(T).Name}>: {ex.Message}");
                return null;
            }
        }
    }

    // This matches your backend response envelope
    [Serializable]
    public class BackendResponse<T>
    {
        public bool status;
        public int statusCode;
        public string message;
        public long timeStamp;
        public string payload;
        public T data;
    }
}
