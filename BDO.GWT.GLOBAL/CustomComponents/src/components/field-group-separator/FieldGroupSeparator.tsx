import React from 'react';
import type { FieldGroupSeparatorProps } from '@model/fieldGroupSeparator';

export const FieldGroupSeparator = (props: FieldGroupSeparatorProps = {}) => {
    const label = (props && props.label) ? String(props.label) : '';
    const explanationText = (props && props.explanationText) ? String(props.explanationText) : '';

    return (
        <div className="mt-2">
            {label && (
                <div className={`text-base font-bold text-heading ${explanationText.length > 0 ? 'mb-2' : 'mb-3'}`}>
                    {label}
                </div>
            )}
            {explanationText.length > 0 && (
                <div className="text-xs text-label mb-3.5 leading-normal">
                    {explanationText}
                </div>
            )}
            <div
                aria-hidden="true"
                className="h-[1.2px] w-full bg-[#676767]"
            />
        </div>
    );
};
