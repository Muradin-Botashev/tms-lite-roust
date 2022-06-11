import React, { useState, forwardRef } from 'react';
import { Button, Form, Icon, Input, Popup } from 'semantic-ui-react';
import DatePicker from 'react-datepicker';
import { dateToString, parseDate } from '../../utils/dateTimeFormater';
import { useTranslation } from 'react-i18next';

const Facet = forwardRef(({ name, text, value, onChange, sort, setSort }, ref) => {
    const { t } = useTranslation();

    const getStringItem = i => {
        const parts = (value || '').split('-');
        return parts[i] || null;
    };

    const getDateItem = i => {
        let s = getStringItem(i);
        if (s) return parseDate(s);
        return null;
    };

    let [isPeriod, setIsPeriod] = useState(Boolean(getStringItem(1)));

    const callbackOnChange = (start, end) => {
        let value = '';
        if (start && !end) {
            value = start;
        } else if (start && end) {
            value = [start, end].join('-');
        }

        if (onChange !== undefined) onChange(new Event('change'), { name, value });
    };

    const toggleStart = value => {
        let start = dateToString(value);
        if (start === getStringItem(0)) start = null;
        callbackOnChange(start, getStringItem(1));
    };

    const toggleEnd = value => {
        let end = dateToString(value);
        if (end === getStringItem(1)) end = null;
        callbackOnChange(getStringItem(0), end);
    };

    const handlePeriod = () => {
        setIsPeriod(true);
        ref && ref.current && ref.current.positionUpdate();
    };

    const handleRestClick = () => {
        callbackOnChange(null, null);
        setIsPeriod(false);
        ref && ref.current && ref.current.positionUpdate();
    };

    return (
        <div className="facet-input">
            <div className="reset-selected">
                <span onClick={handleRestClick}>{t('reset_selected')}</span>
            </div>
            <Form className="filter-popup">
                {/* <div>{t(name)}</div>*/}
                <Form.Group>
                    <Form.Field width={8}>
                        <DatePicker
                            inline
                            locale="ru"
                            selected={getDateItem(0) || null}
                            dateFormat="dd.MM.yyyy"
                            allowSameDay
                            onChange={toggleStart}
                        />
                    </Form.Field>
                    {isPeriod ? (
                        <Form.Field width={8}>
                            <DatePicker
                                inline
                                locale="ru"
                                disabled={!getDateItem(0)}
                                selected={getDateItem(1) || null}
                                dateFormat="dd.MM.yyyy"
                                minDate={getDateItem(0)}
                                allowSameDay
                                onChange={toggleEnd}
                            />
                        </Form.Field>
                    ) : null}
                </Form.Group>
            </Form>
            <Button size="mini" compact disabled={isPeriod || !getDateItem(0)} onClick={handlePeriod}>
                {t('Choose period')}
            </Button>
           {/* <div
                className={`facet-input_period ${
                    isPeriod || !getDateItem(0) ? 'facet-input_period__disabled' : ''
                }`}
                onClick={handlePeriod}
            >
                Выбрать период
            </div>*/}
        </div>
    );
});

export default Facet;
