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
    required?: boolean;
}

const styles = {
    wrapper: {
        marginBottom: '24px',
        fontFamily: 'GT Walsheim", sans-serif',
    } as React.CSSProperties,
    label: {
        display: 'block',
        marginBottom: '8px',
        fontSize: '14px',
        fontWeight: 400,
        color: '#525252',
        lineHeight: '16px',
    } as React.CSSProperties,
    required: {
        color: '#cb2929',
        marginRight: '2px',
    } as React.CSSProperties,
    select: (invalid?: boolean) => ({
        width: '100%',
        padding: '10px 36px 10px 16px',
        border: '1px solid #525252',
        borderRadius: '50px',
        fontSize: '14px',
        lineHeight: '1.43',
        color: '#3e3e3c',
        backgroundColor: '#fff',
        cursor: 'pointer',
        outline: 'none',
        appearance: 'none' as const,
        WebkitAppearance: 'none' as const,
        MozAppearance: 'none' as const,
        backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='10' height='6' viewBox='0 0 10 6'%3E%3Cpath fill='%23707070' d='M0 0l5 6 5-6z'/%3E%3C/svg%3E")`,
        backgroundRepeat: 'no-repeat',
        backgroundPosition: 'right 16px center',
        backgroundSize: '10px 6px',
        transition: 'border-color 0.2s ease, box-shadow 0.2s ease',
        boxSizing: 'border-box' as const,
    }) as React.CSSProperties,
    explanationText: {
        color: '#707070',
        fontSize: '12px',
        lineHeight: '1.5',
        marginTop: '6px',
        paddingLeft: '16px',
    } as React.CSSProperties,
    validationMessage: {
        color: '#cb2929',
        fontSize: '12px',
        lineHeight: '1.5',
        marginTop: '6px',
        paddingLeft: '16px',
        fontWeight: 500,
    } as React.CSSProperties,
};

export const BDOSQLDropdown = (props: CustomDropdownProps) => {
    const [focused, setFocused] = React.useState(false);
    const displayLabel = props.fieldLabel || props.label;

    const handleChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        if (props.onChange) {
            props.onChange(e.target.value);
        }
    };

    return (
        <div style={styles.wrapper}>
            {displayLabel && (
                <label style={styles.label}>
                    {props.required && <span style={styles.required}>*</span>}
                    {displayLabel}
                </label>
            )}
            <select
                value={props.value ?? ''}
                onChange={handleChange}
                onFocus={() => setFocused(true)}
                onBlur={() => setFocused(false)}
                style={styles.select(props.invalid)}
            >
                <option value="">
                    {props.defaultValue ?? 'Choose an option'}
                </option>
                {props.options?.map((item, index) => (
                    <option key={index} value={item.text}>
                        {item.text}
                    </option>
                ))}
            </select>

            {props.explanationText && (
                <div style={styles.explanationText}>
                    {props.explanationText}
                </div>
            )}

            {props.validationMessage && (
                <div style={styles.validationMessage}>
                    {props.validationMessage}
                </div>
            )}
        </div>
    );
};

export { BDOSQLDropdown as BDOSQLDropdownFormComponent };