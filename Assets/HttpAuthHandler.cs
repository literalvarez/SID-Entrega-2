using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System.Linq;

public class HttpAuthHandler : MonoBehaviour
{

    [SerializeField]
    private string ServerApiURL;
    [SerializeField] 
    Text leaderboard;

    public string Token { get; set; }
    public string Username { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(Token))
        {
            Debug.Log("No hay token");
            //Ir a Login
        }
        else
        {
            Debug.Log(Token);
            Debug.Log(Username);
            StartCoroutine(GetPerfil());
        }
    }

    public void Registrar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Registro(postData));
    }

    public void Ingresar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Login(postData));
    }

    public void StartGetScores()
    {
        StartCoroutine(GetScores());
    }

    IEnumerator Registro(string postData)
    {
        Debug.Log(postData);
        string url = ServerApiURL + "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Put(url, postData);
        //UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios", postData);
        www.method = "POST";
        www.SetRequestHeader("content-type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                

                Debug.Log(jsonData.usuario.username + " se regitro con id " + jsonData.usuario._id);
                //Proceso de autenticacion
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator Login(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/auth/login", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " inicio sesion");

                Token = jsonData.token;
                Username = jsonData.usuario.username;

                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);

                //Cambiar de escena
                SceneManager.LoadScene("Game");

            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator GetPerfil()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios/"+Username);
        www.SetRequestHeader("x-token", Token);


        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " Sigue con la sesion inciada");

                
                
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }

    IEnumerator GetScores()
    {
        string url = ServerApiURL + "/api/usuarios?limit=5&sort=true";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token", Token);
        www.SetRequestHeader("content-type", "application/json");

        yield return www.SendWebRequest();

        leaderboard.enabled = true;

        leaderboard.text += "LEADERBOARD: \n";

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
        }
        else if (www.responseCode == 200)
        {
            string response = www.downloadHandler.text;

            UserList ScoreLists = JsonUtility.FromJson<UserList>(www.downloadHandler.text);
            List<User> UserList = ScoreLists.usuarios.OrderByDescending(u => u.data.score).ToList<User>();
            Debug.Log(www.downloadHandler.text);
            foreach (User i in UserList)
            {
                leaderboard.text += i.username+ "="+ i.data.score+ "\n";
            }

        }
        else
        {
            Debug.Log(www.error);
        }
    }

    public void UpdateHighScore()
    {
        if (PlayerPrefs.GetInt("highScore") < Score.score)
        {
            User user = new User();

            user.username = PlayerPrefs.GetString("username");
            user.data = new UserScore(Score.score);
            string postData = JsonUtility.ToJson(user);
            StartCoroutine(UpdateScore(postData));
        }
    }

    public IEnumerator UpdateScore(string postData)
    {
        Debug.Log("patch score");


        string url = ServerApiURL + "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Put(url, postData);
        www.method = "PATCH";
        www.SetRequestHeader("content-type", "application/json");
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
            Debug.Log(www.downloadHandler.text);
        }
        else if (www.responseCode == 200)
        {
            Debug.Log(www.downloadHandler.text);
            UserData resData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);

            //Debug.Log("Puntaje: " + resData.username + " | " + resData.score); 

        }
        else
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }

}

[System.Serializable]
public class User
{
    public string _id;
    public string username;
    public string password;
    public UserScore data;
    public User() { }


    public User(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}
[System.Serializable]
public class UserScore
{
    public int score;
    public UserScore(int score)
    {
        this.score = score;
    }
}
public class UserList
{
    public List<User> usuarios;
}

public class AuthJsonData
{
    public User usuario;
    public string token;
}