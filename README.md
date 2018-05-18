# HoloLensComputerVisionSample

de:code 2018 AC62「簡単！！HoloLensで始めるCognitive Services～de:code 2018特別バージョン～」の  
Computer Vision API用サンプルコードです。  
HoloLensで画像キャプチャを取得し、Computer Vision APIを呼び出すことで\
画像の見出しを取得します。  
取得した見出しは時刻と共にローカルにファイル保存されます。
保存したファイルは音声コマンド「read diary」で可視化できます。

## バージョン情報
 Unity：2017.1.2p3  
 MRToolkit：HoloToolkit-Unity-v1.2017.1.2  
 VisualStudio：15.5.4  

## 使い方

1.本PJをクローンし、Azure Computer Vision APIのキーを  
 GetVisionInfo.cs の visionAPIKeyに設定してください。  

2.エアタップで画像取得～Computer Vision APIの呼び出し～ローカル保存までを行います。  

3.「read diary」と発話することでキーワード認識が発動し、今まで保存していたファイルを読み込んで表示します。

## 注意点

1.AzureおよびComputer Vision API自体の操作、設定に関しては本PJ内では説明致しません。

2.Capability SettingsのMicrophone,Webcam,Internet Clientは必須です。

## 問い合わせ
twitter [@morio36](https://twitter.com/morio36)
