<template>
  <div id="app" ref="appContainer">
    <!-- 手机端 UI -->
    <MobileIndex v-if="cloudReady && isMobile"></MobileIndex>
    <Alert />
    <Notify />
    <Toast />
    <Confirm />
    <RttInfo v-if="cloudReady" />
    <Menu />
    <ControlBall v-if="cloudReady && !isMobile" />
    <States />
    <Voice v-if="cloudReady" />
  </div>
</template>

<script>
import { LarkSR } from "larksr_websdk";
import MobileIndex from "./components/mobile/index";
import { mapState, mapGetters, mapMutations, mapActions } from "vuex";
import Unit from './utils/unit';
import Alert               from './components/alert/alert';
import Notify              from './components/notify/notify';
import Toast               from './components/toast/toast';
import Confirm             from './components/confirm/confirm';
import RttInfo             from './components/rttinfo/rttinfo';
import Menu                from './components/menu/menu';
import ControlBall         from './components/control_ball/control_ball'; 
import States              from './components/states_modal/states_modal';
import Voice               from './components/voice/voice.vue';

export default {
  name: "App",
  components: {
    MobileIndex,
    Alert,
    Notify,
    Toast,
    Confirm,
    RttInfo,
    Menu,
    ControlBall,
    States,
    Voice,
  },
  data() {
    return {
      appContainer: null,
      cloudReady: false,
    };
  },
  computed: {
    ...mapState({
      isMobile: (state) => state.isMobile,
    }),
  },
  methods: {
    ...mapMutations({
        setLarksr: "setLarksr",
        setAggregatedStats: "setAggregatedStats",
    }),
    ...mapActions({
      "resize": "resize",
      'toast': 'toast/toast',
      'notify': 'notifyBar/notify',
      'alert': 'modalAlert/showModalAlert',
      'confirm': 'modalConfirm/showModalConfirm',
      'resetLocalization': 'resetLocalization',
    }),
  },
  mounted() {
    // 直接调用进入应用接口创建实例，自动配置连接云端资源
    const larksr = new LarkSR({
        rootElement: this.$refs["appContainer"],
        // 服务器地址,实际使用中填写您的服务器地址
        // 如：http://222.128.6.137:8181/
        // 托管平台可留空
        serverAddress: "http://192.168.0.161:8181/",
        // 视频缩放模式，默认保留宽高比，不会拉伸并完整显示在容器中
        // scaleMode: "contain",
        // show log
        // logLevel: 'warn',
    });
    
    // 初始化您的授权ID
    // "SDK 授权码，联系 business@pingxingyun.com 获取,注意是 SDK 本身的授权码，不是服务器上的授权"
    larksr.initSDKAuthCode("SDK 授权码，联系 business@pingxingyun.com 获取,注意是 SDK 本身的授权码，不是服务器上的授权")
    .then(() => {
      // start connect;
      // 私有部署进入应用
      // larksr.connect({
      // 平行云托管平台进入应用
      larksr.connectWithPxyHost({
        appliId: "您的APPID",
      })
      .then(() => {
        console.log('enter success');
      })
      .catch((e) => {
        console.error(e);
        alert(JSON.stringify(e));
      }); 
    })
    .catch((e) => {
      console.error(e);
      alert(JSON.stringify(e));
    });

    // 监听连接成功事件
    larksr.on("connect", (e) => {
      console.log("LarkSRClientEvent CONNECT", e);
    });

    larksr.on("gotremotesteam", (e) => {
      console.log("LarkSRClientEvent gotremotesteam", e);
    });

    larksr.on("meidaloaded", (e) => {
      console.log("LarkSRClientEvent meidaloaded", e);
      this.cloudReady = true;
    });

    larksr.on("mediaplaysuccess", (e) => {
      console.log("LarkSRClientEvent mediaplaysuccess", e);
    });

    larksr.on("mediaplayfailed", (e) => {
      console.log("LarkSRClientEvent mediaplayfailed", e);
      this.alert({des: "开始"})
      .then(() => {
          larksr.videoElement.sountPlayout();
          larksr.videoElement.playVideo();
      });
    });

    larksr.on("meidaplaymute", (e) => {
      console.log("LarkSRClientEvent meidaplaymute", e);
      this.toast({text: '点击屏幕中心打开音频', position: 2, level: 3});
    });

    larksr.on("peerstatusreport", (e) => {
      // console.log("LarkSRClientEvent peerstatusreport", e);
      this.setAggregatedStats(e.data);
    });

    larksr.on('error', (e) => {
        console.error("LarkSRClientEvent error", e.message); 
        this.alert({des: e.message, code: e.code})
        .then(() => {
            Unit.quit();
        });
    });   

    larksr.on('info', (e) => {
        console.log("LarkSRClientEvent info", e); 
        this.toast({text: e.message});
    });

    
    larksr.on("datachannelopen", (e) => {
      console.log("LarkSRClientEvent datachannelopen", e);
    });


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

    console.log("load appli success", larksr);

    // reset states.
    this.setLarksr(larksr);
    this.resetLocalization();
    this.resize();

    // this.alert({des: 1});
    // this.confirm({des:"22"});
    console.log("ref", this.$refs["appContainer"]);

    let resizeTimeput = null;
    window.addEventListener("resize", () => {
        if (resizeTimeput) {
            window.clearTimeout(resizeTimeput);
        }
        resizeTimeput = window.setTimeout(() => {
            this.resize();
            resizeTimeput = null;
        }, 100);
    });
    window.addEventListener("orientationchange", () => {
        if (resizeTimeput) {
            window.clearTimeout(resizeTimeput);
        }
        resizeTimeput = window.setTimeout(() => {
            this.resize();
            resizeTimeput = null;
        }, 100);
    });
    this.resize();
  },
  beforeUnmount() {
    // 主动关闭
    this.larksr?.close();
  },
};
</script>

<style></style>
