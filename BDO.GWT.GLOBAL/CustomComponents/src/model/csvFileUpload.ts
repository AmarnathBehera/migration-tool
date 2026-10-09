export interface CsvFileUploadProps {
    value?: string;
    onChange?: (value: string) => void;
    label?: string;
    explanationText?: string;
    validationMessage?: string;
    invalid?: boolean;
    required?: boolean;
    disabled?: boolean;
}
