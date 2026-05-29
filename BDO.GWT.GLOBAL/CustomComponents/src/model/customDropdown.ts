export interface DropDownOption {
    value: string;
    text: string;
}

export interface CustomDropdownProps {
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
