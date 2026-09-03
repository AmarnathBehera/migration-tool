import '@styles/index.css';
import { BDOSQLDropdown } from '@components/custom-dropdown/CustomDropdown';
import { GDPRControl } from '~/components/gdpr-control/GDPRControl';
import { FieldGroupSeparator } from '@components/field-group-separator/FieldGroupSeparator';
import { CsvFileUpload } from '@components/csv-file-upload/CsvFileUpload';
import { ApiPermissionsControl } from '@components/api-permissions/ApiPermissionsControl';
import { BDOFetchGlobalSettingFieldData } from '@components/fetch-global-setting-field-data/FetchGlobalSettingFieldData';

if (typeof document !== 'undefined') {
    const cssHref = '/Admin/adminresources/bdo.custom.components/entry.css';
    if (!document.querySelector(`link[href="${cssHref}"]`)) {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = cssHref;
        document.head.appendChild(link);
    }
}

export {
    BDOSQLDropdown,
    BDOSQLDropdown as BDOSQLDropdownFormComponent,
    GDPRControl,
    GDPRControl as GDPRControlFormComponent,
    FieldGroupSeparator as FieldGroupSeparatorFormComponent,
    CsvFileUpload,
    CsvFileUpload as CsvFileUploadFormComponent,
    ApiPermissionsControl,
    ApiPermissionsControl as ApiPermissionsControlFormComponent,
    BDOFetchGlobalSettingFieldData,
    BDOFetchGlobalSettingFieldData as BDOFetchGlobalSettingFieldDataFormComponent,
};
export { EditQueryTemplate as EditQuery, EditQueryTemplate } from '@components/custom-reporting/EditQuery';
export { EditQueryTemplate as default } from '@components/custom-reporting/EditQuery';
export * from '@components/custom-reporting/EditQueryTemplate';
