import React from 'react';
import type { CustomDropdownProps } from '@model/customDropdown';

export const BDOSQLDropdown = (props: CustomDropdownProps) => {
    const displayLabel = props.fieldLabel || props.label;

    return (
        <div className="mb-6 font-display">
            {displayLabel && (
                <label className="block mb-2 text-sm font-normal text-label leading-4">
                    {props.required && <span className="text-error mr-0.5">*</span>}
                    {displayLabel}
                </label>
            )}
            <select
                value={props.value ?? ''}
                onChange={e => props.onChange && props.onChange(e.target.value)}
                className={`w-full py-2.5 pl-4 pr-9 rounded-full text-sm leading-snug bg-white cursor-pointer outline-none appearance-none border box-border transition-colors duration-200 text-[#151515] select-chevron ${props.invalid ? 'border-error' : 'border-[#8C8C8C] hover:border-black'}`}
            >
                <option value="">{props.defaultValue ?? 'Choose an option'}</option>
                {props.options?.map((item, index) => (
                    <option key={index} value={item.text}>
                        {item.text}
                    </option>
                ))}
            </select>

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
