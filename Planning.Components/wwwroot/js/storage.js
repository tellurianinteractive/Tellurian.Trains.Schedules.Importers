// IndexedDB-backed key/value store used by BrowserStorageService.
// Values are strings; the .NET layer serialises objects to JSON before storing.
// IndexedDB is used instead of localStorage because a serialised schedule can
// exceed the ~5 MB localStorage quota.

const DB_NAME = 'planning';
const STORE = 'keyval';
const VERSION = 1;

let dbPromise;

function openDb() {
    dbPromise ??= new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, VERSION);
        request.onupgradeneeded = () => {
            const db = request.result;
            if (!db.objectStoreNames.contains(STORE)) {
                db.createObjectStore(STORE);
            }
        };
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
    return dbPromise;
}

async function run(mode, operation) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const transaction = db.transaction(STORE, mode);
        const request = operation(transaction.objectStore(STORE));
        transaction.oncomplete = () => resolve(request.result);
        transaction.onerror = () => reject(transaction.error);
        transaction.onabort = () => reject(transaction.error);
    });
}

export async function get(key) {
    const value = await run('readonly', store => store.get(key));
    return value ?? null;
}

export function set(key, value) {
    return run('readwrite', store => store.put(value, key));
}

export function remove(key) {
    return run('readwrite', store => store.delete(key));
}
