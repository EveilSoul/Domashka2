using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CMD_Script : MonoBehaviour
{
    public Text Path;
    public InputField InputField;
    public Text ResultText;

    public string CurrentPath;
    public string StartPath = @"C:\Users\Ev\Documents\Новая папка";

    private string currentText;
    private List<string> commandsHistory;
    private int commandIndex;
    private string moveMessage = @"<color=#47FF8E><b>Файлы были успешно перемещены</b></color>";
    private string errorPathMessage = @"<color=red><b>Заданный путь не существует</b></color>";
    private string errorFileExistMessage = @"Данные файлы не существуют: ";
    private string errorCommandExistMessage = @"Команда не распознана";

    public void OnEnable()
    {
        CurrentPath = StartPath;
        Path.text = CurrentPath;
        ResultText.text = "";
        InputField.text = "";
        InputField.Select();
        commandsHistory = new List<string>();
        commandIndex = -1;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteCommand(InputField.text);
            InputField.text = "";
            InputField.Select();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (commandIndex == -1)
                return;
            InputField.text = commandsHistory[commandIndex];
            commandIndex--;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (commandIndex == commandsHistory.Count - 1)
                return;
            commandIndex++;
            InputField.text = commandsHistory[commandIndex];
        }
    }

    public void ExecuteCommand(string command)
    {
        commandsHistory.Add(command);
        commandIndex = commandsHistory.Count - 1;

        var com = command.Split(' ');
        if (com[0] == "ls")
        {
            var files = GetFiles(CurrentPath);
            UpdateText(command, files);
        }
        else if (com[0] == "cd")
        {
            var path = GetPath(com[1]);
            if (PathExist(path))
            {
                CurrentPath = path;
                Path.text = path;
                UpdateText(command);
            }
            else
            {
                UpdateText(command, errorPathMessage);
            }
        }
        else if (com[0] == "mv")
        {
            var path = GetPath(com[com.Length - 1]);
            if (PathExist(path))
            {
                MoveFiles(command, CurrentPath);
            }
            else
            {
                UpdateText(command, errorPathMessage);
            }
        }
    }

    public void MoveFiles(string command, string path)
    {
        List<string> allFiles = Directory.GetFiles(path).ToList().Select(e => e.Split('\\').Last()).ToList();

        if (command.Contains('*') || command.Contains('?'))
        {
            string[] splitedCommand;
            if (command.Contains('"'))
            {
                splitedCommand = command.Split('"');
            }
            else
            {
                splitedCommand = command.Split(' ');
            }
            var files = Directory.GetFiles(path, splitedCommand[1]);
            UpdateText(command, moveMessage + '\n' + GetFilesList(files, false, true));
        }
        else
        {
            var files = GetFilesFromCommand(command);

            var allExist = true;
            var notExistFiles = new List<string>();
            foreach (var e in files)
            {
                if (!allFiles.Contains(e))
                {
                    allExist = false;
                    notExistFiles.Add(e);
                }
            }

            if (allExist)
            {
                UpdateText(command, moveMessage);
            }
            else
            {
                UpdateText(command, errorFileExistMessage + GetStr(notExistFiles));
            }
        }
    }

    public List<string> GetFilesFromCommand(string command)
    {
        var sp = command.Split('"');
        var res = new List<string>();
        var noQuote = new List<string>();

        if (sp.Length == 1)
        {
            var spl = sp[0].Split(' ');
            for (int i = 1; i < spl.Length - 1; i++)
            {
                if (spl[i].Length > 0 && !spl[i].Contains(' '))
                    res.Add(spl[i]);
            }
        }
        else
        {
            for (int i = 1; i < sp.Length - 1; i++)
            {
                if (i % 2 == 1 && !sp[i].All(x => x == ' '))
                    res.Add(sp[i]);
                else
                {
                    var spl = sp[i].Split(' ');
                    foreach (var e in spl)
                    {
                        if (e.Length > 0 && !e.Contains(' '))
                            noQuote.Add(e);
                    }
                }
            }
        }

        return res.Concat(noQuote).ToList();
    }

    public string GetStr(List<string> list)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < list.Count; i++)
        {
            sb.Append(list[i]);
            if (i < list.Count - 1)
                sb.Append(", ");
        }
        return sb.ToString();
    }

    public bool PathExist(string path)
    {
        return Directory.Exists(path) && !File.Exists(path);
    }

    public string GetPath(string path)
    {
        var sp = path.Split('\\');
        if (sp[0].Length == 2 && sp[0][1] == ':')
        {
            return path;
        }
        else if (path == ".." || path == "../" || path == "..\\")
        {
            var res = "";
            var splitedCurPath = CurrentPath.Split('\\');
            for (var i = 0; i < splitedCurPath.Length - 1; i++)
            {
                res += splitedCurPath[i];
                if (i < splitedCurPath.Length - 2)
                    res += '\\';
            }
            return res;
        }
        else if (path.All(x => x == '.'))
        {
            return CurrentPath;
        }
        else 
        {
            return CurrentPath + '\\' + path;
        }
    }

    public void UpdateText(string command, string text)
    {
        var curText = "";
        curText += @"> " + command;
        curText += '\n';
        curText += text;
        curText += '\n';
        curText += ResultText.text;
        ResultText.text = curText;
    }

    public void UpdateText(string command)
    {
        var curText = "";
        curText += @"> " + command;
        curText += '\n';
        curText += ResultText.text;
        ResultText.text = curText;
    }

    public string GetFilesList(string[] files, bool isDir, bool isZapyataya)
    {
        var sb = new StringBuilder();
        foreach (var e in files)
        {
            var sp = e.Split('\\');
            if (isDir)
            {
                sb.Append("<color=#00ffffff>");
                sb.Append(sp.Last());
                sb.Append("</color>");
            }
            else
            {
                sb.Append(sp.Last());
            }
            if (isZapyataya)
            {
                sb.Append(", ");
            }
            else
            {
                sb.Append('\n');
            }
        }
        if (sb.Length > 0)
            sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    public string GetFiles(string path)
    {
        return GetFilesList(Directory.GetDirectories(path), true, false) + GetFilesList(Directory.GetFiles(path), false, false);
    }



}
