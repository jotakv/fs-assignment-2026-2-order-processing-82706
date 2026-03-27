window.sportsStoreStorage = {
    get(key) {
        return window.sessionStorage.getItem(key);
    },
    set(key, value) {
        window.sessionStorage.setItem(key, value);
    },
    remove(key) {
        window.sessionStorage.removeItem(key);
    }
};
