import React from 'react';
import { Dropdown } from 'semantic-ui-react';
import {
    SETTINGS_TYPE_EDIT,
    SETTINGS_TYPE_HIDE,
    SETTINGS_TYPE_SHOW,
} from '../../../constants/formTypes';

const SettingCell = ({ value, t, onChange, loading, isDisabled, status, fieldName }) => {
    const availabilityListOptions = [
        /* { key: SETTINGS_TYPE_HIDE, value: SETTINGS_TYPE_HIDE, text: t(SETTINGS_TYPE_HIDE) },*/
        { key: SETTINGS_TYPE_SHOW, value: SETTINGS_TYPE_SHOW, text: t(SETTINGS_TYPE_SHOW) },
        { key: SETTINGS_TYPE_EDIT, value: SETTINGS_TYPE_EDIT, text: t(SETTINGS_TYPE_EDIT) },
    ];

    return (
        <Dropdown
            options={availabilityListOptions}
            fluid
            disabled={isDisabled}
            value={value}
            loading={loading}
            status={status}
            fieldName={fieldName}
            onChange={onChange}
        />
    );
};

export default React.memo(SettingCell);
