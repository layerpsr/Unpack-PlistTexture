using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
//using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.UI;

public class MainActivity : MonoBehaviour
{
    //Canvas
    private Transform m_Canvas;

    //In源路径
    private InputField m_Inpath_Input;
    private Toggle m_Inpath_ChildDir;
    private Button m_Input_ChoiceFile;
    private Button m_Input_ChoiceDir;
    //Out路径
    private InputField m_Outpath_Input;
    private Toggle m_Outpath_UseSource;
    private Button m_Output_Choice;
    private Button m_Output_Copy;
    //Setting设置
    private Toggle m_Setting_Rotate;
    private Toggle m_Setting_Backup;
    private Toggle m_Setting_ClearOut;
    private Toggle m_Setting_UseFileName;
    //OutLog
    private Text m_Outlog_State;
    private Text m_Outlog_Show;

    // Use this for initialization
    void Start()
    {
        //节点
        m_Canvas = GameObject.Find("Canvas").transform;
        m_Inpath_Input = m_Canvas.Find("Inpath/InputField").GetComponent<InputField>();
        m_Inpath_ChildDir = m_Canvas.Find("Inpath/ChildDir").GetComponent<Toggle>();
        m_Input_ChoiceFile = m_Canvas.Find("Inpath/File").GetComponent<Button>();
        m_Input_ChoiceDir = m_Canvas.Find("Inpath/Dir").GetComponent<Button>();
        m_Outpath_Input = m_Canvas.Find("Outpath/InputField").GetComponent<InputField>();
        m_Outpath_UseSource = m_Canvas.Find("Outpath/UseSource").GetComponent<Toggle>();
        m_Output_Choice = m_Canvas.Find("Outpath/Choice").GetComponent<Button>();
        m_Output_Copy = m_Canvas.Find("Outpath/Copy").GetComponent<Button>();
        m_Setting_Rotate = m_Canvas.Find("Setting/Rotate").GetComponent<Toggle>();
        m_Setting_Backup = m_Canvas.Find("Setting/Backup").GetComponent<Toggle>();
        m_Setting_ClearOut = m_Canvas.Find("Setting/ClearOut").GetComponent<Toggle>();
        m_Setting_UseFileName = m_Canvas.Find("Setting/UseFileName").GetComponent<Toggle>();
        m_Outlog_State = m_Canvas.Find("OutLog/State").GetComponent<Text>();
        m_Outlog_Show = m_Canvas.Find("OutLog/Text").GetComponent<Text>();
        InitializationEvent();
        //开始按钮
        m_Canvas.Find("Menu/Start").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (m_Outpath_UseSource.isOn || Directory.Exists(m_Outpath_Input.text))
            {
                string outPath = m_Outpath_UseSource.isOn ? null : m_Outpath_Input.text;
                SplitTexture.Do(m_Inpath_Input.text, m_Inpath_ChildDir.isOn, outPath, m_Setting_ClearOut.isOn, m_Setting_UseFileName.isOn, m_Setting_Backup.isOn, m_Setting_Rotate.isOn);
            }
        });
    }
    private void Update()
    {
        m_Outlog_State.text = SplitTexture.state;
        m_Outlog_Show.text = "完成" + SplitTexture.number;
    }
    //注册事件
    void InitializationEvent()
    {
        m_Input_ChoiceFile.onClick.AddListener(() =>
        {
            var path = OpenDialog.OpenFile();
            if (!string.IsNullOrEmpty(path))
            {
                m_Inpath_Input.text = path;
            }
        });
        m_Input_ChoiceDir.onClick.AddListener(() =>
        {
            var path = OpenDialog.OpenDir();
            if (!string.IsNullOrEmpty(path))
            {
                m_Inpath_Input.text = path;
            }
        });
        m_Output_Choice.onClick.AddListener(() =>
        {
            var path = OpenDialog.OpenDir();
            if (!string.IsNullOrEmpty(path))
            {
                m_Outpath_Input.text = path;
            }
        });
        m_Outpath_UseSource.onValueChanged.AddListener((isOn) =>
        {
            m_Outpath_Input.interactable = !isOn;
            m_Output_Choice.interactable = !isOn;
            m_Output_Copy.interactable = !isOn;
        });
        m_Output_Copy.onClick.AddListener(()=> {
            if (!string.IsNullOrEmpty(m_Inpath_Input.text)) {
                m_Outpath_Input.text = Directory.Exists(m_Inpath_Input.text) ? m_Inpath_Input.text : Path.GetDirectoryName(m_Inpath_Input.text);
            }
        });
    }
}
