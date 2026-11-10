// Ethiopian (ዓመተ ምሕረት) ↔ Gregorian dates. Mirrors Extensions/EthiopianDate.cs — change both together.
// Twelve 30-day months plus ጳጉሜ (5 days, 6 when year % 4 === 3). Uses Julian day numbers; epoch JDN 1724221.
(function (global) {
    'use strict';

    const EPOCH_MINUS_ONE_YEAR = 1723856;
    const JDN_2000_01_01 = 2451545;
    const DAY_MS = 86400000;

    const monthNames = ['መስከረም', 'ጥቅምት', 'ኅዳር', 'ታኅሣሥ', 'ጥር', 'የካቲት', 'መጋቢት', 'ሚያዝያ', 'ግንቦት', 'ሰኔ', 'ሐምሌ', 'ነሐሴ', 'ጳጉሜ'];
    const weekdayNames = ['እሑድ', 'ሰኞ', 'ማክሰኞ', 'ረቡዕ', 'ሐሙስ', 'ዓርብ', 'ቅዳሜ'];

    // Day count of a yyyy-mm-dd string, independent of the browser's time zone.
    function jdnFromIso(iso) {
        const [y, m, d] = iso.split('-').map(Number);
        return Math.round((Date.UTC(y, m - 1, d) - Date.UTC(2000, 0, 1)) / DAY_MS) + JDN_2000_01_01;
    }

    function isoFromJdn(jdn) {
        const date = new Date(Date.UTC(2000, 0, 1) + (jdn - JDN_2000_01_01) * DAY_MS);
        return `${date.getUTCFullYear()}-${String(date.getUTCMonth() + 1).padStart(2, '0')}-${String(date.getUTCDate()).padStart(2, '0')}`;
    }

    /** yyyy-mm-dd (Gregorian) → { year, month, day } (Ethiopian), or null for an invalid value. */
    function fromGregorian(iso) {
        if (!/^\d{4}-\d{2}-\d{2}$/.test(iso || '')) return null;
        const offset = jdnFromIso(iso) - EPOCH_MINUS_ONE_YEAR;
        const r = ((offset % 1461) + 1461) % 1461;
        const n = (r % 365) + 365 * Math.floor(r / 1460);
        const year = 4 * Math.floor(offset / 1461) + Math.floor(r / 365) - Math.floor(r / 1460);
        return { year, month: Math.floor(n / 30) + 1, day: (n % 30) + 1 };
    }

    /** Ethiopian year/month/day → yyyy-mm-dd (Gregorian). */
    function toGregorian(year, month, day) {
        const jdn = EPOCH_MINUS_ONE_YEAR + 365 + 365 * (year - 1) + Math.floor(year / 4) + 30 * (month - 1) + day - 1;
        return isoFromJdn(jdn);
    }

    const isLeapYear = year => year % 4 === 3;
    const daysInMonth = (year, month) => month < 13 ? 30 : (isLeapYear(year) ? 6 : 5);
    const pad = value => String(value).padStart(2, '0');

    /** 17/08/2018 */
    function format(iso) {
        const e = fromGregorian(iso);
        return e ? `${pad(e.day)}/${pad(e.month)}/${e.year}` : '-';
    }

    /** ሰኞ፣ 17 ሚያዝያ 2018 */
    function formatLong(iso) {
        const e = fromGregorian(iso);
        if (!e) return '-';
        const weekday = new Date(`${iso}T00:00:00Z`).getUTCDay();
        return `${weekdayNames[weekday]}፣ ${e.day} ${monthNames[e.month - 1]} ${e.year}`;
    }

    function todayIso() {
        const now = new Date();
        return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
    }

    global.EthiopianDate = { fromGregorian, toGregorian, isLeapYear, daysInMonth, format, formatLong, todayIso, monthNames, weekdayNames };
})(typeof window !== 'undefined' ? window : globalThis);
