import '@styles/index.css';
import { BDOSQLDropdown } from '@components/custom-dropdown/CustomDropdown';
import { CustomActionButton } from '@components/custom-action-button/CustomActionButton';
import { EmailLookupFormComponent } from '@components/email-lookup/EmailLookup';
import { FieldGroupSeparator } from '@components/field-group-separator/FieldGroupSeparator';

function copyTextToClipboard(text: string): void {
    if (!text) {
        return;
    }

    if (navigator.clipboard?.writeText) {
        void navigator.clipboard.writeText(text);
        return;
    }

    const probe = document.createElement('textarea');
    probe.value = text;
    probe.style.position = 'fixed';
    probe.style.left = '-9999px';
    document.body.appendChild(probe);
    probe.focus();
    probe.select();
    document.execCommand('copy');
    document.body.removeChild(probe);
}

function readEmailAddress(): string {
    const directMatch = document.querySelector(
        'input[name$="EmailAddress"], input[id*="EmailAddress"], input[aria-label="Email Address"]'
    ) as HTMLInputElement | null;

    if (directMatch?.value?.trim()) {
        return directMatch.value.trim();
    }

    const labels = Array.from(document.querySelectorAll('label'));
    const emailLabel = labels.find(labelElement =>
        (labelElement.textContent || '').trim().toLowerCase().startsWith('email address')
    );

    if (!emailLabel) {
        return '';
    }

    const forId = emailLabel.getAttribute('for');
    if (forId) {
        const labelledInput = document.getElementById(forId) as HTMLInputElement | null;
        if (labelledInput?.value?.trim()) {
            return labelledInput.value.trim();
        }
    }

    const formFieldContainer = emailLabel.closest('div');
    const nestedInput = formFieldContainer?.querySelector('input') as HTMLInputElement | null;
    return nestedInput?.value?.trim() || '';
}

function writeResultToTextarea(result: string): void {
    const results = document.querySelector(
        'textarea[name$="Results"], textarea[id*="Results"], textarea[aria-label="Results"]'
    ) as HTMLTextAreaElement | null;

    if (!results) {
        return;
    }

    results.value = result;
    results.dispatchEvent(new Event('input', { bubbles: true }));
    results.dispatchEvent(new Event('change', { bubbles: true }));
}

function clearGdprFormData(): void {
    const emailInput = document.querySelector(
        'input[name$="EmailAddress"], input[id*="EmailAddress"], input[aria-label="Email Address"]'
    ) as HTMLInputElement | null;

    if (emailInput) {
        emailInput.value = '';
        emailInput.dispatchEvent(new Event('input', { bubbles: true }));
        emailInput.dispatchEvent(new Event('change', { bubbles: true }));
    }

    writeResultToTextarea('');
}

async function executeGdprAction(route: string, method: 'GET' | 'DELETE'): Promise<void> {
    const email = readEmailAddress();
    if (!email) {
        writeResultToTextarea('Email address is required.');
        return;
    }

    const url = `/admin-api/custom-action/${route}?email=${encodeURIComponent(email)}`;

    try {
        const response = await fetch(url, { method, credentials: 'include' });
        const payload = await response.json() as { result?: string };
        writeResultToTextarea(payload.result || (response.ok ? 'Action completed.' : 'Action failed.'));
    } catch {
        writeResultToTextarea('Action failed.');
    }
}

function initGdprActionHandlers(): void {
    document.addEventListener('click', (event) => {
        const target = event.target as HTMLElement | null;
        const button = target?.closest('button');
        if (!button) {
            return;
        }

        const title = button.getAttribute('title') || '';

        if (title === 'Copy GDPR results') {
            const results = document.querySelector(
                'textarea[name$="Results"], textarea[id*="Results"], textarea[aria-label="Results"]'
            ) as HTMLTextAreaElement | null;
            copyTextToClipboard(results?.value?.trim() ?? '');
            return;
        }

        if (title === 'Search contact and load text') {
            event.preventDefault();
            event.stopPropagation();
            void executeGdprAction('searchtotext', 'GET');
            return;
        }

        if (title === 'Search contact and load XML') {
            event.preventDefault();
            event.stopPropagation();
            void executeGdprAction('searchtoxml', 'GET');
            return;
        }

        if (title === 'Delete contact by email') {
            event.preventDefault();
            event.stopPropagation();
            void executeGdprAction('deleteuser', 'DELETE');
        }
    }, true);
}

function initGdprCopyAction(): void {
    clearGdprFormData();
    initGdprActionHandlers();
}

if (typeof document !== 'undefined') {
    const cssHref = '/Admin/adminresources/bdo.custom.components/entry.css';
    if (!document.querySelector(`link[href="${cssHref}"]`)) {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = cssHref;
        document.head.appendChild(link);
    }

    initGdprCopyAction();
}

export {
    BDOSQLDropdown,
    BDOSQLDropdown as BDOSQLDropdownFormComponent,
    CustomActionButton,
    EmailLookupFormComponent,
    FieldGroupSeparator as FieldGroupSeparatorFormComponent,
};
