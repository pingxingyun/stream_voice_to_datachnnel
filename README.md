# Use LarkXR datachannel streaming voice data to cloud app

---

[English](./README.md) [中文](./README.zh_CN.md)

---

## Prepare

### install node

1. install nodejs v15.14.0

[v15.14.0](https://nodejs.org/download/release/v15.14.0/)

2. install yarn

* yarn

```cmd
npm install -g yarn
```

## Start

Terminal cd to project [web] dir and run: 

```cmd
cd web
yarn install
```

### Start Debug

```cmd
yarn serve
```

```cmd
  App running at:
  - Local:   http://localhost:8080/
  - Network: http://192.168.0.161:8080/
```

Browser Open http://localhost:8080/

> Browser request https or localhost to enable audio recoder.

### Build and output

```
yarn build
```

### Change LarkSR config

Open web/src/App.vue find line 87 , change serverAddress, appliId. LarkSR cloud appid from LarkSR admin server.

```javascript
// SDK auth code，connect business@pingxingyun.com or register https://www.pingxingyun.com/console/#/
larksr.initSDKAuthCode("Your SDK ID")
```

### APPID

Open web/src/App.vue find line 93-95

```javascript
larksr.connectWithPxyHost({
    // LarkSR cloud appid from LarkSR admin server.
    // doc
    // https://www.pingxingyun.com/online/api3_2.html?id=476
    appliId: "appid from LarkSR admin",
});
```

> If not use https://www.pingxingyun.com/console/#/, connect to private LarkXR Server should change LarkXR Server address at line 78

### Send Voice Data

web/src/Components/voice/voice.vue, see startRecode, stopRecode, recodeTimeout, sendRecoderBuffer.

Data formate

```javascript
// wav, sampleRate: 16000 sampleBits: 16,
// 0x0 0x0 0x0 0x1---------Start 
// 0x0 0x0 0x0 0x2---------Sending(Send interal 1s)
// 0x0 0x0 0x0 0x3---------End
```

Recode audio permission see `requestAudio`。

> thanks: https://github.com/Zousdie/recorderx

Send data to cloud use datachannel, see sendRecoderBuffer：

```javascript
// send array buffer to datachannel.
this.larksr?.sendBinaryToDataChannel(uint8array);
```

data callback，see web/src/App.vue

```javascript
// 数据通道相关事件
larksr.on("datachannelclose", (e) => {
    console.log("LarkSRClientEvent datachannelclose", e);
});


larksr.on("datachanneltext", (e) => {
    console.log("LarkSRClientEvent datachanneltext", e);
});    

larksr.on("datachannelbinary", (e) => {
    console.log("LarkSRClientEvent datachannelbinary", e);
});
```

## Cloud Unity3D App

Import lark_datachannel_unity3d_export_unity2019.unitypackage enable datachannel Assets/Scenes/Manager.cs

```cs
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
```

Receive audio data

```cs
public void OnBinaryMessaeg(byte[] binary)
{
    Debug.Log("OnBinaryMessaeg " + binary.Length);
    // wav, sampleRate: 16000 sampleBits: 16,
    // 0x0 0x0 0x0 0x1---------Start 
    // 0x0 0x0 0x0 0x2---------Sending(Send interal 1s)
    // 0x0 0x0 0x0 0x3---------End
    if (binary.Length < 4)
    {
        receiveText = "Data error : " + BitConverter.ToString(binary);
        return;
    }

    byte[] header = { binary[0], binary[1], binary[2], binary[3] };

    int bodyLength = binary.Length - 4;

    if (totalLength + bodyLength >= MAX_VOICE_BUFFER_LENGTH)
    {
        receiveText = "clera data";
        totalLength = 0;
        return;
    }

    // save data
    Buffer.BlockCopy(binary, 4, voiceBuffer, totalLength, bodyLength);
    totalLength += bodyLength;

    string headerText = "";

    if (header.SequenceEqual(VOICE_HEADER_START))
    {
        headerText = "Start Recode Header: ";
    } else if (header.SequenceEqual(VOICE_HEADER_RECODING))
    {
        headerText = "Recoding Header: ";
    }
    else if (header.SequenceEqual(VOICE_HEADER_END))
    {
        headerText = "End Header: ";
        // 模拟处理数据
        // 处理之后清空buffer
        totalLength = 0;
    } else
    {
        headerText = "Wrong header : ";
    }

    receiveText = headerText + BitConverter.ToString(header) + "; body length : " + bodyLength + "; total length " + totalLength;
}
```
