using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class StravaTokenResponse
{
    public string token_type;
    public string access_token;
    public string refresh_token;
    public int expires_at;
    public int expires_in;
    public StravaAthlete athlete;
}

[Serializable]
public class StravaAthlete
{
    public string username;
    public string firstname;
    public string lastname;
}

[Serializable]
public class StravaActivity
{
    public long id;
    public string name;
    public float distance;               // meters
    public int moving_time;             // seconds
    public float total_elevation_gain;  // meters
    public string start_date;
    public StravaMap map;
}

[Serializable]
public class StravaActivityDetail
{
    public long id;
    public string name;
    public float distance;
    public int moving_time;
    public float total_elevation_gain;
    public float average_heartrate;
    public float max_heartrate;

    [JsonProperty("laps")]
    public List<StravaLap> laps;
}

[Serializable]
public class StravaLap
{
    public long id;
    public string name;

    [JsonProperty("elapsed_time")]
    public int elapsedTime;

    [JsonProperty("moving_time")]
    public int movingTime;

    public float distance;

    [JsonProperty("start_date")]
    public string startDateUtc;

    [JsonProperty("start_date_local")]
    public string startDateLocal;

    [JsonProperty("average_speed")]
    public float averageSpeed;

    [JsonProperty("lap_index")]
    public int lapIndex;
}

[Serializable]
public class StravaMap
{
    public string summary_polyline;
}

[Serializable]
public class StravaActivityWrapper
{
    public List<StravaActivity> activities;
}

