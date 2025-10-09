using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class MyObserver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Simulator.OnNewPlayer += OnNewPlayer;
    }

    void OnNewPlayer(string name, string country, int age, float gender, DateTime date)
    {
        // Convert to JSON object
        PlayerData playerData = new PlayerData
        {
            name = name,
            country = country,
            age = age,
            gender = gender,
            date = date.ToString("o") // ISO 8601 format
        };

        string json = JsonUtility.ToJson(playerData);

        // Send to server
        StartCoroutine(SendPlayerData(json));
    }

    IEnumerator SendPlayerData(string json)
    {
        string url = "https://citmalumnes.upc.es/~marcobp1/player.php"; // Replace with your server URL
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Data sent successfully: " + request.downloadHandler.text);
            CallbackEvents.OnAddPlayerCallback?.Invoke(42);
        }
        else
        {
            Debug.LogError("Error sending data: " + request.error);
        }
    }
}

[Serializable]
public class PlayerData
{
    public string name;
    public string country;
    public int age;
    public float gender;
    public string date;
}


