using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    readonly int MAX_VOICE_BUFFER_LENGTH  = 10 * 1024 * 1024;
    readonly byte[] VOICE_HEADER_START    = { 0x0, 0x0, 0x0, 0x1 };
    readonly byte[] VOICE_HEADER_RECODING = { 0x0, 0x0, 0x0, 0x2 };
    readonly byte[] VOICE_HEADER_END      = { 0x0, 0x0, 0x0, 0x3 };

    string statusText = "";
    string receiveText = "";

    // 测试保存数据
    byte[] voiceBuffer;
    int totalLength = 0;

    // Start is called before the first frame update
    void Start()
    {
        voiceBuffer = new byte[MAX_VOICE_BUFFER_LENGTH];

        lark.LarkManager larkManager = lark.LarkManager.Instance;
        larkManager.DataChannel.onConnected += OnConnected;
        larkManager.DataChannel.onText += OnTextMessage;
        larkManager.DataChannel.onBinary += OnBinaryMessaeg;
        larkManager.DataChannel.onClose += OnClose;

        // start connect
        lark.DataChannelNativeApi.ApiRestult restult = lark.LarkManager.Instance.StartConnect();

        if (restult == lark.DataChannelNativeApi.ApiRestult.XR_SUCCESS)
        {
            statusText = "StartConnect XR_SUCCESS";
        }
        else
        {
            statusText = "StartConnect " + restult;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(10, 10, 400, 150), "Voice Test");
        GUI.Label(new Rect(20, 40, 380, 20), "TaskId " + lark.LarkManager.Instance.TaskId);
        GUI.Label(new Rect(20, 70, 380, 20), "状态 " + statusText);
        GUI.Label(new Rect(20, 100, 380, 50), "收到 " + receiveText);
    }

    #region wrap channel 
    public void SendText(string txt)
    {
        lark.LarkManager.Instance.Send(txt);
    }

    public void SendBinary(byte[] data)
    {
        lark.LarkManager.Instance.Send(data);
    }
    #endregion

    #region callbacks
    public void OnConnected()
    {
        statusText = "连接成功";
    }
    public void OnTextMessage(string msg)
    {
        Debug.Log("OnTextMessage " + msg);
        receiveText = msg;
    }
    public void OnBinaryMessaeg(byte[] binary)
    {
        Debug.Log("OnBinaryMessaeg " + binary.Length);
        // 0x0 0x0 0x0 0x1---------音频输入开始
        // 0x0 0x0 0x0 0x2---------音频输入中(用户录音时循环发送，初步定义为 1s 中切片一次即发送一次音频)
        // 0x0 0x0 0x0 0x3---------音频输入结束(用户本次输入结束)

        // 音频格式为 wav, sampleRate: 16000 sampleBits: 16,
        if (binary.Length < 4)
        {
            receiveText = "收到错误格式数据 : " + BitConverter.ToString(binary);
            return;
        }

        byte[] header = { binary[0], binary[1], binary[2], binary[3] };

        int bodyLength = binary.Length - 4;

        if (totalLength + bodyLength >= MAX_VOICE_BUFFER_LENGTH)
        {
            receiveText = "数据BUFFER已满; 清空已有数据";
            totalLength = 0;
            return;
        }

        // save data
        Buffer.BlockCopy(binary, 4, voiceBuffer, totalLength, bodyLength);
        totalLength += bodyLength;

        string headerText = "";

        if (header.SequenceEqual(VOICE_HEADER_START))
        {
            headerText = "音频开始 Header: ";
        } else if (header.SequenceEqual(VOICE_HEADER_RECODING))
        {
            headerText = "录音中 Header: ";
        }
        else if (header.SequenceEqual(VOICE_HEADER_END))
        {
            headerText = "音频结束 Header: ";
            // 模拟处理数据
            // 处理之后清空buffer
            totalLength = 0;
        } else
        {
            headerText = "收到未知header : ";
        }

        receiveText = headerText + BitConverter.ToString(header) + "; body length : " + bodyLength + "; total length " + totalLength;
    }
    public void OnClose(lark.DataChannelNativeApi.ErrorCode code)
    {
        statusText = "通道已关闭 code " + code;
    }
    #endregion
}
