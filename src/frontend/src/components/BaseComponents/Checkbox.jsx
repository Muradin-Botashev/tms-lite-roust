import React from 'react';
import { Checkbox } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';

const CheckBox = ({ checked, name, onChange, isDisabled, label }) => {
    const { t } = useTranslation();

    return (
        <Checkbox
            checked={checked}
            label={label || t(name)}
            name={name}
            disabled={isDisabled}
            onClick={(event, { name, checked }) => onChange(event, { name, value: checked })}
        />
    );
};

export default CheckBox;
