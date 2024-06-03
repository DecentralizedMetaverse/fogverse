using System.IO;
using DC;
using Teo.AutoReference;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Path = System.IO.Path;

/// <summary>
/// </summary>
public class ContentImportView : UIComponent
{
    private const string SaveDataKey = "ContentDirectory";
    private static readonly string contentPath = $"{Application.dataPath}/../content/";
    private const string DriveRoot = "DriveRoot";

    [SerializeField] private ButtonLabelView buttonPrefab;

    [Get, SerializeField] private UIEasingAnimationPosition animation;

    [GetInChildren, Name("DirectoryInputField"), SerializeField]
    private TMP_InputField input;

    [GetInChildren, Name("Content"), SerializeField]
    private Transform content;

    [GetInChildren, Name("OpenFolderButton"), SerializeField]
    private Button openFolderButton;

    private string[] files;
    private string[] directories;
    private string currentPath;

    private void Start()
    {
        SaveData.I.TryGetValue(SaveDataKey, out string path);
        path ??= $"{Application.dataPath}/{GM.mng.contentPath}";
        currentPath = path;
        input.text = path;
        input.onEndEdit.AddListener(_ => OnDirectoryChanged());
        openFolderButton.onClick.AddListener(OpenFolder);
    }

    private void OpenFolder()
    {
        System.Diagnostics.Process.Start(currentPath);
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    private void OnDirectoryChanged()
    {
        currentPath = input.text;
        if (currentPath == DriveRoot || currentPath == "")
        {
            // Display drives
            content.DestroyChildren();
            var drives = Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                var driveName = drive;
                var driveButton = Instantiate(buttonPrefab, content);
                driveButton.gameObject.SetActive(true);
                driveButton.Label.text = drive;
                driveButton.Button.onClick.AddListener(() => OnDriveSelected(driveName));
            }
        }
        else if (Directory.Exists(currentPath))
        {
            SaveData.I.Set(SaveDataKey, currentPath);

            content.DestroyChildren();

            files = Directory.GetFiles(currentPath);
            directories = Directory.GetDirectories(currentPath);

            // Add parent directory button
            var parentButton = Instantiate(buttonPrefab, content);
            parentButton.gameObject.SetActive(true);
            parentButton.Label.text = ".. (Parent Directory)";
            parentButton.Button.onClick.AddListener(OnParentDirectorySelected);

            // Add directory buttons
            var i = 0;
            foreach (var directory in directories)
            {
                var dirIndex = i;
                var directoryButton = Instantiate(buttonPrefab, content);
                directoryButton.gameObject.SetActive(true);
                directoryButton.Label.text = Path.GetFileName(directory);
                directoryButton.Button.onClick.AddListener(() => OnDirectorySelected(dirIndex));
                i++;
            }

            // Add file buttons
            foreach (var file in files)
            {
                var fileIndex = i;
                var fileButton = Instantiate(buttonPrefab, content);
                fileButton.gameObject.SetActive(true);
                fileButton.Label.text = Path.GetFileName(file);
                fileButton.Button.onClick.AddListener(() => OnSubmit(fileIndex));
                i++;
            }
        }
    }

    private void OnParentDirectorySelected()
    {
        if (currentPath == DriveRoot || Directory.GetParent(currentPath) == null)
        {
            currentPath = DriveRoot;
        }
        else
        {
            currentPath = Directory.GetParent(currentPath)?.FullName;
        }
        input.text = currentPath;
        OnDirectoryChanged();
    }

    private void OnDirectorySelected(int i)
    {
        currentPath = directories[i];
        input.text = currentPath;
        OnDirectoryChanged();
    }

    private void OnDriveSelected(string drive)
    {
        currentPath = drive;
        input.text = currentPath;
        OnDirectoryChanged();
    }

    /// <summary>
    /// </summary>
    /// <param name="i"></param>
    private void OnSubmit(int i)
    {
        // Copy file
        var source = files[i - directories.Length];
        Debug.Log($"[ContentImportView] OnSubmit: {source}");
        var destination = $"{contentPath}/{Path.GetFileName(source)}";

        if (!File.Exists(destination))
        {
            File.Copy(source, destination);
        }

        GM.Msg("GenerateObj", destination);
        GM.Msg("RegisterObject"); // これを実行するタイミングに注意　全MetaFileが書き変わる
    }

    public override void Show()
    {
        base.Show();
        animation.Show();
        currentPath = DriveRoot;
        OnDirectoryChanged();
    }

    public override void Close()
    {
        base.Close();
        animation.Close();
    }
}
