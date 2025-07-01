using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using UniRx;

public class CommonPopup : GameMonoBehaviour
{
    public Button closeButton, leftButton, rightButton;
    public TextMeshProUGUI titleText, descriptionText, leftButtonText, rightButtonText;
    Action leftButtonListener, rightButtonListener;

    public IReactiveProperty<string> title = new ReactiveProperty<string>();
    public IReactiveProperty<string> description = new ReactiveProperty<string>();
    public IReactiveProperty<string> leftButtonContent = new ReactiveProperty<string>();
    public IReactiveProperty<string> rightButtonContent = new ReactiveProperty<string>();

    private void Awake()
    {
        closeButton.onClick.AsObservable().Subscribe(x =>
        {
            Services.AudioService.PlayUIClick();
            Services.BackLogService.CloseLastUI();
        });

        rightButton.onClick.AsObservable().Subscribe(x => {
            rightButtonListener?.Invoke();
            Services.AudioService.PlayUIClick();
            Services.BackLogService.CloseLastUI();
        });

        leftButton.onClick.AsObservable().Subscribe(x => {
            leftButtonListener?.Invoke();
            Services.AudioService.PlayUIClick();
            Services.BackLogService.CloseLastUI();
        });

        title.AsObservable().SubscribeToText(titleText);
        description.AsObservable().SubscribeToText(descriptionText);
        leftButtonContent.AsObservable().SubscribeToText(leftButtonText);
        rightButtonContent.AsObservable().SubscribeToText(rightButtonText);
    }

    public void OpenPopup(string title, string description, string leftButtonContent, string rightButtonContent, Action leftButtonCallback, Action rightButtonCallback)
    {
        leftButtonListener = leftButtonCallback;
        rightButtonListener = rightButtonCallback;

        print(rightButtonContent);
        print(leftButtonContent);

        this.title.Value = title;
        this.description.Value = description;
        this.leftButtonContent.Value = leftButtonContent;
        this.rightButtonContent.Value = rightButtonContent;

        this.Show(BacklogType.KeepPreviousScreen);
    }
}
