import React, { useState } from "react";
import { Button, InputBase } from "@kentico/xperience-admin-components";
import { useFormComponentCommandProvider, FormComponentProps } from "@kentico/xperience-admin-base";

type GdprCommand = 'FetchTextData' | 'FetchXmlData' | 'Delete';

interface GdprCommandResult {
    success: boolean;
    message: string;
}

const isValidEmail = (email: string) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim());

const commandLabels: Record<GdprCommand, string> = {
    FetchTextData: 'Search to Text',
    FetchXmlData: 'Search to XML',
    Delete: 'Delete',
};

export const GDPRControl = (props: FormComponentProps) => {
    const [emailAddress, setEmailAddress] = useState("");
    const [results, setResults] = useState("");
    const [fetchedEmail, setFetchedEmail] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [successMessage, setSuccessMessage] = useState("");
    const [copied, setCopied] = useState(false);

    const { executeCommand } = useFormComponentCommandProvider();

    const validateEmailInput = (): boolean => {
        if (!emailAddress.trim()) {
            setErrorMessage("Please enter an email address.");
            return false;
        }
        if (!isValidEmail(emailAddress)) {
            setErrorMessage("Please enter a valid email address.");
            return false;
        }
        return true;
    };

    const invokeCommand = async (commandName: GdprCommand) => {
        setErrorMessage("");
        setSuccessMessage("");

        const result = await executeCommand<GdprCommandResult, string>(
            props,
            commandName,
            emailAddress
        );

        if (!result) return;

        if (!result.success) {
            setErrorMessage(result.message);
            return;
        }

        if (commandName === "Delete") {
            setResults("");
            setFetchedEmail("");
            setSuccessMessage(result.message);
            setTimeout(() => setSuccessMessage(""), 3000);
        } else {
            setResults(result.message);
            setFetchedEmail(emailAddress.trim());
        }
    };

    const handleDelete = async () => {
        if (!validateEmailInput()) return;
        if (!window.confirm("Are you sure you want to delete the personal data for this email address? This action cannot be undone.")) return;
        await invokeCommand("Delete");
    };

    const handleSearch = async (commandName: GdprCommand) => {
        if (!validateEmailInput()) return;
        await invokeCommand(commandName);
    };

    const handleCopy = async () => {
        if (!results) return;

        try {
            await navigator.clipboard.writeText(results);
            setCopied(true);
            setTimeout(() => setCopied(false), 3000);
        } catch {
            setErrorMessage("Failed to copy to clipboard. Please copy the results manually.");
        }
    };

    return (
        <div className="font-display">
            <div className="mb-6">
                <label className="block mb-2 text-sm font-normal text-label leading-4">
                    Email Address
                </label>
                <InputBase
                    value={emailAddress}
                    onChange={(event) => {
                        setEmailAddress(event.target.value);
                        setFetchedEmail("");
                    }}
                />
            </div>

            <div className="flex flex-wrap items-center gap-2 mb-4">
                <Button label={commandLabels.FetchTextData} onClick={() => handleSearch("FetchTextData")} />
                <Button label={commandLabels.FetchXmlData} onClick={() => handleSearch("FetchXmlData")} />
                <Button label={commandLabels.Delete} onClick={handleDelete} disabled={!results || !fetchedEmail || fetchedEmail !== emailAddress.trim()} />
                <Button label="Copy" onClick={handleCopy} disabled={!results} />
                {copied && (
                    <span className="text-xs text-[#2e7d32] font-medium pl-1">
                        ✓ Copied to clipboard
                    </span>
                )}
            </div>

            {errorMessage && (
                <div className="text-error/75 text-xs leading-normal mt-1.5 pl-4 font-medium mb-4">
                    {errorMessage}
                </div>
            )}

            {successMessage && (
                <div className="text-[#2e7d32] text-xs leading-normal mt-1.5 pl-4 font-medium mb-4">
                    ✓ {successMessage}
                </div>
            )}

            <div className="mb-2">
                <label className="block mb-2 text-sm font-normal text-label leading-4">
                    Results
                </label>
                <textarea
                    value={results}
                    readOnly
                    rows={10}
                    className="w-full min-h-[250px] p-3 text-xs leading-relaxed text-body font-mono bg-[#F5F5F5] border border-[#D0D0D0] rounded resize-y outline-none"
                />
            </div>
        </div>
    );
};