// Service worker: keeps the pages and files a data encoder has opened so they open again without a
// connection. Registered by wwwroot/js/offline.js (HTTPS or localhost only — browsers' rule).
//
//  - Pages: network first; the last good copy is used when the server cannot be reached. Pages that
//    must not be replayed (login, a redirect, X-Offline-Cache: no) are never kept.
//  - Scripts, styles, fonts, images (own and CDN): served from the cache, refreshed in the background.
//    The app's own files carry ?v=<hash> (asp-append-version), so a changed file is a new URL.
//  - Last week's prices for the wizard: network first, cached copy offline.
//  - Everything else (other JSON, every POST) goes straight to the network; the wizard's outbox
//    handles submitting offline.
// Change VERSION to drop everything cached by an older version.
const VERSION = 'ew-v1';
const PAGES = `${VERSION}-pages`;
const DATA = `${VERSION}-data`;
const ASSETS = `${VERSION}-assets`;
const SCOPE = new URL(self.registration.scope).pathname;
const OFFLINE_PAGE = `${SCOPE}offline.html`;

self.addEventListener('install', event => {
    event.waitUntil(caches.open(PAGES).then(cache => cache.add(OFFLINE_PAGE)).then(() => self.skipWaiting()));
});

self.addEventListener('activate', event => {
    event.waitUntil(caches.keys()
        .then(keys => Promise.all(keys.filter(k => k.startsWith('ew-') && !k.startsWith(`${VERSION}-`)).map(k => caches.delete(k))))
        .then(() => self.clients.claim()));
});

const keepable = response => response && response.ok && !response.redirected && response.type === 'basic'
    && response.headers.get('X-Offline-Cache') !== 'no';

async function networkFirstPage(request) {
    const cache = await caches.open(PAGES);
    try {
        const response = await fetch(request);
        if (keepable(response)) cache.put(request, response.clone());
        return response;
    } catch {
        return (await cache.match(request))
            || (await cache.match(request, { ignoreSearch: true }))
            || (await cache.match(OFFLINE_PAGE))
            || Response.error();
    }
}

async function networkFirstData(request) {
    const cache = await caches.open(DATA);
    try {
        const response = await fetch(request);
        if (keepable(response)) cache.put(request, response.clone());
        return response;
    } catch {
        return (await cache.match(request)) || Response.error();
    }
}

async function staleWhileRevalidate(event) {
    const cache = await caches.open(ASSETS);
    const cached = await cache.match(event.request);
    const refresh = fetch(event.request).then(response => {
        // Opaque responses are CDN files fetched without CORS; they are fine to replay.
        if (response && (response.ok || response.type === 'opaque')) cache.put(event.request, response.clone());
        return response;
    });
    if (cached) {
        event.waitUntil(refresh.catch(() => undefined));
        return cached;
    }
    return refresh;
}

self.addEventListener('fetch', event => {
    const request = event.request;
    const url = new URL(request.url);
    const sameOrigin = url.origin === self.location.origin;

    // Signing out: forget the pages kept for this user.
    if (request.method === 'POST' && sameOrigin && /\/account\/logout/i.test(url.pathname)) {
        event.waitUntil(Promise.all([caches.delete(PAGES), caches.delete(DATA)]));
        return;
    }
    if (request.method !== 'GET') return;
    if (sameOrigin && /\/identity\//i.test(url.pathname)) return;

    if (request.mode === 'navigate' && sameOrigin) {
        event.respondWith(networkFirstPage(request));
    } else if (['script', 'style', 'font', 'image'].includes(request.destination)) {
        event.respondWith(staleWhileRevalidate(event));
    } else if (sameOrigin && /\/GetPreviousWeekPrices$/i.test(url.pathname)) {
        event.respondWith(networkFirstData(request));
    }
});
