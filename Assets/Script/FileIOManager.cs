using System;
using System.IO;
using System.Text;
using UnityEngine;

public class FileIOManager : MonoBehaviour {
    private StreamWriter sw;
    private StreamReader sr;
    private FileInfo fi;
    private string date;

    void Start () {

        date = DateTime.Now.ToString("yyyyMMdd");
        try
        {
            fi = new FileInfo(Application.persistentDataPath + "/MyDiary" + date + ".txt");
        }
        catch (Exception e)
        {
            throw e;
        }
    }

    /*
     * テキストファイルに一行書き込む
     */
    public void WriteDiary(string txt)
    {
        string hour = DateTime.Now.ToString("HH");
        string minute = DateTime.Now.ToString("mm");
        string second = DateTime.Now.ToString("ss");

        sw = fi.AppendText();
        sw.WriteLine(hour+"時"+minute+"分"+second+"秒、私は"+ txt +"を見ました。"); //日記風の文字列を生成。

        sw.Flush();
        sw.Dispose();
    }

    /*
     * テキストファイルを読み込んで文字列を返却する
     */
    public string ReadDiary()
    {
        string str;

        try
        {
            sr = new StreamReader(fi.OpenRead(), Encoding.UTF8);
            str = sr.ReadToEnd();
        }
        catch (Exception e)
        {
            throw e;
        }

        sr.Dispose();
        return str;
    }
}
