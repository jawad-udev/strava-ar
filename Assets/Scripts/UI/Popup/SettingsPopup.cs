using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UniRx;

public class SettingsPopup : GameMonoBehaviour
{
    public Button musicButton, soundButton, vibrationButton, closeButton, clearData, showDebugButton;

    //  Add new UI fields
    public Dropdown sexDropdown;
    public Dropdown buildDropdown;
    public Dropdown modelTypeDropdown;

    void Awake()
    {
        closeButton.onClick.AsObservable().Subscribe(x => OnClickCloseButton());
        clearData.onClick.AsObservable().Subscribe(x => OnClickClearDataButton());
        showDebugButton.onClick.AsObservable().Subscribe(x => OnClickShowDebugButton());

        //  Dropdown listeners
        sexDropdown.onValueChanged.AddListener(OnSexChanged);
        buildDropdown.onValueChanged.AddListener(OnBuildChanged);
        modelTypeDropdown.onValueChanged.AddListener(OnModelTypeChanged);

        LoadSettings(); // Load previous choices
    }

    void OnClickCloseButton()
    {
        Services.BackLogService.CloseLastUI();
        Services.AudioService.PlayUIClick();
    }

    void OnClickClearDataButton()
    {
        Services.instance.clearPrefs = true;
        clearData.interactable = false;
        Services.AudioService.PlayUIClick();
    }

    void OnClickShowDebugButton()
    {
        Services.AudioService.PlayUIClick();
    }

    // Dropdown callbacks
    void OnSexChanged(int index)
    {
        PlayerPrefs.SetInt("model_sex", index); // 0 = Male, 1 = Female
        Debug.Log("Sex changed to: " + sexDropdown.options[index].text);
    }

    void OnBuildChanged(int index)
    {
        PlayerPrefs.SetInt("model_build", index); // 0 = Solid, 1 = Slim
        Debug.Log("Build changed to: " + buildDropdown.options[index].text);
    }

    void OnModelTypeChanged(int index)
    {
        PlayerPrefs.SetInt("model_type", index); // 0 = Bike, 1 = Runner
        Debug.Log("Model type changed to: " + modelTypeDropdown.options[index].text);
    }

    void LoadSettings()
    {
        sexDropdown.value = PlayerPrefs.GetInt("model_sex", 0);
        buildDropdown.value = PlayerPrefs.GetInt("model_build", 0);
        modelTypeDropdown.value = PlayerPrefs.GetInt("model_type", 0);
    }
}
