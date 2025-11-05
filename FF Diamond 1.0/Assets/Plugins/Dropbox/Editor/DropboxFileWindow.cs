using UnityEditor;
using UnityEngine;
using System.Text;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using Cysharp.Threading.Tasks;
using Plugins.Dropbox;

public class DropboxFilesWindow : EditorWindow
{
    bool isNeedDownloadButton = false;
    private Vector2 scrollPosition;
    private string filesList = "Click any button";
 
    [MenuItem("Dropbox/Show Files")]
    public static void ShowWindow()
    {
        var window = GetWindow<DropboxFilesWindow>("Dropbox Files");
        window.Show();
    }
 
    private void OnGUI()
    {
        GUILayout.Label("Dropbox Files", EditorStyles.boldLabel);
 
        if (GUILayout.Button("Refresh with download links"))
        {
            isNeedDownloadButton = true;
            _ = ButtonRefresh();
        }
 
        if (GUILayout.Button("Refresh visual path"))
        {
            isNeedDownloadButton = false;
            _ = ButtonRefreshVisual();
        }
 
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
 
        if (isNeedDownloadButton)
        {
            if (!string.IsNullOrEmpty(filesList))
            {
                string[] lines = filesList.Split('\n');
 
                foreach (string line in lines)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(line, EditorStyles.label);
 
                    if (GUILayout.Button("Download", GUILayout.Width(80)))
                    {
                        _ = StartDownloadFile(line);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField(filesList, EditorStyles.wordWrappedLabel);
        }
        EditorGUILayout.EndScrollView();
    }
 
    private async UniTaskVoid StartDownloadFile(string relativePath)
    {
        await DropboxHelper.Initialize();
 
        if (string.IsNullOrEmpty(relativePath))
        {
            string[] elements = filesList.Split('\r');
 
            foreach (string element in elements)
            {
                await DownloadFile(element);
            }
        }
        else
        {
            await DownloadFile(relativePath);
        }
    }
 
    private async UniTask DownloadFile(string relativePath)
    {
        relativePath = relativePath.TrimEnd('\r', '\n', ' ');
        relativePath = relativePath.TrimStart('\r', '\n', ' ');
 
        UnityWebRequest downloadRequest = DropboxHelper.GetRequestForFileDownload(relativePath.Substring(1));
        var operation = downloadRequest.SendWebRequest();
 
        while (!downloadRequest.isDone)
        {
            Debug.Log($"Download progress: {downloadRequest.downloadProgress:P2}");
            await UniTask.Delay(TimeSpan.FromSeconds(1));
        }
 
        string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/" + Application.productName;
 
        if (downloadRequest.result == UnityWebRequest.Result.Success)
        {
            string savePath = downloadsPath + relativePath;
 
            string directoryPath = Path.GetDirectoryName(savePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
 
            await File.WriteAllBytesAsync(savePath, downloadRequest.downloadHandler.data);
            Debug.Log($"File downloaded and saved to {savePath}");
        }
        else
        {
            Debug.LogError($"Error downloading file: {downloadRequest.error}");
        }
        downloadRequest.Dispose();
    }
 
    private async UniTaskVoid ButtonRefresh()
    {
        await DropboxHelper.Initialize();
        RefreshFilesList().Forget();
    }
 
    private async UniTaskVoid ButtonRefreshVisual()
    {
        await DropboxHelper.Initialize();
        RefreshFilesListVisual().Forget();
    }
 
    private async UniTaskVoid RefreshFilesList()
    {
        StringBuilder result = new StringBuilder();
        await FetchAllPaths("", result);
        filesList = result.ToString();
        Repaint();
    }
 
    private async UniTaskVoid RefreshFilesListVisual()
    {
        StringBuilder result = new StringBuilder();
        await FetchAllPathsVisual("", result);
        filesList = result.ToString();
        Repaint();
    }
 
    public async UniTask FetchAllPaths(string relativePathToFile, StringBuilder result)
    {
        UnityWebRequest request = DropboxHelper.GetRequestForListFolder(relativePathToFile);
        await request.SendWebRequest().ToUniTask();
        if (request.result == UnityWebRequest.Result.Success)
        {
            FileList response = JsonConvert.DeserializeObject<FileList>(request.downloadHandler.text);
 
            if (response.entries.Count == 0)
            {
                result.AppendLine(relativePathToFile + "/ !Folder Empty!");
            }
            else
            {
                var folders = response.entries.Where(entry => entry.tag == "folder").OrderBy(entry => entry.name).ToList();
                var files = response.entries.Where(entry => entry.tag != "folder").OrderBy(entry => entry.name).ToList();
 
                foreach (var folder in folders)
                {
                    await FetchAllPaths(folder.path_display, result);
                }
 
                for (int i = 0; i < files.Count; i++)
                {
                    result.AppendLine(files[i].path_display);
                }
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
    
    
    public async UniTask FetchAllPathsVisual(string relativePathToFile, StringBuilder result, string indent = "")
    {
        UnityWebRequest request = DropboxHelper.GetRequestForListFolder(relativePathToFile);
        await request.SendWebRequest().ToUniTask();
        if (request.result == UnityWebRequest.Result.Success)
        {
            FileList response = JsonConvert.DeserializeObject<FileList>(request.downloadHandler.text);
 
            if (response.entries.Count == 0)
            {
                result.AppendLine(indent + "└ Folder empty!");
            }
            else
            {
                var folders = response.entries.Where(entry => entry.tag == "folder").OrderBy(entry => entry.name).ToList();
                var files = response.entries.Where(entry => entry.tag != "folder").OrderBy(entry => entry.name).ToList();
 
                foreach (var folder in folders)
                {
                    result.AppendLine(indent + "┌ " + folder.name + "\t\t" + folder.server_modified);
                    await FetchAllPathsVisual(folder.path_display, result, indent + "│ ");
                }
 
                for (int i = 0; i < files.Count; i++)
                {
                    string prefix = (i == files.Count - 1) ? "└ " : "├ ";
                    result.AppendLine(indent + prefix + files[i].name);
                }
            }
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
 
    [Serializable]
    public class FileList
    {
        [JsonProperty("entries")]
        public List<Entry> entries;
 
        [JsonProperty("cursor")]
        public string cursor;
 
        [JsonProperty("has_more")]
        public bool has_more;
    }
 
    [Serializable]
    public class Entry
    {
        [JsonProperty(".tag")]
        public string tag;
 
        [JsonProperty("name")]
        public string name;
 
        [JsonProperty("path_lower")]
        public string path_lower;
 
        [JsonProperty("path_display")]
        public string path_display;
 
        [JsonProperty("id")]
        public string id;
 
        [JsonProperty("client_modified")]
        public string client_modified;
 
        [JsonProperty("server_modified")]
        public string server_modified;
 
        [JsonProperty("rev")]
        public string rev;
 
        [JsonProperty("size")]
        public int size;
 
        [JsonProperty("is_downloadable")]
        public bool is_downloadable;
 
        [JsonProperty("content_hash")]
        public string content_hash;
    }
}