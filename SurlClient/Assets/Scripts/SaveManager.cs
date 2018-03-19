using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Encrypt and save player email
    public static void SaveEmail(string email)
    {
        ES2.Save(email, "gepr?tag=email");
    }
}
