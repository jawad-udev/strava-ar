using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ARProfileCard : MonoBehaviour
{
    public RawImage profileImage;
    public TextMeshProUGUI nameText,userNameText, locationText, followersText;

    public void SetupAthleteInfo(StravaAthlete athlete)
    {
        nameText.text = $"{athlete.firstname} {athlete.lastname}";
        locationText.text = $"{athlete.city}, {athlete.country}";
        followersText.text = athlete.sex == "M" ? "Male" : "Female";
        StartCoroutine(LoadProfileImage(athlete.profile));
    }

    IEnumerator LoadProfileImage(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            profileImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
}
