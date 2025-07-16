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
    public long id;
    public string username;
    public string firstname;
    public string lastname;
    public string city;
    public string country;
    public string sex;
    public int follower_count;
    public int friend_count;
    public string profile; // Profile picture URL
    public string email;
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
public class StravaZone
{
    [JsonProperty("score")]
    public int score;

    [JsonProperty("distribution_buckets")]
    public DistributionBuckets distributionBuckets;

    [JsonProperty("type")]
    public string type;
}

[Serializable]
public class DistributionBuckets
{
    [JsonProperty("max")]
    public int max;

    [JsonProperty("min")]
    public int min;

    [JsonProperty("type")]
    public string type;

    [JsonProperty("buckets")]
    public List<int> buckets;
}

[Serializable]
public class StravaPhoto
{
    [JsonProperty("id")]
    public long id;

    [JsonProperty("unique_id")]
    public string uniqueId;

    [JsonProperty("urls")]
    public Dictionary<string, string> urls;  // keys: 100, 600, etc.

    [JsonProperty("source")]
    public int source;

    [JsonProperty("created_at")]
    public string createdAt;
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

[Serializable]
public class StravaStats
{
    [JsonProperty("biggest_ride_distance")]
    public float biggest_ride_distance;

    [JsonProperty("biggest_climb_elevation_gain")]
    public float biggest_climb_elevation_gain;

    [JsonProperty("recent_ride_totals")]
    public StravaTotals recent_ride_totals;

    [JsonProperty("ytd_ride_totals")]
    public StravaTotals ytd_ride_totals;

    [JsonProperty("all_ride_totals")]
    public StravaTotals all_ride_totals;
}

[Serializable]
public class StravaTotals
{
    public int count;
    public float distance;
    public int moving_time;
    public float elevation_gain;
}

[Serializable]
public class StravaUserZones
{
    [JsonProperty("heart_rate")]
    public List<HeartRateZone> heart_rate;

    [Serializable]
    public class HeartRateZone
    {
        public int min;
        public int max;
    }
}
