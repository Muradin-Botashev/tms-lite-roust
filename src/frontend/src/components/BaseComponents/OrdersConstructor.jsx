import React, { useState, useMemo } from 'react';
import nanoid from 'nanoid';
import { Button, Form, Grid, Icon } from 'semantic-ui-react';
import FormField from '../../containers/customGrid/components/formField';
import { clearError } from '../../ducks/gridCard';
import { useDispatch } from 'react-redux';

const OrdersConstructor = ({
    item,
    index,
    onChange,
    onAdd,
    onDelete,
    isAdd,
    t,
    columnsConfig,
    error: errorProps,
    settings,
    defaultOrdersItem,
}) => {
    const [id] = useState(nanoid);
    const dispatch = useDispatch();

    const handleChange = (e, { name, value }) => {
        onChange(e, { name, value, index });
        if (errorProps[`${name}_${index}`]) {
            dispatch(clearError && clearError(`${name}_${index}`));
        }
    };


    const error = useMemo(() => {
        let errors = {};

        Object.keys(defaultOrdersItem).forEach(key => {
            if (errorProps[`${key}_${index}`]) {
                errors = {
                    ...errors,
                    [key]: errorProps[`${key}_${index}`],
                };
            }
        });
        return errors;
    }, [errorProps, index]);

    return (
        <Grid.Row key={id}>
            <Grid.Column width={4}>
                <div className="temperature-fields">
                    <label>{t('from')}</label>
                    <FormField
                        noLabel
                        name="palletsFrom"
                        columnsConfig={columnsConfig}
                        settings={settings}
                        form={item}
                        error={error}
                        onChange={handleChange}
                    />
                    <label>{t('to')}</label>
                    <FormField
                        noLabel
                        name="palletsTo"
                        columnsConfig={columnsConfig}
                        settings={settings}
                        form={item}
                        error={error}
                        onChange={handleChange}
                    />
                </div>
            </Grid.Column>
            <Grid.Column width={2}>
                <FormField
                    noLabel
                    name="clientOrderNumber"
                    columnsConfig={columnsConfig}
                    settings={settings}
                    form={item}
                    error={error}
                    onChange={handleChange}
                />
            </Grid.Column>
            <Grid.Column width={2}>
                <FormField
                    noLabel
                    name="orderNumber"
                    columnsConfig={columnsConfig}
                    settings={settings}
                    form={item}
                    error={error}
                    onChange={handleChange}
                />
            </Grid.Column>
            <Grid.Column width={2}>
                <FormField
                    noLabel
                    name="orderType"
                    columnsConfig={columnsConfig}
                    settings={settings}
                    form={item}
                    error={error}
                    onChange={handleChange}
                />
            </Grid.Column>
            <Grid.Column width={2}>
                <FormField
                    noLabel
                    name="weightKg"
                    columnsConfig={columnsConfig}
                    settings={settings}
                    form={item}
                    error={error}
                    onChange={handleChange}
                />
            </Grid.Column>
            <Grid.Column width={2}>
                <FormField
                    noLabel
                    name="orderAmountExcludingVAT"
                    columnsConfig={columnsConfig}
                    settings={settings}
                    form={item}
                    error={error}
                    onChange={handleChange}
                />
            </Grid.Column>
            <Grid.Column width={2}>
                {isAdd ? (
                    <Button icon onClick={onAdd}>
                        <Icon name="add" />
                    </Button>
                ) : null}
                <Button icon onClick={() => onDelete(index)}>
                    <Icon name="trash alternate" />
                </Button>
            </Grid.Column>
        </Grid.Row>
    );
};

export default OrdersConstructor;
