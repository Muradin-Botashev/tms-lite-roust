import React from 'react';
import { Form, Radio } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';

const Bool = ({
    value,
    name,
    onChange,
    isDisabled,
    noLabel,
    className,
    text,
    error,
    emptyValue,
}) => {
    const { t } = useTranslation();


    return (
        <Form.Field>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span dangerouslySetInnerHTML={{ __html: t(text || name) }} />
                </label>
            ) : null}
            <div className="bool-radio-button">
                <Radio
                    label={t('Yes')}
                    name={name}
                    value={true}
                    checked={value === true}
                    className={className}
                    disabled={isDisabled}
                    onChange={onChange}
                />
                <Radio
                    label={t('No')}
                    name={name}
                    value={false}
                    checked={value === false}
                    className={className}
                    disabled={isDisabled}
                    onChange={onChange}
                />
                {emptyValue === 'allowed' && (
                    <Radio
                        label={t('not_chosen')}
                        name={name}
                        value={null}
                        checked={value !== false && value !== true}
                        className={className}
                        disabled={isDisabled}
                        onChange={onChange}
                    />
                )}
            </div>
        </Form.Field>
    );
};
export default Bool;
