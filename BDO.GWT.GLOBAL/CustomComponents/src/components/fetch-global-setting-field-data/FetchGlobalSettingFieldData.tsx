import React, { useEffect, useRef } from 'react';
import type { FetchGlobalSettingFieldDataProps } from '@model/fetchGlobalSettingFieldData';

export const BDOFetchGlobalSettingFieldData = (props: FetchGlobalSettingFieldDataProps) => {
    const textAreaRef = useRef<HTMLTextAreaElement>(null);
    const displayLabel = props.fieldLabel || props.label;
    const displayValue = props.value ?? '';

    useEffect(() => {
        const textArea = textAreaRef.current;

        if (!textArea) {
            return;
        }

        textArea.style.height = 'auto';
        textArea.style.height = `${textArea.scrollHeight}px`;
    }, [displayValue]);

    return (
        <div className="mb-6 font-display">
            {displayLabel && (
                <label className="block mb-2 text-sm font-normal text-label leading-4">
                    {props.required && <span className="text-error mr-0.5">*</span>}
                    {displayLabel}
                </label>
            )}

            <textarea
                ref={textAreaRef}
                value={displayValue}
                readOnly
                aria-readonly="true"
                placeholder={props.placeholder}
                rows={3}
                className={`w-full min-h-[112px] resize-none overflow-hidden py-2.5 px-4 rounded-[20px] text-sm leading-normal outline-none border box-border transition-colors duration-200 text-label bg-[#F5F5F5] border-[#D0D0D0] cursor-default ${props.invalid ? 'border-error' : ''}`}
            />

            {props.explanationText && (
                <div className="text-muted text-xs leading-normal mt-1.5 pl-4">
                    {props.explanationText}
                </div>
            )}

            {props.validationMessage && (
                <div className="text-error text-xs leading-normal mt-1.5 pl-4 font-medium">
                    {props.validationMessage}
                </div>
            )}
        </div>
    );
};
