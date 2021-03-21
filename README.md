# MinecraftAutoBackup


## 概要
minecraftで「普段遊んでいるワールドに違うバージョンで入ってしまった！！」という時に役立つバックアップソフトです

minecraft起動時（minecraft lanchar起動時）に指定したワールドを一斉にバックアップします(デフォルトではすべてのワールドをバックアップします)
___

## 動作環境

- Windows 10 April 2018 Update(バージョン 1803)以降のWindows 10 PC

.NET Framework動作環境については[Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/framework/get-started/system-requirements)を参照ください

## ダウンロード方法

### 1. 以下のリンクをクリック

[最新バージョン](https://github.com/Cou01000111/MinecraftAutoBackup/releases/download/v1.0.0/MinecraftAutoBackup.zip)


注：この際以下のような画像が表示されることがございますが、これはダウンロード数が少ないことに起因するものになります

![a](https://github.com/Cou01000111/imgs/blob/main/MinecraftAutoBackup/%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%BC%E3%83%B3%E3%82%B7%E3%83%A7%E3%83%83%E3%83%88%202021-03-22%20012306.png)


### 2. ダウンロードしたzipファイルを解凍後好きな場所に配置

Cドライブをお勧めします

## 使い方

### 1. minecraft Auto Backupの起動

`minecraft Auto Backup.exe`を起動してください

### 2. 保存するワールドの選択

リストから保存したいワールドにチェックをして下さい（以下の画像はcouの環境での再現になります）

![起動した画面](https://github.com/Cou01000111/imgs/blob/main/MinecraftAutoBackup/%E3%82%B9%E3%82%AF%E3%83%AA%E3%83%BC%E3%83%B3%E3%82%B7%E3%83%A7%E3%83%83%E3%83%88%202021-03-22%20013341.png)

ここでokを押下すると変更が保存されます

この後から、minecraft launcherを起動した際にバックアップが作成されます

注: okを押下した際にmabprocessatwait.exeというソフトが起動しますが、これはMinecraft Auto Backupの一部になります

## その他機能について

### 設定から以下の項目を変えられます

- バックアップの保存形式（folder or zip）
- バックアップの保存数（一つのワールドに対して何世代バックアップを保存するかを設定できます）
- バックアップの保存先（デフォルトは`Document/MinecraftAutoBackup`です）
- ゲームディレクトリの追加（`ゲームディレクトリ/saves`という形態のフォルダであればゲームディレクトリとして認識します）
- フォント（僕のおすすめはUD 教科書字体 NK-Rです）
- スタートアップ（一応ボタン一つで追加できます）

## アンインストール方法
レジストリ等一切いじっていないので、ダウンロードしたフォルダをそのまま削除してください
一応互換性はありますので、バックアップフォルダは放置していてもアプリを再ダウンロードして、バックアップ先を削除前と同じ物にすれば認識するはずです（たぶん）

## 開発環境
- Windows10 PC
- Visual Studio 2019
- .NET SDK 5.0.200
- .NET Framework 4.7.2

## license
MIT licenseで配布するようにしたはずなんだけどなぁ・・・？
