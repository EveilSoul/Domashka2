using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Explorer : MonoBehaviour
{
    public static bool IsPointerOutFile;

    private static Explorer instance;

    public string StartPath => Manager.StartPath;

    public GameObject ErrorMessage;
    public GameObject SuccessMessage;
    public GameObject FileMenu;
    public Button CutButton;
    public Button PasteButton;

    public Color SelectedColor;

    public Text PathText;
    public GameObject Files;

    public GameObject FolderIcon;
    public GameObject FileIcon;
    public GameObject ImageIcon;

    private string currentPath;
    private Color normalColor;
    private List<GameObject> selectedFiles;
    private List<GameObject> fileObjectsToMove;
    private List<string> filesToMove;

    private string[] ImageFormats = new[] { "png", "jpg", "jpeg" };

    private Vector3 menuOffset;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        normalColor = FileIcon.GetComponent<Image>().color;
        gameObject.SetActive(false);
        var offset = FileMenu.GetComponent<RectTransform>().sizeDelta;
        menuOffset = new Vector3(offset.x, -(offset.y + 1)) / 2;

        CutButton.interactable = false;
        PasteButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && IsPointerOutFile)
        {
            ClearSelectedFiles();
            FileMenu.SetActive(false);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            FileMenu.SetActive(true);
            FileMenu.transform.position = Input.mousePosition + menuOffset;
        }
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        currentPath = StartPath;
        Restart();
    }

    public void OnFolderClick(GameObject folder)
    {
        var prevPath = instance.currentPath;
        try
        {
            var name = folder.GetComponentInChildren<Text>().text;
            instance.currentPath += '\\' + name;
            instance.Restart();
        }
        catch (UnauthorizedAccessException e)
        {
            instance.currentPath = prevPath;
            instance.ShowError("Ошибка доступа", "Нет доступа к выбранному каталогу");
            instance.Restart();
        }
    }

    public static void OnFileSelected(GameObject file)
    {
        if (Input.GetMouseButtonUp(1))
        {
            Debug.Log("hh");
            if (instance.selectedFiles == null || instance.selectedFiles.Count == 0)
            {
                instance.AddFileToSelected(file);
                instance.CheckMenuButtons();
            }
        }
        else
        {
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                instance.ClearSelectedFiles();
            }
            instance.AddFileToSelected(file);
            instance.CheckMenuButtons();
        }
    }

    private void CheckMenuButtons()
    {
        CutButton.interactable = selectedFiles.Count > 0;
    }

    public void OnCutButtonClick()
    {
        FileMenu.SetActive(false);
        PasteButton.interactable = true;

        ClearFilesToMove();

        selectedFiles.ForEach(x => x.GetComponentsInChildren<Image>().First(i => i.gameObject.name == "Cut").enabled = true);
        selectedFiles.ForEach(x => fileObjectsToMove.Add(x));
        selectedFiles.ForEach(x => filesToMove.Add(currentPath + '\\' + x.GetComponentInChildren<Text>().text));
    }

    private void ClearFilesToMove()
    {
        if (fileObjectsToMove != null)
            fileObjectsToMove.ForEach(x => x.GetComponentsInChildren<Image>().First(i => i.gameObject.name == "Cut").enabled = false);
        fileObjectsToMove = new List<GameObject>();
        filesToMove = new List<string>();
    }

    public void OnPasteButtonClick()
    {
        if (!TryMoveFiles(filesToMove, currentPath, out List<string> filesCannotMove))
        {
            ShowError("ОШИБКА ПЕРЕМЕЩЕНИЯ",
                "Невозможно выполнить операцию\n" +
                "В данной директории уже есть следующие файлы:\n",
                filesCannotMove.ToArray());
            FileMenu.SetActive(false);
            return;
        }

        FileMenu.SetActive(false);
        PasteButton.interactable = false;
        CutButton.interactable = false;


        ClearFilesToMove();
        ClearSelectedFiles();

        Restart();

        ShowSuccess();
    }

    private void ShowSuccess()
    {
        SuccessMessage.SetActive(true);
    }

    /// <summary>
    /// Пытается переместить файлы из списка
    /// Если существует такой же файл в каталоге, перемещение не будет выполнено
    /// </summary>
    /// <param name="filesToMove">список файлов с абсолютным значением пути</param>
    /// <param name="destFolder">абсолютный путь к папке, куда перемещаем</param>
    /// <param name="filesCannotMove">список имен файлов, которые вызывают ошибку перемещения</param>
    /// <returns>успешность или неуспешность опреации</returns>
    public static bool TryMoveFiles(IEnumerable<string> filesToMove, string destFolder, out List<string> filesCannotMove)
    {
        Func<string, string> GetPath = name => destFolder + '\\' + name;

        filesCannotMove = new List<string>();
        foreach (var file in filesToMove)
        {
            var name = file.Split('\\').Last();
            if (File.Exists(GetPath(name)))
                filesCannotMove.Add(name);
        }

        if (filesCannotMove.Count > 0)
            return false;

        foreach (var file in filesToMove)
        {
            var name = file.Split('\\').Last();
            File.Move(file, GetPath(name));
        }
        return true;
    }

    private void AddFileToSelected(GameObject file)
    {
        if (selectedFiles == null)
            selectedFiles = new List<GameObject>();

        if (!selectedFiles.Contains(file))
        {
            selectedFiles.Add(file);
            file.GetComponent<Image>().color = SelectedColor;
        }
        else
        {
            file.GetComponent<Image>().color = normalColor;
            selectedFiles.Remove(file);
        }
    }

    private void ClearSelectedFiles()
    {
        if (selectedFiles != null)
        {
            selectedFiles.ForEach(x => x.GetComponent<Image>().color = normalColor);
        }
        selectedFiles = new List<GameObject>();
    }

    private void ShowError(string errorName, string errorMessage, params string[] errors)
    {
        ErrorMessage.SetActive(true);
        var allText = ErrorMessage.GetComponentsInChildren<Text>();
        allText.First(x => x.name == "Label").text = errorName;
        Text msg = allText.First(x => x.name == "Message");
        msg.text = errorMessage;
        if (errors != null && errors.Length > 0)
        {
            if (errors.Length > 1)
                msg.text += "\n" + errors.Aggregate((x, y) => $"{y}, {x}");
            else msg.text += "\n" + errors[0];
        }
    }

    public void OnStartScrolling()
    {
        IsPointerOutFile = false;
    }

    public void OnEndScrolling()
    {
        IsPointerOutFile = true;
    }

    public void Restart()
    {
        fileObjectsToMove = new List<GameObject>();

        PathText.text = currentPath;
        var dirs = Directory.GetDirectories(currentPath);
        var files = Directory.GetFiles(currentPath);

        var childCount = Files.transform.childCount;
        Enumerable.Range(0, childCount).ToList().ForEach(i => Destroy(Files.transform.GetChild(i).gameObject));

        SetDynamicHeigth(dirs, files);

        foreach (var dir in dirs)
        {
            var folder = Instantiate(FolderIcon, Files.transform);
            folder.GetComponentInChildren<Text>().text = GetName(dir);
        }

        foreach (var file in files)
        {
            string name = GetName(file);
            var prefab = FileIcon;
            if (ImageFormats.Contains(name.Split('.').Last()))
                prefab = ImageIcon;
            var obj = Instantiate(prefab, Files.transform);
            obj.GetComponentInChildren<Text>().text = name;

            if (filesToMove != null && filesToMove.Contains(file))
                obj.GetComponentsInChildren<Image>().First(i => i.gameObject.name == "Cut").enabled = true;
        }
    }

    private static string GetName(string dir)
    {
        string name = dir.Split('\\').Last();
        if (name.Contains(':'))
            name = name.Split(':').Last();
        return name;
    }

    private void SetDynamicHeigth(string[] dirs, string[] files)
    {
        var totalCount = dirs.Length + files.Length;
        GridLayoutGroup fileGroup = Files.GetComponent<GridLayoutGroup>();
        var cellSize = fileGroup.cellSize;
        RectTransform filesTransform = Files.GetComponent<RectTransform>();
        var filesInRow = filesTransform.sizeDelta.x / (cellSize.x + fileGroup.spacing.x);
        var height = (totalCount / filesInRow + (totalCount % filesInRow == 0 ? 0 : 1)) * (cellSize.y + fileGroup.spacing.y);
        filesTransform.sizeDelta = new Vector2(filesTransform.sizeDelta.x, Mathf.Max(height, 450));
        filesTransform.anchoredPosition = new Vector2(filesTransform.anchoredPosition.x, -height / 2);
    }

    public void MoveUp()
    {
        var res = "";
        var splitedCurPath = currentPath.Split('\\');
        for (var i = 0; i < splitedCurPath.Length - 1; i++)
        {
            res += splitedCurPath[i];
            if (i < splitedCurPath.Length - 2)
                res += '\\';
        }
        currentPath = res;
        Restart();
    }
}
