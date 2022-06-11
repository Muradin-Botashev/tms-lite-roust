import React, { useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';

import { Dropdown, Form } from 'semantic-ui-react';

import './style.scss';
import {getLookupRequest, listFromMultiSelect, listFromSelectSelector, valuesListSelector} from '../../ducks/lookup';

const MultiSelect = ({
    value,
    onChange,
    placeholder = '',
    isDisabled,
    label,
    name,
    text,
    multiple,
    loading,
    clearable,
    source,
    isTranslate,
    error,
    textValue,
    noLabel,
    isRequired,
    autoComplete,
    children,
    sourceParams,
}) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();

    useEffect(() => {
        dispatch(
            getLookupRequest({
                name: source,
                isForm: true,
                sourceParams,
            }),
        );
    }, []);

    const valuesList = useSelector(state =>
        listFromMultiSelect(state, source, isTranslate, t),
    );

    const handleChange = (e, { value }) => {
        onChange(e, { value: value ? valuesList.filter(item => value.includes(item.value)) : null, name });
    };

    /*let items =
        valuesList &&
        valuesList.map((x, index) => ({
            key: `${x.value}_${index}`,
            value: x.value,
            text: isTranslate ? t(x.name) : x.name,
        }));*/

    return (
        <Form.Field>
            {!noLabel ? (
                <label className={isDisabled ? 'label-disabled' : null}>{`${t(text || name)}${
                    isRequired ? ' *' : ''
                }`}</label>
            ) : null}
            <div className="form-select">
                <Dropdown
                    placeholder={placeholder}
                    fluid
                    clearable={clearable}
                    selection
                    loading={loading}
                    search
                    text={textValue}
                    error={error}
                    multiple
                    disabled={isDisabled}
                    value={value ? value.map(item => item.value) : null}
                    options={valuesList}
                    onChange={handleChange}
                    selectOnBlur={false}
                    autoComplete={autoComplete}
                />
                {children && children}
            </div>
            {error && typeof error === 'string' && <span className="label-error">{error}</span>}
        </Form.Field>
    );
};

export default React.memo(MultiSelect);
