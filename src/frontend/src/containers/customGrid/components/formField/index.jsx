import React from 'react';
import Field from '../../../../components/BaseComponents';

const FormField = props => {
    const { form = {}, columnsConfig = {}, error = {}, settings = {}, name } = props;

    return (
        <Field
            {...columnsConfig[name]}
            {...props}
            key={name}
            value={form[name]}
            error={error[name]}
            settings={settings[name]}
        />
    );
};

export default React.memo(FormField);
