// LocalStorage.jslib - Unity WebGL용 브라우저 localStorage + AudioContext 연동
mergeInto(LibraryManager.library, {

    SaveToLocalStorage: function(key, value) {
        try {
            localStorage.setItem(UTF8ToString(key), UTF8ToString(value));
        } catch(e) {
            console.warn('[LocalStorage] SaveToLocalStorage 실패:', e);
        }
    },

    LoadFromLocalStorage: function(key) {
        try {
            var val = localStorage.getItem(UTF8ToString(key));
            if (val === null) val = '';
            var len = lengthBytesUTF8(val) + 1;
            var buf = _malloc(len);
            stringToUTF8(val, buf, len);
            return buf;
        } catch(e) {
            console.warn('[LocalStorage] LoadFromLocalStorage 실패:', e);
            var empty = _malloc(1);
            HEAP8[empty] = 0;
            return empty;
        }
    },

    HasLocalStorageKey: function(key) {
        try {
            return localStorage.getItem(UTF8ToString(key)) !== null ? 1 : 0;
        } catch(e) {
            return 0;
        }
    },

    RemoveFromLocalStorage: function(key) {
        try {
            localStorage.removeItem(UTF8ToString(key));
        } catch(e) {
            console.warn('[LocalStorage] RemoveFromLocalStorage 실패:', e);
        }
    },

    UnlockWebAudio: function() {
        if (typeof WEBAudio !== 'undefined' && WEBAudio.audioContext) {
            WEBAudio.audioContext.resume().then(function() {
                console.log('[WebGL] AudioContext resumed');
            }).catch(function(e) {
                console.warn('[WebGL] AudioContext resume 실패:', e);
            });
        }
    }

});
