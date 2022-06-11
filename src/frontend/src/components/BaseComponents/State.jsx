import React, { useEffect } from 'react';
import { Dropdown, Form, Icon } from 'semantic-ui-react';
import { useDispatch, useSelector } from 'react-redux';
import { useTranslation } from 'react-i18next';
import { getLookupRequest, valuesListSelector } from '../../ducks/lookup';

const State = ({
    value,
    name,
    isDisabled,
    onChange,
    className,
    source,
    placeholder,
    isRequired,
    error,
}) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    let stateColors = useSelector(state => valuesListSelector(state, source)) || [];

    useEffect(() => {
        dispatch(
            getLookupRequest({
                name: source,
                isForm: true,
                isState: true,
            }),
        );
    }, []);

    const state = stateColors.find(x => x.name === value);
    const color = state ? state.color : 'grey';
    const items = (stateColors || []).map(x => {
        return {
            key: x.name,
            value: x.name,
            text: t(x.name),
            label: { color: x.color, empty: true, circular: true },
        };
    });

    if (!isDisabled)
        return (
            <Form.Field>
                <label className={isDisabled ? 'label-disabled' : null}>
                    <span dangerouslySetInnerHTML={{ __html: `${t(name)}${isRequired ? ' *' : ''}` }} />
                </label>
                <Dropdown
                    placeholder={placeholder}
                    className={className}
                    selection
                    search
                    error={error}
                    disabled={isDisabled || false}
                    name={name}
                    value={value}
                    fluid
                    options={items}
                    onChange={onChange}
                />
                {error && typeof error === 'string' ? (
                    <span className="label-error">{error}</span>
                ) : null}
            </Form.Field>
        );
    else
        return (
            <Form.Field>
                <label>{t(name)}</label>
                <div className="semantic-ui-div state-edit-field">
                    <Icon color={color} name="circle" />
                    {t(value)}
                </div>
            </Form.Field>
        );
};
export default State;
