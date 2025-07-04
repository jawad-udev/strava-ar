using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Backend
{
	[Serializable]
	// Should have used builder pattern to construct objects of this class... Meh...
	public class RequestMessage
	{
		public const string KEY_PAYLOAD = "_payload";
		private const string KEY_DEVICE_ID = "deviceId";
		private const string KEY_DEVICE_VERSION = "X-Forwarded-Version";
        private const string KEY_USER_AUTH = "Authorization";
        private const string KEY_USER_VALUE = "Bearer";

        public enum RequestType : int
		{
			PUT = 		0x0001 << 0,
			GET = 		0x0001 << 1,
			POST =		0x0001 << 2,
			DELETE = 	0x0001 << 3
		}


		private static Dictionary<string, string> defaultHeaders;
		public static Dictionary<string, string> _defaultHeaders
		{
			get
			{

				defaultHeaders = new Dictionary<string, string>();
				defaultHeaders[KEY_DEVICE_ID] = SystemInfo.deviceUniqueIdentifier;
				defaultHeaders["Language"] = PlayerPrefs.GetString("Language");
				defaultHeaders[KEY_DEVICE_VERSION] = "45";
				return new Dictionary<string, string>(defaultHeaders);
			}
		}
		

        private static Dictionary<string, string> logoutHeaders;
        public static Dictionary<string, string> _logoutHeaders
        {
			get
			{
				logoutHeaders = new Dictionary<string, string>();
				//logoutHeaders[KEY_USER_AUTH] = KEY_USER_VALUE + " " + Services.PlayerService._player.token.Value;
				logoutHeaders["Language"] = PlayerPrefs.GetString("Language");
				return new Dictionary<string, string>(logoutHeaders);
			}
        }

        private static Dictionary<string, string> tokenHeaders;
        public static Dictionary<string, string> _tokenHeaders
        {
			get
			{
				tokenHeaders = new Dictionary<string, string>();
				//tokenHeaders[KEY_USER_AUTH] = KEY_USER_VALUE + " " + Services.PlayerService._player.token.Value;
				if(Services.PlayerService._player!=null)
				{
					//tokenHeaders["Language"] = Services.PlayerService.GetActiveLanguage();//PlayerPrefs.GetString("Language");
				}
				else{
					tokenHeaders["Language"] = PlayerPrefs.GetString("Language");
				}

				return new Dictionary<string, string>(tokenHeaders);
			}
        }

        public string _body;
		public string _payload;
		public string _requestPath;
		public RequestType _requestType;
		public Dictionary<string, string> _headers;
		public Dictionary<string, string> _requestParameters = new Dictionary<string, string>();


		/* public override string ToString ()
		{
			return string.Format ("[RequestMessage] _requestPath = {0} headers = {1} parameters = {2} body = {3}", _requestPath,  JsonCustomSerializer.Serialize(_headers),
				JsonCustomSerializer.Serialize(_requestParameters), _body);
		} */
	}
}

