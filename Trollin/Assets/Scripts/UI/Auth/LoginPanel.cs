using GameSparks.Api.Requests;
using GameSparks.Api.Responses;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour {

    public InputField UserNameInput;
    public InputField PasswordInput;
    public Button LoginButton;
    public Button RegisterButton;
    public Text ErrorMessageText;

    private SimpleAuthValidator mSimpleAuthValidator;

    private void Awake()
    {
        mSimpleAuthValidator = new SimpleAuthValidator();
        LoginButton.onClick.AddListener(Login);
        RegisterButton.onClick.AddListener(Register);
    }

    private void Register()
    {
        BlockInput();
        string usernameError = mSimpleAuthValidator.ValidateUsername(UserNameInput.text);
        string passwordError = mSimpleAuthValidator.ValidatePassword(PasswordInput.text);

        if(usernameError == null && passwordError == null)
        {
            RegistrationRequest request = new RegistrationRequest();
            request.SetUserName(UserNameInput.text);
            request.SetDisplayName(UserNameInput.text);
            request.SetPassword(PasswordInput.text);
            request.Send(OnRegistrationSuccess, OnRegistrationError);
        } else
        {
            ErrorMessageText.text = usernameError != null ? usernameError : passwordError;
            UnblockInput();
        }
    }

    private void OnRegistrationSuccess(RegistrationResponse obj)
    {
        Login();
    }

    private void OnRegistrationError(RegistrationResponse obj)
    {
        UnblockInput();
        ErrorMessageText.text = obj.Errors.JSON.ToString();
    }

    private void Login()
    {
        BlockInput();
        AuthenticationRequest request = new AuthenticationRequest();
        request.SetUserName(UserNameInput.text);
        request.SetPassword(PasswordInput.text);
        request.Send(OnLoginSuccess, OnLoginError);
    }

    private void OnLoginError(AuthenticationResponse obj)
    {
        UnblockInput();
        ErrorMessageText.text = obj.Errors.JSON.ToString();
    }

    private void OnLoginSuccess(AuthenticationResponse obj)
    {
        SceneManager.LoadScene(LoadingManager.LOBBY_SCENE);
    }

    private void BlockInput()
    {
        UserNameInput.interactable = false;
        PasswordInput.interactable = false;
        LoginButton.interactable = false;
        RegisterButton.interactable = false;
    }

    private void UnblockInput()
    {
        UserNameInput.interactable = true;
        PasswordInput.interactable = true;
        LoginButton.interactable = true;
        RegisterButton.interactable = true;
    }
}
