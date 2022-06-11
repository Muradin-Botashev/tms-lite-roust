import React from 'react';
import { Form, TextArea } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';

const BigText = ({
    value,
    name,
    onChange,
    isDisabled,
    noLabel,
    className,
    rows,
    text,
    isRequired,
    error,
    autoFocus,
}) => {
    const { t } = useTranslation();

    const getClassNames = () => {
        const classNames = [];

        if (error) {
            classNames.push('input-error');
        }

        if (className) {
            classNames.push(className);
        }

        return classNames.join(' ');
    };

    return (
        <Form.Field>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span dangerouslySetInnerHTML={{ __html: `${t(text || name)}${isRequired ? ' *' : ''}` }} />
                </label>
            ) : null}
            <TextArea
                className={getClassNames()}
                autoHeight
                disabled={isDisabled || false}
                name={name}
                rows={rows}
                value={value || ''}
                autoFocus={autoFocus}
                onChange={onChange}
            />
            {error && typeof error === 'string' ? (
                <span className="label-error">{error}</span>
            ) : null}
        </Form.Field>
    );
};
export default BigText;
