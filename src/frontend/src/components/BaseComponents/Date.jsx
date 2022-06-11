import React from 'react';
import { Form } from 'semantic-ui-react';
import DatePicker from 'react-datepicker';
import { formatDate, parseDate } from '../../utils/dateTimeFormater';
import { useTranslation } from 'react-i18next';

const Date = ({
    value,
    name,
    onChange,
    isDisabled,
    noLabel,
    text,
    placeholder,
    className,
    isRequired,
    error,
    maxDate,
    minDate,
}) => {
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

    const { t } = useTranslation();

    return (
        <Form.Field>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span dangerouslySetInnerHTML={{ __html: `${t(text || name)}${isRequired ? ' *' : ''}` }} />
                </label>
            ) : null}
            <DatePicker
                disabled={isDisabled || false}
                placeholderText={placeholder}
                className={getClassNames()}
                isClearable={!(isDisabled || false)}
                selected={parseDate((value || '').substring(0, 10))}
                locale={localStorage.getItem('i18nextLng')}
                dateFormat="dd.MM.yyyy"
                maxDate={parseDate((maxDate || '').substring(0, 10))}
                minDate={parseDate((minDate || '').substring(0, 10))}
                onChange={(date, e) => {
                    onChange(e, { name: name, value: date ? formatDate(date) : null });
                }}
            />
            {error && typeof error === 'string' ? (
                <span className="label-error">{error}</span>
            ) : null}
        </Form.Field>
    );
};
export default Date;
