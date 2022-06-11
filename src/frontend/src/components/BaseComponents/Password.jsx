import React, { useState } from 'react';
import { Form, Icon, Input } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';

const PasswordField = ({
    value = '',
    onChange,
    noLabel,
    isDisabled,
    isRequired,
    text,
    name,
    onBlur,
    error,
}) => {
    let [show, setShow] = useState(false);
    const { t } = useTranslation();

    const toggle = () => {
        setShow(prevState => !prevState);
    };

    return (
        <Form.Field>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span dangerouslySetInnerHTML={{ __html: `${t(text || name)}${isRequired ? ' *' : ''}` }} />
                </label>
            ) : null}
            <Input
                value={value}
                fluid
                name={name}
                error={error}
                type={show ? 'text' : 'password'}
                icon={<Icon name={show ? 'eye slash' : 'eye'} link onClick={toggle} />}
                autoComplete="new-password"
                onChange={onChange}
                onBlur={onBlur}
            />
            {error && typeof error === 'string' ? (
                <span className="label-error">{error}</span>
            ) : null}
        </Form.Field>
    );
};

export default PasswordField;
