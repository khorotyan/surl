using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static int userID = 0;
    public static string username = "";
    public static string token = "";
}

public class User
{
    public string userID;
    public string username;
    public string email;
    public int followerNum;
    public bool followingUser;
    public int followMins;
    public string token;
}
