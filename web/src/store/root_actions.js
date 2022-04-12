import Capabilities from '@/utils/capabilities';
import Unit from '@/utils/unit';
import LocalizationLoader from '@/localization/loader';
import { ScaleMode } from 'larksr_websdk';

export const RootActions/*: ActionTree<RootState, RootState>*/ = {
    resize({ commit, state, getters }) {
        // 浏览器窗口宽
        let screenW = getters.screenOrientation === 'landscape' ? 
                Unit.viewport().height :
                Unit.viewport().width;
        let pixUnit = screenW / 100;

        if (Capabilities.isMobile) {
            // resize rem.
            document.documentElement.style.fontSize = pixUnit + 'px';
        } else {
            // document.documentElement.style.fontSize = screenW / 200 + 'px';
            document.documentElement.style.fontSize = '9px';
        }
        commit('setMobilePixelUnit', pixUnit);
    },
    // keyboard,menu,joystick  同时只显示一个
    toggleVKeyboard({commit, state}) {
        commit('setVirtualKeyboard', !state.vkeyboard);
        if (state.vkeyboard) {
            commit('setMenu', false);
            commit('setJoyStick', false);
        }
    },
    toggleMenu({commit, state, dispatch}) {
        commit('setMenu', !state.menu);
        if (state.menu) {
            commit('setVirtualKeyboard', false);
            commit('setJoyStick', false);
        }
    },
    toggleJoyStick({commit, state, dispatch}) {
        commit('setJoyStick', !state.joystick);
        if (state.joystick) {
            commit('setVirtualKeyboard', false);
            commit('setMenu', false);
        }
    },
    toggleVMouseMode({ commit, state, getters,dispatch }) {
        if (state.vmouseMode == 'vmouse') {
            commit('setVmouseMode', 'pointer');
        } else {
            commit('setVmouseMode', 'vmouse');
        }
    },
    resetLocalization({commit, state, dispatch}) {
        dispatch('setLocalization', state.larksr.params.language);
    },
    setLocalization({commit, state, dispatch}, language) {
        LocalizationLoader.init(language);
        commit('setUI', LocalizationLoader.ui);
    },
    // scale mode
    resetScaleMode({ commit, state, getters,dispatch }) {
        commit('resetScaleMode');
        dispatch('resize');
    },
    setScaleMode({ commit, state, getters, dispatch }, mode) {
        commit('setScaleMode', mode);
        dispatch('resize');
    },
    setScaleToDefault({ commit, state, getters, dispatch }) {
        commit('resetScaleMode');
        dispatch('resize');
    },
    setScaleToFitScreen({ commit, state, getters, dispatch }) {
        commit('setScaleMode', ScaleMode.FIT_SCREEN);
        dispatch('resize');
    },
    setScaleToFillStretch({ commit, state, getters, dispatch }) {
        commit('setScaleMode', ScaleMode.FILL_STRETCH);
        dispatch('resize');
    },
    toggleScaleToFitScreen({ commit, state, getters, dispatch }) {
        const { larksr } = state;
        if (ScaleMode.FIT_SCREEN === larksr.params.scaleMode) {
            return;
        }
        if (getters.isChangedScaledMode) {
            commit('resetScaleMode');
        } else {
            commit('setScaleMode', ScaleMode.FIT_SCREEN);
        }
        dispatch('resize');
    },
    toggleScaleToFillStretch({ commit, state, getters,dispatch }) {
        const { larksr } = state;
        if (ScaleMode.FILL_STRETCH === larksr.params.scaleMode) {
            return;
        }
        if (getters.isChangedScaledMode) {
            commit('resetScaleMode');
        } else {
            commit('setScaleMode', ScaleMode.FILL_STRETCH);
        }
        dispatch('resize');
    },
};

export default RootActions;
