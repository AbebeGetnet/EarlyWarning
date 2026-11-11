// Beneficiary tables (አባወራ/እማወራ, ቤተሰብ, ህፃናት, ወጣቶች, አረጋውያን, አካል ጉዳተኞች).
// Mirrors Extensions/Beneficiaries.cs — change both together.
//
// Household heads and family members are the people counted (primary rows). Children, youth and elderly
// (age) and disabled (health status) are breakdowns of those same people: they are never added to the
// total, and per sex the age groups together, and the disabled, may not exceed the primary total.
window.Beneficiaries = (function () {
    const PRIMARY = /(HouseHold|Household|Family)/i;
    const AGE = /(Child|Youth|Elderly)/i;
    const HEALTH = /Disabled/i;
    const NOTE = 'ህፃናት፣ ወጣቶች፣ አረጋውያን እና አካል ጉዳተኞች ከአባወራ/እማወራና ቤተሰብ ውስጥ የተካተቱ ናቸው፤ በድምሩ ላይ አይደመሩም።';

    const value = input => parseInt(input?.value, 10) || 0;
    const kind = row => {
        const name = row.querySelector('.val-male, .val-female')?.name || '';
        return PRIMARY.test(name) ? 'primary' : AGE.test(name) ? 'age' : HEALTH.test(name) ? 'health' : null;
    };

    // Labels the table once so users see which rows make up the total.
    function decorate(table) {
        if (table.dataset.beneficiaryTable) return;
        table.dataset.beneficiaryTable = '1';
        table.querySelectorAll('tbody tr').forEach(row => {
            const k = kind(row);
            if (k === 'age' || k === 'health') row.classList.add('beneficiary-breakdown');
        });
        const footLabel = table.querySelector('tfoot tr td');
        if (footLabel) footLabel.textContent = 'ጠቅላላ ድምር (አባወራ/እማወራ + ቤተሰብ)';
        const foot = table.querySelector('tfoot');
        if (foot && table.querySelector('.beneficiary-breakdown')) {
            const note = document.createElement('tr');
            note.className = 'beneficiary-note';
            note.innerHTML = `<td colspan="${table.querySelector('thead tr')?.children.length || 4}" class="small text-muted fw-normal text-start"></td>`;
            note.firstElementChild.textContent = NOTE;
            foot.appendChild(note);
        }
    }

    function calculate(table) {
        decorate(table);
        const sums = { male: { primary: 0, age: 0, health: 0 }, female: { primary: 0, age: 0, health: 0 } };
        const inputs = { male: [], female: [] };
        table.querySelectorAll('tbody tr').forEach(row => {
            const male = row.querySelector('.val-male');
            const female = row.querySelector('.val-female');
            const rowTotal = row.querySelector('.row-total');
            if (rowTotal) rowTotal.innerText = (value(male) + value(female)).toLocaleString();
            const k = kind(row);
            if (!k) return;
            sums.male[k] += value(male);
            sums.female[k] += value(female);
            if (k !== 'primary') {
                if (male) inputs.male.push({ input: male, k });
                if (female) inputs.female.push({ input: female, k });
            }
        });

        ['male', 'female'].forEach(sex => {
            const s = sums[sex];
            const label = sex === 'male' ? 'ወንድ' : 'ሴት';
            inputs[sex].forEach(({ input, k }) => {
                const over = s[k] > s.primary;
                const message = !over ? '' : k === 'age'
                    ? `${label}፡ ህፃናት + ወጣቶች + አረጋውያን (${s.age.toLocaleString()}) ከአባወራ/እማወራ + ቤተሰብ ድምር (${s.primary.toLocaleString()}) መብለጥ አይችልም።`
                    : `${label}፡ አካል ጉዳተኞች (${s.health.toLocaleString()}) ከአባወራ/እማወራ + ቤተሰብ ድምር (${s.primary.toLocaleString()}) መብለጥ አይችልም።`;
                // Leave range errors set by other scripts alone; only own the distribution message.
                if (message || input.dataset.beneficiaryError) input.setCustomValidity(message);
                if (message) input.dataset.beneficiaryError = '1'; else delete input.dataset.beneficiaryError;
                input.classList.toggle('is-invalid', !!message);
                input.title = message;
            });
        });

        const set = (selector, n) => { const cell = table.querySelector(selector); if (cell) cell.innerText = n.toLocaleString(); };
        set('.total-male', sums.male.primary);
        set('.total-female', sums.female.primary);
        set('.grand-total', sums.male.primary + sums.female.primary);
        return sums;
    }

    return { calculate };
})();
