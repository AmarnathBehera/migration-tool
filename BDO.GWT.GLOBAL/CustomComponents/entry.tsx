import '@styles/index.css';
import { BDOSQLDropdown } from '@components/custom-dropdown/CustomDropdown';
import { CustomActionButton } from '@components/custom-action-button/CustomActionButton';
import { FieldGroupSeparator } from '@components/field-group-separator/FieldGroupSeparator';

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
    CustomActionButton,
    FieldGroupSeparator as FieldGroupSeparatorFormComponent,
};
