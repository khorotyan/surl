using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class AuthManager : MonoBehaviour
{
    private RefVars rv;

    private void Awake()
    {
        rv = transform.GetChild(0).GetComponent<RefVars>();

        // Signup
        rv.suUsernameIn.onEndEdit.AddListener(delegate { ValUsername(); });
        rv.suEmailIn.onEndEdit.AddListener(delegate { ValEmail(); });
        rv.suPassIn.onEndEdit.AddListener(delegate { ValPass(); });
        rv.suConfPassIn.onEndEdit.AddListener(delegate { ValPassConf(); });
        rv.suSignupB.onClick.AddListener(delegate { OnCreateAccountClick(); });
        rv.suSigninB.onClick.AddListener(delegate { OpenSignin(); });

        // Signin
        rv.siSigninB.onClick.AddListener(delegate { StartCoroutine(Signin()); });
        rv.siSignupB.onClick.AddListener(delegate { OpenSignup(); });
        rv.siSkipB.onClick.AddListener(delegate { });

        rv.siEmailIn.text = LoadManager.LoadEmail();
    }

    // Validate username
    private void ValUsername()
    {
        if (rv.suUsernameIn.text.Length < 4)
        {
            rv.suUsernameIn.text = "";
            rv.ShowInfo("Username is too short");
        }
    }

    // Validate signup email
    private void ValEmail()
    {
        string pattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

        if (!Regex.IsMatch(rv.suEmailIn.text, pattern))
        {
            rv.suEmailIn.text = "";
            rv.ShowInfo("Email is invalid");
        }
    }

    // Validate Password
    private void ValPass()
    {
        if (rv.suPassIn.text.Length < 6)
        {
            rv.suPassIn.text = "";       
            rv.ShowInfo("Password is too short");
        }
    }

    // Validate Password Confirmation
    private void ValPassConf()
    {
        if (rv.suPassIn.text != rv.suConfPassIn.text)
        {
            rv.suConfPassIn.text = "";
            rv.ShowInfo("Passwords do not match");
        }
    }

    // Check whether all the information is correct
    private void OnCreateAccountClick()
    {
        if (rv.suUsernameIn.text.Length == 0)
        {
            rv.ShowInfo("Please fill the username field");
        }
        else if (rv.suEmailIn.text.Length == 0)
        {
            rv.ShowInfo("Please fill the email field");
        }
        else if (rv.suPassIn.text.Length == 0 || rv.suConfPassIn.text.Length == 0)
        {
            rv.ShowInfo("Please fill the password fields");
        }
        else
        {
            StartCoroutine(Signup());
        }
    }

    // Open Signin page
    private void OpenSignin()
    {
        rv.signinPanel.SetActive(true);
        rv.signupPanel.SetActive(false);
    }

    // Open Signup page
    private void OpenSignup()
    {
        rv.signupPanel.SetActive(true);
        rv.signinPanel.SetActive(false);
    }

    private IEnumerator Signup()
    {
        WWWForm form = new WWWForm();

        form.AddField("Username", rv.suUsernameIn.text);
        form.AddField("Email", rv.suEmailIn.text);
        form.AddField("Password", rv.suPassIn.text);

        WWW www = new WWW(rv.reqURL + "/api/Users/Create", form.data);
        yield return www;

        if (www.error != null)
        {
            Debug.Log(www.error);

            if (www.text.Contains("Username already exists"))
            {
                rv.ShowInfo("Username already exists");
            }
            else
            {
                rv.ShowInfo("Connection error");
            }
        }
        else
        {
            string jsonData = www.text;

            User user = JsonUtility.FromJson<User>(jsonData);
            UserManager.userID = int.Parse(user.userID);
            UserManager.username = user.username;
            UserManager.token = user.token;
            SaveManager.SaveEmail(user.email);
            rv.suUsernameIn.text = "";
            rv.suEmailIn.text = "";
            rv.suPassIn.text = "";
            rv.suConfPassIn.text = "";
            rv.siEmailIn.text = LoadManager.LoadEmail();

            rv.qaPanel.SetActive(true);
            rv.signupPanel.SetActive(false);
        }
    }

    private IEnumerator Signin()
    {
        WWWForm form = new WWWForm();

        form.AddField("Email", rv.siEmailIn.text);
        form.AddField("Password", rv.siPassIn.text);

        WWW www = new WWW(rv.reqURL + "/api/Users/Login", form.data);
        yield return www;

        if (www.error != null)
        {
            Debug.Log(www.error);

            rv.ShowInfo("Login failed");
        }
        else
        {
            string jsonData = www.text;
            User user = JsonUtility.FromJson<User>(jsonData);
            UserManager.userID = int.Parse(user.userID);
            UserManager.username = user.username;
            UserManager.token = user.token;
            SaveManager.SaveEmail(user.email);
            rv.siPassIn.text = "";

            rv.qaPanel.SetActive(true);
            rv.signinPanel.SetActive(false);
        }
    }
}
