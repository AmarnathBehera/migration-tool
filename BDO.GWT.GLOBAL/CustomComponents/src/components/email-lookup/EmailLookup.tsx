import React, { useEffect, useMemo, useState } from 'react';
import { useFormComponentCommandProvider } from '@kentico/xperience-admin-base';
import type { EmailLookupCommandArguments, EmailLookupCommandResult, EmailLookupProps } from '@model/emailLookup';

const isValidEmail = (value: string) => /.+@.+\..+/.test(value.trim());

export const EmailLookupFormComponent = (props: EmailLookupProps) => {
    const isReadOnly = props.disabled === true;
    const [emailAddress, setEmailAddress] = useState(props.email ?? '');
    const [isBusy, setIsBusy] = useState(false);
    const [resultText, setResultText] = useState(props.value ?? '');
    const { executeCommand } = useFormComponentCommandProvider();

    const canSearch = useMemo(() => !isReadOnly && isValidEmail(emailAddress), [isReadOnly, emailAddress]);
    const canDelete = useMemo(() => !isReadOnly && isValidEmail(emailAddress) && !isBusy, [isReadOnly, emailAddress, isBusy]);

    useEffect(() => {
        setResultText(props.value ?? '');
    }, [props.value]);

    const updateOutput = (nextValue: string) => {
        setResultText(nextValue);

        if (!isReadOnly && props.onChange) {
            props.onChange(nextValue);
        }
    };

    const executeLookupAction = async (commandName: string | undefined) => {
        const trimmedEmail = emailAddress.trim();
        if (!commandName || !isValidEmail(trimmedEmail) || isReadOnly || isBusy) {
            return;
        }

        setIsBusy(true);

        try {
            const response = await executeCommand<EmailLookupCommandResult, EmailLookupCommandArguments>(
                props,
                commandName,
                {
                    email: trimmedEmail,
                    results: resultText,
                }
            );

            updateOutput(response?.results ?? '');
        } catch {
            updateOutput('Unable to execute GDPR action.');
        } finally {
            setIsBusy(false);
        }
    };

    const onSearchToText = async () => executeLookupAction(props.linkText);

    const onSearchToXml = async () => executeLookupAction(props.linkXml);

    const onDelete = async () => executeLookupAction(props.linkDelete);

    const onCopy = async () => {
        if (!props.linkCopy || !resultText) {
            return;
        }

        const response = await executeCommand<EmailLookupCommandResult, EmailLookupCommandArguments>(
            props,
            props.linkCopy,
            {
                email: emailAddress.trim(),
                results: resultText,
            }
        );

        const textToCopy = response?.results ?? resultText;
        updateOutput(textToCopy);

        if (typeof navigator !== 'undefined' && navigator.clipboard?.writeText) {
            await navigator.clipboard.writeText(textToCopy);
        }
    };

    return (
        <div className="font-display w-full">
            <div className="flex flex-col gap-2.5">
                <div className="flex flex-col gap-3 xl:flex-row xl:items-center xl:justify-between">
                    <div className="flex items-center gap-2.5">
                        <label className="text-sm font-semibold leading-5 text-heading min-w-[90px]">Email Address:</label>
                        <input
                            type="email"
                            value={emailAddress}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setEmailAddress(e.target.value)}
                            disabled={isReadOnly}
                            className={`w-full max-w-[340px] rounded-full border px-4 py-2 text-sm leading-5 outline-none ${isReadOnly ? 'bg-[#F5F5F5] border-[#D0D0D0]' : 'bg-white border-[#9CA3AF] focus:border-[#3553BD]'}`}
                        />
                    </div>

                    <div className="flex items-center justify-start gap-3 xl:justify-end">
                        <button
                            type="button"
                            onClick={onSearchToText}
                            disabled={!canSearch || isBusy}
                            className={`h-11 min-w-[170px] rounded-full px-6 text-sm font-semibold leading-5 ${canSearch && !isBusy ? 'bg-[#3553BD] text-white' : 'bg-[#A7B4E4] text-white cursor-not-allowed'}`}
                        >
                            Search to Text
                        </button>
                        <button
                            type="button"
                            onClick={onSearchToXml}
                            disabled={!canSearch || isBusy}
                            className={`h-11 min-w-[170px] rounded-full px-6 text-sm font-semibold leading-5 ${canSearch && !isBusy ? 'bg-[#C9CFDC] text-heading' : 'bg-[#E5E7EB] text-[#6B7280] cursor-not-allowed'}`}
                        >
                            Search to XML
                        </button>
                    </div>
                </div>

                <div className="text-sm font-semibold leading-5 text-heading">Results:</div>

                <textarea
                    value={resultText}
                    onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => updateOutput(e.target.value)}
                    disabled={isReadOnly}
                    rows={20}
                    className={`w-full min-h-[585px] resize-none rounded-sm border px-3 py-2 text-sm leading-5 outline-none ${props.invalid ? 'border-error' : 'border-[#D1D5DB]'} ${isReadOnly ? 'bg-[#F3F4F6]' : 'bg-[#DDE1E7]'}`}
                />

                <div className="flex gap-3 pt-1">
                    <button
                        type="button"
                        onClick={onDelete}
                        disabled={!canDelete}
                        className={`h-9 min-w-[95px] rounded-full px-5 text-sm font-semibold leading-5 ${canDelete ? 'bg-[#F3F4F6] text-black' : 'bg-[#E5E7EB] text-[#9CA3AF] cursor-not-allowed'}`}
                    >
                        Delete
                    </button>
                    <button
                        type="button"
                        onClick={onCopy}
                            disabled={!props.linkCopy || !resultText}
                            className={`h-9 min-w-[95px] rounded-full px-5 text-sm font-semibold leading-5 ${props.linkCopy && resultText ? 'bg-[#CDD4DF] text-heading' : 'bg-[#E5E7EB] text-[#9CA3AF] cursor-not-allowed'}`}
                    >
                        Copy
                    </button>
                </div>

                {props.explanationText && (
                    <div className="text-xs leading-normal text-muted">{props.explanationText}</div>
                )}

                {props.validationMessage && (
                    <div className="text-xs leading-normal text-error">{props.validationMessage}</div>
                )}
            </div>
        </div>
    );
};
