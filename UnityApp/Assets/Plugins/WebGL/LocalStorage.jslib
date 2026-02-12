mergeInto(LibraryManager.library, {

    SaveToLocalStorage: function(keyPtr, valuePtr) {
        var key = UTF8ToString(keyPtr);
        var value = UTF8ToString(valuePtr);
        localStorage.setItem(key, value);
    },

    LoadFromLocalStorage: function(keyPtr) {
        var key = UTF8ToString(keyPtr);
        var value = localStorage.getItem(key);
        if (value === null) {
            value = "";
        }
        var bufferSize = lengthBytesUTF8(value) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(value, buffer, bufferSize);
        return buffer;
    },

    HasLocalStorageKey: function(keyPtr) {
        var key = UTF8ToString(keyPtr);
        return localStorage.getItem(key) !== null ? 1 : 0;
    },

    RemoveFromLocalStorage: function(keyPtr) {
        var key = UTF8ToString(keyPtr);
        localStorage.removeItem(key);
    },

    UnlockWebAudio: function() {
        // 브라우저 오디오 컨텍스트 잠금 해제
        // Unity WebGL은 WEBAudio 객체를 통해 오디오 관리
        if (typeof WEBAudio !== 'undefined' && WEBAudio.audioContext) {
            WEBAudio.audioContext.resume();
        }
    }
});
