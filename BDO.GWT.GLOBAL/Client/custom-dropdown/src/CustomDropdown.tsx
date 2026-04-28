import React from 'react';

interface DropDownOption {
    value: string;
    text: string;
}

interface CustomDropdownProps {
    value?: string;
    onChange?: (value: string) => void;
    options: DropDownOption[];
    placeholder?: string;
    defaultValue?: string;
    label?: string;
    fieldLabel?: string;
    explanationText?: string;
    validationMessage?: string;
    invalid?: boolean;
}

export const BDOSQLDropdown = (props: CustomDropdownProps) => {

    const displayLabel = props.fieldLabel || props.label;

    const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        if (props.onChange) {
            props.onChange(e.target.value);
        }
    };

    return (
        <div style={{ marginBottom: '16px' }}>
            {displayLabel && (
                <label style={{
                    display: 'block',
                    marginBottom: '6px',
                    fontSize: '14px',
                    fontWeight: 600,
                    color: '#222'
                }}>
                    {displayLabel}
                </label>
            )}
            <select
                value={props.value ?? ''}
                onChange={handleChange}
                style={{
                    width: '100%',
                    padding: '8px 12px',
                    border: props.invalid ? '1px solid red' : '1px solid #ccc',
                    borderRadius: '4px',
                    fontSize: '14px',
                    backgroundColor: '#fff',
                    cursor: 'pointer'
                }}
            >
                <option value="">
                    {props.defaultValue ?? '-- Select --'}
                </option>
                {props.options?.map((item, index) => (
                    <option key={index} value={item.text}>
                        {item.text}
                    </option>
                ))}
            </select>

            {props.explanationText && (
                <div style={{ color: '#666', fontSize: '12px', marginTop: '4px' }}>
                    {props.explanationText}
                </div>
            )}

            {props.validationMessage && (
                <div style={{ color: 'red', fontSize: '12px', marginTop: '4px' }}>
                    {props.validationMessage}
                </div>
            )}
        </div>
    );
};

export { BDOSQLDropdown as BDOSQLDropdownFormComponent };