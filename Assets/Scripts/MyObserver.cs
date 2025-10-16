using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class MyObserver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Simulator.OnNewPlayer += OnNewPlayer;
        Simulator.OnNewSession += OnNewSession;
        Simulator.OnEndSession += OnEndSession;
        Simulator.OnBuyItem += OnNewPurchase;
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
        StartCoroutine(UploadPlayerData(json));
    }

    IEnumerator UploadPlayerData(string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("PlayerData", json);

        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~marcobp1/player.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Form uploaded completed UwU Teehee! <3");
            Debug.Log(www.downloadHandler.text); // Muestra la respuesta del servidor
            var a = www.downloadHandler.text;
            uint player_id = uint.Parse(www.downloadHandler.text);
            CallbackEvents.OnAddPlayerCallback?.Invoke(player_id);
        }
    }
    SessionData sessionData;


    public void OnNewSession(DateTime _date, uint playerId)
    {
        sessionData = new SessionData();
        sessionData.date = _date.ToString("yyyy-MM-dd HH:mm:ss");
        sessionData.UID = (int)playerId;
        sessionData.endDate = _date.ToString("yyyy-MM-dd HH:mm:ss"); // TODO send this onEnd session event
        string json = JsonUtility.ToJson(sessionData);
        StartCoroutine(UploadSession(json));
    }

    IEnumerator UploadSession(string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("SessionData", json);

        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~marcobp1/player.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Session uploaded completed UwU Teehee! <3");
            Debug.Log(www.downloadHandler.text); // Muestra la respuesta del servidor
            uint session_id = uint.Parse(www.downloadHandler.text); // recupera el session id del echo del servidor
            CallbackEvents.OnNewSessionCallback?.Invoke(session_id);
        }
    }

    void OnEndSession (DateTime _date, uint sessionId)
    {
        sessionData.endDate = _date.ToString("yyyy-MM-dd HH:mm:ss");
        string json = JsonUtility.ToJson(sessionData);
        //StartCoroutine(UpdateSessionEnd(json));
    }

    IEnumerator UpdateSessionEnd(string json)
    {
        WWWForm form = new WWWForm();

        form.AddField("SessionData", json);
        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~marcobp1/player.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Session uploaded completed UwU Teehee! <3");
            Debug.Log(www.downloadHandler.text); // Muestra la respuesta del servidor
            uint purchase_id = uint.Parse(www.downloadHandler.text); // recupera el session id del echo del servidor
            CallbackEvents.OnItemBuyCallback?.Invoke(purchase_id);
        }
    }

    void OnNewPurchase(int item_id, DateTime purchase_date, uint session_id)
    {
        PurchaseData purchase_data = new PurchaseData(item_id, purchase_date.ToString("yyyy-MM-dd HH:mm:ss"), (int)session_id);

        string json = JsonUtility.ToJson(purchase_data);
        StartCoroutine(UploadPurchase(json));
    }

    IEnumerator UploadPurchase(string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("PurchaseData", json);

        UnityWebRequest www = UnityWebRequest.Post("https://citmalumnes.upc.es/~marcobp1/player.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Session uploaded completed UwU Teehee! <3");
            Debug.Log(www.downloadHandler.text); // Muestra la respuesta del servidor
            uint purchase_id = uint.Parse(www.downloadHandler.text); // recupera el session id del echo del servidor
            CallbackEvents.OnItemBuyCallback?.Invoke(purchase_id);
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

[Serializable]
public class SessionData
{
    public int sessionId;
    public int UID;
    // TODO : change date to DateTime type , C# serializes it differently than how MySQL excpoects it so we must ensure the format change
    public string date;
    public string endDate;
}

[Serializable]
public class PurchaseData
{
    public PurchaseData(int item_id, string purchase_date, int session_id)
    {
        this.item_id = item_id;this.purchase_date = purchase_date;this.session_id = session_id;
    }
    public string purchase_date;
    public int session_id;
    public int item_id;


}

