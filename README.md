# 使用数据通道发送网页音频流到云端应用

## node 环境搭建

1. 安装 nodejs v15.14.0 （保持环境一至，都安装 v15.14.0 版本）

[下载 v15.14.0](https://nodejs.org/download/release/v15.14.0/)

2. 安装 cnpm (npm 淘宝镜像，速度快)

安装完 nodejs 打开命令行执行下面命令

```cmd
npm install -g cnpm --registry=https://registry.npmmirror.com
```

3. 安装依赖工具

* yarn

```cmd
cnpm install -g yarn
```

## 开始Web项目, 进入 web 目录下

命令行进入进入 web 目录下，执行:

```cmd
yarn install
```

### 调试

```cmd
yarn serve
```

yarn serve 执行成功后命令行显示如下：

```cmd
  App running at:
  - Local:   http://localhost:8080/
  - Network: http://192.168.0.161:8080/
```

浏览器打开下面连接 http://localhost:8080/

> 录音功能只支持 https 或者 localhost，不要打开 ip 开头的连接。

### 编译输出

```
yarn build
```

### 填写授权码

进入 web目录下，web/src/App.vue，找到 87 行

```javascript
larksr.initSDKAuthCode("SDK 授权码，联系 business@pingxingyun.com 获取,注意是 SDK 本身的授权码，不是服务器上的授权")
```

### 填写 APPID

进入 web目录下，web/src/App.vue，找到 93-95 行

```javascript
larksr.connectWithPxyHost({
    appliId: "您的APPID",
});
```

> 如果是私有部署要在之前填写服务器地址，78 行

### 发送语音相关

进入 web目录下，web/src/Components/voice/voice.vue, 主要参考 startRecode, stopRecode, recodeTimeout, sendRecoderBuffer 等方法

实际发送音频的格式如下

```javascript
// 音频格式为 wav, sampleRate: 16000 sampleBits: 16,
// 0x0 0x0 0x0 0x1---------音频输入开始
// 0x0 0x0 0x0 0x2---------音频输入中(用户录音时循环发送，初步定义为 1s 中切片一次即发送一次音频)
// 0x0 0x0 0x0 0x3---------音频输入结束(用户本次输入结束)
```

请求权限参考 requestAudio 方法。

> 本例子中采集音频使用 https://github.com/Zousdie/recorderx

数据通道发送字节数据, 在 sendRecoderBuffer 方法中：

```javascript
// send array buffer to datachannel.
this.larksr?.sendBinaryToDataChannel(uint8array);
```

数据通道监听回调，参考 web/src/App.vue

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

## Unity3D 应用

首先导入 lark_datachannel_unity3d_export_unity2019.unitypackage 启用数据通道并接受音频参考 Assets/Scenes/Manager.cs

启用数据通道

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

接受语音数据

```cs
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
```
