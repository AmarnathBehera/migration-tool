import React, { useRef } from 'react';
import type { CsvFileUploadProps } from '@model/csvFileUpload';

export const CsvFileUpload = (props: CsvFileUploadProps = {}) => {
    const inputRef = useRef<HTMLInputElement>(null);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (event) => {
            const text = event.target?.result as string;
            if (props?.onChange) {
                props.onChange(text);
            }
        };
        reader.readAsText(file);
    };

    const isReadOnly = props?.disabled === true;

    return (
        <div className="mb-6 font-display">
            {props?.label && (
                <label className="block mb-2 text-sm font-normal text-label leading-4">
                    {props?.required && <span className="text-error mr-0.5">*</span>}
                    {props.label}
                </label>
            )}

            <div
                className={`flex items-center gap-3 px-4 py-2.5 rounded-full border box-border transition-colors duration-200 ${isReadOnly ? 'bg-[#F5F5F5] border-[#D0D0D0]' : props?.invalid ? 'border-error bg-white' : 'border-[#8C8C8C] bg-white hover:border-black'}`}
            >
                <button
                    type="button"
                    disabled={isReadOnly}
                    onClick={() => inputRef.current?.click()}
                    className={`text-sm px-3 py-1 rounded-full border transition-colors duration-150 ${isReadOnly ? 'text-[#8C8C8C] border-[#D0D0D0] cursor-default' : 'text-label border-[#8C8C8C] hover:border-black cursor-pointer'}`}
                >
                    Choose file
                </button>
                <span className="text-sm text-label truncate">
                    {props?.value ? '✓ File loaded' : 'No file chosen'}
                </span>
                <input
                    ref={inputRef}
                    type="file"
                    accept=".csv,text/csv"
                    disabled={isReadOnly}
                    className="hidden"
                    onChange={handleFileChange}
                />
            </div>

            {props?.explanationText && (
                <div className="text-muted text-xs leading-normal mt-1.5 pl-4">
                    {props.explanationText}
                </div>
            )}

            {props?.validationMessage && (
                <div className="text-error text-xs leading-normal mt-1.5 pl-4 font-medium">
                    {props.validationMessage}
                </div>
            )}
        </div>
    );
};
