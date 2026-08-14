export type GdprCommand = 'FetchTextData' | 'FetchXmlData' | 'Delete';

export interface GdprCommandResult {
    success: boolean;
    message: string;
}
