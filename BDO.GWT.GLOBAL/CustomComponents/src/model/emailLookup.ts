export interface EmailLookupCommandArguments {
    email: string;
    results: string;
}

export interface EmailLookupCommandResult {
    results?: string;
}

export interface EmailLookupProps {
    email?: string;
    linkText?: string;
    linkXml?: string;
    linkDelete?: string;
    linkCopy?: string;
    results?: string;
    value?: string;
    onChange?: (value: string) => void;
    label?: string;
    explanationText?: string;
    validationMessage?: string;
    invalid?: boolean;
    required?: boolean;
    disabled?: boolean;
}
