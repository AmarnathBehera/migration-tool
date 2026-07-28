declare module '@kentico/xperience-admin-base' {
    export interface FormComponentProps<TValue> {
        value?: TValue;
        onChange?: (value?: TValue) => void;
        label?: string;
        explanationText?: string;
        validationMessage?: string;
        invalid?: boolean;
        required?: boolean;
        disabled?: boolean;
        name?: string;
        tooltip?: string;
        disabledMessage?: string;
        editMode?: unknown;
    }

    export function useFormComponentCommandProvider(): {
        executeCommand<TResponseResult = unknown, TCommandParameters = unknown>(
            props: unknown,
            commandName: string,
            commandHandlerArguments?: TCommandParameters
        ): Promise<TResponseResult>;
    };
}