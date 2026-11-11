// Offline data collection.
//
// A report finished without a connection goes into this browser's outbox (localStorage, one outbox per
// user) and is posted to the server as soon as it can be reached: on page load, when the browser comes
// back online, and every minute. The server answers with JSON when the request carries X-Offline-Sync
// (RegisterationWizardController.SubmissionRejected). A report the server rejects is marked "failed"
// and the wizard shows the reasons when it is opened again.
//
// Over HTTPS (or on localhost) a service worker (wwwroot/sw.js) also keeps pages and files, so the
// wizard opens without a connection. Browsers refuse service workers on plain http:// addresses; there
// the wizard must already be open when the connection drops.
window.EWOffline = (function () {
    const meta = name => document.querySelector(`meta[name="${name}"]`)?.content || '';
    const base = meta('ew-base') || '/';
    const user = meta('ew-user');
    const tokenUrl = meta('ew-token-url');
    const outboxKey = `ew-outbox:${user}`;
    let flushing = false;
    let loginNeeded = false;
    let reachable = navigator.onLine;

    const read = () => { try { return JSON.parse(localStorage.getItem(outboxKey)) || []; } catch { return []; } };
    const write = list => { try { localStorage.setItem(outboxKey, JSON.stringify(list)); return true; } catch { return false; } };

    // One entry per report (key = the wizard's local draft key, i.e. woreda + week); a newer one replaces it.
    function queue(entry) {
        const list = read().filter(x => x.key !== entry.key);
        list.push({ ...entry, status: 'pending', queuedAt: new Date().toISOString() });
        const saved = write(list);
        render();
        if (saved) setTimeout(flush, 0);
        return saved;
    }
    const find = key => read().find(x => x.key === key);
    function remove(key) { write(read().filter(x => x.key !== key)); render(); }
    function update(key, changes) { write(read().map(x => x.key === key ? { ...x, ...changes } : x)); }

    async function freshToken() {
        const response = await fetch(tokenUrl, { cache: 'no-store', headers: { Accept: 'application/json' } });
        // Signed out: the request is redirected to the login page.
        if (response.redirected || !response.ok || !(response.headers.get('content-type') || '').includes('json'))
            throw Object.assign(new Error('login'), { login: true });
        return (await response.json()).token;
    }

    async function flush() {
        if (flushing || !user || !tokenUrl) return;
        if (!read().some(x => x.status === 'pending')) return;
        flushing = true;
        try {
            let token;
            try { token = await freshToken(); }
            catch (error) {
                if (error.login) loginNeeded = true; else reachable = false;
                return;
            }
            loginNeeded = false;
            reachable = true;
            for (const entry of read().filter(x => x.status === 'pending')) {
                const body = new FormData();
                entry.fields.forEach(([name, value]) => body.append(name, value));
                body.append('__RequestVerificationToken', token);
                let response;
                try {
                    response = await fetch(entry.url, { method: 'POST', body, headers: { 'X-Offline-Sync': '1', Accept: 'application/json' } });
                } catch { reachable = false; return; }   // connection dropped again; try later
                const result = await response.json().catch(() => null);
                if (response.ok && result?.ok) {
                    try { localStorage.removeItem(entry.key); } catch { /* ignore */ }
                    remove(entry.key);
                    notice(`✓ ${entry.label || 'ሪፖርቱ'} ተልኳል።`);
                    document.dispatchEvent(new CustomEvent('ew-outbox-sent', { detail: { key: entry.key, message: result.message } }));
                } else if (result?.errors) {
                    update(entry.key, { status: 'failed', errors: result.errors });
                    document.dispatchEvent(new CustomEvent('ew-outbox-failed', { detail: { key: entry.key, errors: result.errors } }));
                } else {
                    if (response.redirected) loginNeeded = true;
                    return;   // server error: leave it queued
                }
            }
        } finally {
            flushing = false;
            render();
        }
    }

    // A small status pill, bottom left: offline, waiting to send, needs login, or sent back with errors.
    let pill;
    function render() {
        if (!document.body) return;
        if (!pill) {
            pill = document.createElement('div');
            pill.id = 'ewOfflineStatus';
            pill.setAttribute('role', 'status');
            pill.style.cssText = 'position:fixed;left:12px;bottom:12px;z-index:2000;max-width:min(420px,calc(100vw - 24px));font-size:.875rem;box-shadow:0 2px 8px rgba(0,0,0,.2)';
            document.body.appendChild(pill);
        }
        const list = read();
        const pending = list.filter(x => x.status === 'pending');
        const failed = list.filter(x => x.status === 'failed');
        const offline = !navigator.onLine || !reachable;
        const parts = [];
        let tone = 'secondary';
        if (offline) parts.push('<i class="fas fa-wifi"></i> ከመስመር ውጭ ነዎት — የሚሞሉት መረጃ በዚህ መሣሪያ ላይ ይቀመጣል።');
        if (pending.length) {
            tone = offline ? 'secondary' : 'info';
            parts.push(`${pending.length} ሪፖርት ለመላክ በመጠባበቅ ላይ${offline ? '፤ ግንኙነት ሲመለስ በራሱ ይላካል።' : ''}`
                + (offline ? '' : ' <button type="button" class="btn btn-sm btn-light py-0 ms-1" data-ew-send>አሁን ላክ</button>'));
        }
        if (loginNeeded && pending.length) {
            tone = 'warning';
            parts.push(`የተቀመጡትን ሪፖርቶች ለመላክ እባክዎ <a href="${base}Identity/Account/Login">ይግቡ</a>።`);
        }
        failed.forEach(x => {
            tone = 'danger';
            parts.push(`${escapeHtml(x.label || 'ሪፖርት')} አልተላከም፤ <a class="text-white fw-bold" href="${x.openUrl}">ከፍተው ያስተካክሉ</a>።`);
        });
        pill.className = `alert alert-${tone} py-2 px-3 mb-0`;
        pill.hidden = parts.length === 0;
        pill.innerHTML = parts.join('<br>');
        pill.querySelector('[data-ew-send]')?.addEventListener('click', flush);
    }

    function notice(message) {
        if (window.toastr) window.toastr.success(message); else console.info(message);
    }
    const escapeHtml = text => String(text).replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

    // Called by pages that notice the server is (un)reachable through their own requests.
    function setReachable(value) {
        if (reachable === value) return;
        reachable = value;
        render();
        if (value) flush();
    }

    async function clearPageCaches() {
        if (!('caches' in window)) return;
        const keys = await caches.keys();
        await Promise.all(keys.filter(k => k.startsWith('ew-') && !k.endsWith('-assets')).map(k => caches.delete(k)));
    }

    if ('serviceWorker' in navigator && window.isSecureContext) {
        window.addEventListener('load', () => navigator.serviceWorker.register(`${base}sw.js`, { scope: base })
            .catch(error => console.warn('Offline support unavailable:', error)));
    }
    // Pages kept for offline use belong to the signed-in user; drop them on logout (the outbox stays,
    // it is kept per user and only that user sends it).
    document.addEventListener('submit', event => {
        if (/logout/i.test(event.target?.action || '')) clearPageCaches();
    }, true);
    window.addEventListener('online', () => { reachable = true; render(); flush(); });
    window.addEventListener('offline', () => { reachable = false; render(); });
    window.addEventListener('storage', event => { if (event.key === outboxKey) render(); });
    document.addEventListener('DOMContentLoaded', () => { render(); flush(); });
    setInterval(flush, 60000);

    return { queue, find, remove, flush, setReachable };
})();
