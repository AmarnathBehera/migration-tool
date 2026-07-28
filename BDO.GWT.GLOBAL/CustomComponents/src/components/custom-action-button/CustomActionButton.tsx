import React from 'react';

type GdprCommand = 'FetchTextData' | 'FetchXmlData' | 'Delete';

interface CustomActionButtonProps {
    commandName?: GdprCommand;
    commandType?: GdprCommand;
    showSelector?: boolean;
}

export const CustomActionButton = ({
    commandName,
    commandType,
    showSelector = true
}: CustomActionButtonProps) => {
    const [isRunning, setIsRunning] = React.useState(false);
    const [isDeleting, setIsDeleting] = React.useState(false);

    const initialCommand: GdprCommand = commandName ?? commandType ?? 'FetchTextData';
    const [selectedCommand, setSelectedCommand] = React.useState(initialCommand);

    const commandUi: Record<GdprCommand, { label: string; title: string; className: string; loadingText: string }> = {
        FetchTextData: {
            label: 'Search to Text',
            title: 'Search contact as text',
            className: 'px-4 py-2 rounded-full text-sm font-medium text-white bg-[#2f55be] hover:bg-[#2649a8]',
            loadingText: 'Running...'
        },
        FetchXmlData: {
            label: 'Search to XML',
            title: 'Search contact as XML',
            className: 'px-4 py-2 rounded-full text-sm font-medium text-[#2f2f2f] bg-[#bcc4d1] hover:bg-[#aeb7c5]',
            loadingText: 'Running...'
        },
        Delete: {
            label: 'Delete User',
            title: 'Delete contact',
            className: 'px-4 py-2 rounded-full text-sm font-medium text-black bg-[#ebebeb] hover:bg-[#dfdfdf]',
            loadingText: 'Deleting...'
        }
    };

    const readEmailAddress = (): string => {
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
    };

    const getEndpointUrl = (command: GdprCommand, email: string): string => {
        const routeMap: Record<GdprCommand, string> = {
            FetchTextData: 'searchtotext',
            FetchXmlData: 'searchtoxml',
            Delete: 'deleteuser'
        };

        return `/admin-api/custom-action/${routeMap[command]}?email=${encodeURIComponent(email)}`;
    };

    const writeResultToTextarea = (result: string): void => {
        const textarea = document.querySelector(
            'textarea[name$="Results"], textarea[id*="Results"], textarea[aria-label="Results"]'
        ) as HTMLTextAreaElement | null;

        if (!textarea) {
            return;
        }

        textarea.value = result;
        textarea.dispatchEvent(new Event('input', { bubbles: true }));
        textarea.dispatchEvent(new Event('change', { bubbles: true }));
    };

    const runCommand = async (command: GdprCommand) => {
        const email = readEmailAddress();
        if (!email) {
            alert('Email address is required.');
            return;
        }

        const isDeleteCommand = command === 'Delete';

        if (isDeleteCommand) {
            setIsDeleting(true);
        } else {
            setIsRunning(true);
        }

        try {
            const response = await fetch(getEndpointUrl(command, email), {
                method: isDeleteCommand ? 'DELETE' : 'GET',
                credentials: 'include'
            });

            const payload = await response.json() as { result?: string };

            if (!response.ok) {
                writeResultToTextarea(payload.result || 'Action failed.');
                alert(payload.result || 'Action failed.');
                return;
            }

            writeResultToTextarea(payload.result || 'Action completed.');
            alert(payload.result || 'Action completed.');
        } catch {
            writeResultToTextarea(isDeleteCommand ? 'Delete action failed.' : 'Custom action failed.');
            alert(isDeleteCommand ? 'Delete action failed.' : 'Custom action failed.');
        } finally {
            if (isDeleteCommand) {
                setIsDeleting(false);
            } else {
                setIsRunning(false);
            }
        }
    };

    const isDeleteCommand = selectedCommand === 'Delete';
    const isBusy = isRunning || isDeleting;
    const ui = commandUi[selectedCommand];

    return (
        <div className="flex items-center gap-2">
            {showSelector && (
                <select
                    value={selectedCommand}
                    onChange={(event) => setSelectedCommand(event.target.value as GdprCommand)}
                    disabled={isBusy}
                    className="px-3 py-2 rounded-full text-sm border border-[#bcc4d1] bg-white text-[#2f2f2f]"
                    title="Select command"
                >
                    <option value="FetchTextData">Search to Text</option>
                    <option value="FetchXmlData">Search to XML</option>
                    <option value="Delete">Delete User</option>
                </select>
            )}

            <button
                type="button"
                onClick={() => runCommand(selectedCommand)}
                disabled={isBusy}
                className={ui.className}
                title={ui.title}
                data-command={selectedCommand}
            >
                {isBusy && isDeleteCommand ? ui.loadingText : isBusy ? 'Running...' : ui.label}
            </button>
        </div>
    );
};
