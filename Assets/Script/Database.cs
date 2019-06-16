using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;
using System.Security.Cryptography;
using System;
using System.Collections;
using System.Collections.Generic;

public class Database : MonoBehaviour
{
    private static Database instance;

    public static Database Instance()
    {
        if (instance == null)
            instance = GameObject.FindObjectOfType(typeof(Database)) as Database;
        return instance;
    }

    public GameController gc;

    void Awake()
    {
        gc = GameController.Instance();
    }


    void Start()
    {
        ReadData();
    }

    /// data object to xml string
    public string SerializeObject(object pObject, System.Type ty)
    {
        string XmlizedString = null;
        MemoryStream memoryStream = new MemoryStream();
        XmlSerializer xs = new XmlSerializer(ty);
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        xs.Serialize(xmlTextWriter, pObject);
        memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
        XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
        return XmlizedString;
    }

    /// xml string to data object 
    public object DeserializeObject(string pXmlizedString, System.Type ty)
    {
        XmlSerializer xs = new XmlSerializer(ty);
        MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
        XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
        return xs.Deserialize(memoryStream);
    }

    public string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        string constructedString = encoding.GetString(characters);
        return (constructedString);
    }

    public byte[] StringToUTF8ByteArray(String pXmlString)
    {
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] byteArray = encoding.GetBytes(pXmlString);
        return byteArray;
    }

    public void CreateTextFile(string fileName, string strFileData, bool isEncryption)
    {
        StreamWriter writer;                               //file stream writer
        string strWriteFileData;
        if (isEncryption)
        {
            strWriteFileData = Encrypt(strFileData);
        }
        else
        {
            strWriteFileData = strFileData;
        }

        writer = File.CreateText(fileName);
        writer.Write(strWriteFileData);
        writer.Close();                                    //close file stream 
    }


    public string LoadTextFile(string fileName, bool isEncryption)
    {
        StreamReader sReader;                              //file stream reader  
        string dataString;                                 //data read 

        sReader = File.OpenText(fileName);
        dataString = sReader.ReadToEnd();
        sReader.Close();                                   //close file stream

        if (isEncryption)
        {
            return Decrypt(dataString);
        }
        else
        {
            return dataString;
        }

    }


    public string Encrypt(string toE)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12348578902223367877723456789012");
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toE);
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    public string Decrypt(string toD)
    {
        byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12348578902223367877723456789012");
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] toEncryptArray = Convert.FromBase64String(toD);
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return UTF8Encoding.UTF8.GetString(resultArray);
    }

    public class UserData
    {
        public int[] scores = new int[SCORES_NUMBER];
        public List<StarInfo> starInfos = new List<StarInfo>();
        public int scoreBefore;

        public UserData()
        {
            for (int i = 0; i < SCORES_NUMBER; i++)
                scores[i] = 0;
            scoreBefore = -1;
        }
    }

    public UserData userData;
    public static int SCORES_NUMBER = 5;

    //save and read data
    public void SaveData(UserData userData)
    {
        string _fileName = Application.persistentDataPath + "/UnityUserData";

        string s = SerializeObject(userData, typeof(UserData));

        CreateTextFile(_fileName, s, false);
    }

    public void ReadData()
    {
        string _fileName = Application.persistentDataPath + "/UnityUserData";
        Debug.Log(_fileName);

        try
        {
            string strTemp = LoadTextFile(_fileName, false);

            userData  = DeserializeObject(strTemp, typeof(UserData)) as UserData;

        }
        catch
        {
            UserData ud = new UserData();
            SaveData(ud);
            userData = ud;
        }

        if (userData.scoreBefore > 0)
            GameController.Instance().ableToContinue = true;
        else
            GameController.Instance().ableToContinue = false;
    }

    public void AddScoreToCharts(int score)
    {
        int index = -1;
        for (int i = 0; i < SCORES_NUMBER; i++)
        {
            if(userData.scores[i] < score)
            {
                index = i;
                break;
            }
        }

        if (index >= 0)
        {
            for (int j = SCORES_NUMBER - 1; j > index; j--)
                userData.scores[j] = userData.scores[j - 1];

            userData.scores[index] = score;

            GameController.Instance().newRecordText.gameObject.SetActive(true);
        }

        SaveData(userData);
    }

    public void SaveStarInfo()
    {
        userData.starInfos.Clear();
        for (int i = 0; i < GameController.WIDTH; i++)
            for (int j = 0; j < GameController.WIDTH; j++)
            {
                if (gc.stars[i, j] != null)
                {
                    userData.starInfos.Add(new StarInfo(new Index(i, j), gc.stars[i, j].colorType));
                }
            }

        userData.scoreBefore = gc.score;

        SaveData(userData);
    }

    public void FinishGame()
    {
        userData.starInfos.Clear();
        userData.scoreBefore = -1;
        SaveData(userData);
    }
}
