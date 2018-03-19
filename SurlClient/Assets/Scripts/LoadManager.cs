using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    // Load the player email
    public static string LoadEmail()
    {
        string email = "";

        if (ES2.Exists("gepr?tag=email"))
        {
            email = ES2.Load<string>("gepr?tag=email");
        }

        return email;
    }
}
