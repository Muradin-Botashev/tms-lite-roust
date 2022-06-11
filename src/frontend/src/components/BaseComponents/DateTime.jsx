import React from 'react';
import { useTranslation } from 'react-i18next';
import { Form } from 'semantic-ui-react';
import DatePicker from 'react-datepicker';
import setMinutes from 'date-fns/setMinutes'
import setHours from 'date-fns/setHours'

import { formatDate, parseDateTime } from '../../utils/dateTimeFormater';
import moment from "moment";

const DateTime = ({
    value,
    name,
    onChange,
    isDisabled,
    noLabel,
    popperPlacement = 'bottom-end',
    className,
    text,
    placeholder,
    isRequired,
    error,
    maxDate,
    minDate,
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

   const getMinTime = (minDate) => {
        if (moment(minDate, 'DD.MM.YYYY').isSame(moment(value, 'DD.MM.YYYY'))) {
            return parseDateTime(minDate);
        }

        return setHours(setMinutes(new Date(), 0), 0);
    };

    const getMaxTime = (maxDate) => {
        if (moment(maxDate, 'DD.MM.YYYY').isSame(moment(value, 'DD.MM.YYYY'))) {
            return parseDateTime(maxDate);
        }

        return setHours(setMinutes(new Date(), 45), 23);
    };

    return (
        <Form.Field className={noLabel ? 'no-label-datepicker' : undefined}>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span dangerouslySetInnerHTML={{ __html: `${t(text || name)}${isRequired ? ' *' : ''}` }} />
                </label>
            ) : null}
            <DatePicker
                placeholderText={placeholder}
                className={getClassNames()}
                locale={localStorage.getItem('i18nextLng')}
                disabled={isDisabled || false}
                isClearable={!(isDisabled || false)}
                selected={parseDateTime(value || '')}
                maxDate={parseDateTime(maxDate || '')}
                minDate={parseDateTime(minDate || '')}
                minTime={getMinTime(minDate)}
                maxTime={getMaxTime(maxDate)}
                dateFormat="dd.MM.yyyy HH:mm"
                showTimeSelect
                timeFormat="HH:mm"
                timeIntervals={15}
                timeCaption={t('Time')}
                onChange={(date, e) => {
                    onChange(e, {
                        name: name,
                        value: date ? formatDate(date, 'dd.MM.yyyy HH:mm') : null,
                    });
                }}
                popperPlacement={popperPlacement}
                onChangeRaw={e => onChange(e, { name, value: e.target.value })}
            />
            {error && typeof error === 'string' ? (
                <span className="label-error">{error}</span>
            ) : null}
        </Form.Field>
    );
};
export default DateTime;
