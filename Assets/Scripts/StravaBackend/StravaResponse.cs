using System;
using System.Collections.Generic;

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
    public float distance;
    public int moving_time;
    public string start_date;
    public StravaMap map;
}

[Serializable]
public class StravaMap
{
    public string summary_polyline;
}

[Serializable]
public class StravaActivityWrapper
{
    public List<StravaActivity> list;
}

