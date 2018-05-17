// Copyright(c) 2018 Shingo Mori
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR.WSA.WebCam;

public class GetVisionInfo : MonoBehaviour, IInputClickHandler
{
    // UI周りのフィールド
    public GameObject DiaryPrefab;   //テキスト部品のプレファブ
    public GameObject Canvas;        //テキストを貼り付けるためのキャンバス
    public Text SystemMessage;       //状況表示用メッセージエリア
    public RawImage photoPanel;      //debug用のキャプチャ表示パネル

    private FileIOManager fileIOManager;

    // カメラ周りのパラメータ
    private PhotoCapture photoCaptureObject = null;
    private Resolution cameraResolution;
    private Quaternion cameraRotation;

    // Azure側のパラメータ群
    private string visionAPIKey = "YOUR_APP_KEY"; //Computer Vision APIのAPPキーをセットする TODO
    private string region = "YOUR_REGION";        //Computer Vision APIの地域をセットする
    private string visionURL;

    // リトライを促すためのメッセージ
    private TextToSpeech tts;
    private const string TRY_AGAIN_STR = "Couldn't get Any Captions. Please Try Again...";

    public void OnInputClicked(InputClickedEventData eventData)
    {
        AnalyzeScene();
    }

    void Start()
    {
        InputManager.Instance.AddGlobalListener(gameObject);
        visionURL = "https://" + region + ".api.cognitive.microsoft.com/vision/v2.0/analyze?visualFeatures=Tags,Description&language=ja";
        cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        tts = gameObject.GetComponent<TextToSpeech>();
        fileIOManager = gameObject.GetComponent<FileIOManager>();
    }

    private void AnalyzeScene()
    {
        DisplaySystemMessage("Detect Start...");
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    //PhotoCaptureの取得は下記参照
    //https://docs.microsoft.com/ja-jp/windows/mixed-reality/locatable-camera-in-unity
    private void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        DisplaySystemMessage("Take Picture...");
        photoCaptureObject = captureObject;

        CameraParameters c = new CameraParameters();
        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.JPEG;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
            throw new Exception();
        }
    }

    private void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // ComputerVision APIに送るimageBufferListにメモリ上の画像をコピーする
            List<byte> imageBufferList = new List<byte>();
            photoCaptureFrame.CopyRawImageDataIntoBuffer(imageBufferList);

            //ここはデバッグ用 送信画像の出力。どんな画像が取れたのか確認したい場合に使用。邪魔ならphotoPanelごと消してもよい。
            Texture2D debugTexture = new Texture2D(100, 100);
            debugTexture.LoadImage(imageBufferList.ToArray());
            photoPanel.texture = debugTexture;

            StartCoroutine(PostToVisionAPI(imageBufferList.ToArray()));
        }
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    private void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    /*
     * 取得した画像をComputer Vision APIに送信し、タグ、キャプションを取得する
     */
    private IEnumerator<object> PostToVisionAPI(byte[] imageData)
    {
        DisplaySystemMessage("Call Computer Vision API...");
        var headers = new Dictionary<string, string>() {
            { "Ocp-Apim-Subscription-Key", visionAPIKey },
            { "Content-Type", "application/octet-stream" }
        };

        WWW www = new WWW(visionURL, imageData, headers);
        yield return www;

        ResponceJson caption = JsonUtility.FromJson<ResponceJson>(www.text);
        
        //キャプションが取得できなかった場合は後続処理Skip（日本語だと取れない場合が多い模様）
        if (caption.description.captions.Length == 0)
        {

            DisplaySystemMessage(TRY_AGAIN_STR);
            tts.StartSpeaking(TRY_AGAIN_STR);
            yield break;
        }

        // 日記生成部品を呼び出す。Tagsは今回のサンプルではDebug出力のみ。
        fileIOManager.WriteDiary(caption.description.captions[0].text);
        DisplaySystemMessage(caption.description.captions[0].text);

        foreach (Tags tag in caption.tags)
        {
            /* ここを自然言語処理でtag集めて文章生成したらより日記風になって楽しくなるはず！！ */
            Debug.Log("name="+tag.name+ ",confidence" +tag.confidence+ "\n");
        }
    }

    /*
     * 音声入力「read diary」で発動するメソッド
     * 日記ファイルを読み込んで出力する。
     */
    public void OnRecognizedReadDiary()
    {

        GameObject prefab = (GameObject)Instantiate(DiaryPrefab);
        Vector3 position = Camera.main.transform.TransformPoint(0, 0, 1.5f);
        prefab.transform.parent = Canvas.transform;
        prefab.transform.position = position;

        Text outputText = prefab.GetComponentInChildren<Text>();
        outputText.text = fileIOManager.ReadDiary();
    }

    /*
     * 状況出力用メッセージ
     */
    private void DisplaySystemMessage(string message)
    {
        SystemMessage.text = message;
    }

    /*
     * ここから、Computer Vision APIの戻り値用クラス
     */
    [Serializable]
    private class ResponceJson
    {
        public Tags[] tags;
        public Discription description;
    }

    [Serializable]
    private class Tags
    {
        public string name;
        public float confidence;
    }

    [Serializable]
    private class Discription
    {
        public string[] tags;
        public Caption[] captions;
    }

    [Serializable]
    private class Caption
    {
        public string text;
        public float confidence;
    }
}