import React from 'react';
import { Form, Input } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';

const Number = ({
    value = '',
    name,
    onChange,
    isDisabled,
    noLabel,
    className,
    text,
    error,
    placeholder,
    isRequired,
    isInteger,
    autoFocus,
}) => {
    const { t } = useTranslation();

    const handleOnChange = (e, { value }) => {
        onChange(e, {
            name,
            value: value.replace(/[^\d]/g, ''),
        });
    };

    return (
        <Form.Field>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span dangerouslySetInnerHTML={{ __html: `${t(text || name)}${isRequired ? ' *' : ''}` }} />
                </label>
            ) : null}
            <Input
                placeholder={placeholder}
                className={className}
                type={isInteger ? 'text' : 'number'}
                error={error}
                disabled={isDisabled || false}
                name={name}
                value={value}
                step="any"
                autoFocus={autoFocus}
                onChange={isInteger ? handleOnChange : onChange}
                autoComplete="off"
            />
            {error && typeof error === 'string' ? (
                <span className="label-error">{error}</span>
            ) : null}
        </Form.Field>
    );
};
export default Number;
