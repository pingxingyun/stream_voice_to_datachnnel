<template>
    <div class="voice">
        <div class="voice-icon"
            :style="positionStyle"
            draggable="false"
        >
            <div 
                :class="start ? 'voice-icon-on' :  'voice-icon-off'"
                 @mousedown.stop.prevent="startRecode"
                 @mouseup.stop.prevent="stopRecode" 
                 @mouseleave.stop.prevent="stopRecode"
                 @touchstart.stop.prevent="startRecode"
                 @touchend.stop.prevent="stopRecode"
                 @touchcancel="stopRecode"
            >
            </div>
        </div>
    </div>
</template>
<script>
import {
    mapState,
    mapGetters,
    mapMutations,
    mapActions,
}                          from 'vuex';
import Unit                from '../../utils/unit';
import Recorderx, { 
  ENCODE_TYPE,
  RECORDER_STATE,
} from "recorderx";

export default {
    data() {
        return {
            start: false,
            position: {
                x: 0,
                y: 0,
            },
            recoder: null,
        }
    },
    computed: {
        positionStyle() {
            if (this.position.x != 0 || this.position.y != 0) {
                return {
                    top: this.position.y + "px",
                    left: this.position.x + "px",
                }
            } else {
                return {
                    top: this.viewPort.height - 150 + "px",
                    left: (this.viewPort.width / 2 - Unit.getMobliePixelWidth(93, this.mobilePixelUnit) / 2) + "px",
                };
            }
        },
        ...mapState({
            // formate states
            larksr: state => state.larksr,
            ui: state => state.ui,
            mobilePixelUnit: (state) => state.mobilePixelUnit,
        }),
        ...mapGetters({
            screenOrientation: 'screenOrientation',
            appliParams: 'appliParams',
            viewPort: 'viewPort',
        })
    },
    methods: {
        // 开始录制
        startRecode() {
            console.log('start recode');
            this.start = true;

            this.recoder = new Recorderx();
            // start recorderx
            this.recoder.start()
            .then(() => {
                if (this.start) {
                    console.log("start recording");
                    this.sendRecoderBuffer(0x1);
                    this.recodeTimeout();
                }
            })
            .catch(error => {
                console.log("Recording failed.", error);
            });
        },
        stopRecode() {
            console.log('stop recode');
            this.start = false;
            if (this.recodeTimer) {
                window.clearInterval(this.recodeTimer);
                this.recodeTimer = null;
            }
            // pause recorderx
            if (this.recoder && this.recoder.state == RECORDER_STATE.RECORDING) {
                console.log('stop recode', this.recoder.state, this.recoder.ctx);
                this.sendRecoderBuffer(0x3);
                this.recoder?.pause();
                this.recoder?.clear();
            }
            this.recoder = null;
        },
        pauseRecode() {
            // pause recorderx
            this.recoder?.pause();
        },
        recodeTimeout() {
            if (this.recodeTimer) {
                window.clearInterval(this.recodeTimer);
            }
            this.recodeTimer = window.setInterval(() => {
                // recodeing
                console.log('recoder update...');
                this.sendRecoderBuffer(0x2);
            }, 1000);
        },
        // 0x0 0x0 0x0 0x1---------音频输入开始
        // 0x0 0x0 0x0 0x2---------音频输入中(用户录音时循环发送，初步定义为 1s 中切片一次即发送一次音频)
        // 0x0 0x0 0x0 0x3---------音频输入结束(用户本次输入结束)
        sendRecoderBuffer(state) {
            // get recoder
            if (this.recoder && this.recoder.state == RECORDER_STATE.RECORDING) {
                let buffer = this.recoder.getRecord({
                    encodeTo: "wav",
                    compressible: true,
                });
                console.log('send recode ', state, buffer, buffer.arrayBuffer());
                const blob = new Blob([new Uint8Array([0x0, 0x0, 0x0, state]), buffer]);
                blob.arrayBuffer()
                .then((value) => {
                    const uint8array = new Uint8Array(value);
                    console.log("send to datachannel buffers", uint8array);
                    // send array buffer to datachannel.
                    this.larksr?.sendBinaryToDataChannel(uint8array);
                })
                .catch((e) => {
                    console.warn('connect buffer failed', e);
                })
                this.recoder.clear();
            }
        },
        // 请求权限
        requestAudio() {
            navigator.mediaDevices.getUserMedia({
                audio: true,
                video: false,
            })
            .then((stream) => {
                stream.getTracks().forEach(track => track.stop());
            })
            .catch((e) => {
                console.error('request media failed ', e);
            })
        },
    },
    mounted() {
        this.requestAudio();
    },
}
</script>
<style lang="scss" scoped>
@import 'voice.scss';
</style>