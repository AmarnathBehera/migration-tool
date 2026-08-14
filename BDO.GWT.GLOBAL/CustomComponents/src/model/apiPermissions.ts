import { FormComponentProps } from "@kentico/xperience-admin-base";

export interface UserOption {
    id: number;
    displayName: string;
}

export interface MethodGroupOption {
    id: number;
    displayName: string;
    description: string;
}

export interface MatrixRow {
    userId: number;
    methodGroupId: number;
    userDisplayName: string;
    methodGroupName: string;
    allowRead: boolean;
    allowManage: boolean;
}

export interface InitResult {
    users: UserOption[];
    methodGroups: MethodGroupOption[];
}

export interface LoadMatrixResult {
    rows: MatrixRow[];
}

export interface ToggleResult {
    success: boolean;
    message: string;
}

export type FilterType = "users" | "methodgroup";

export interface ApiPermissionsProps extends FormComponentProps {
    webChannelSettingId?: number;
}
