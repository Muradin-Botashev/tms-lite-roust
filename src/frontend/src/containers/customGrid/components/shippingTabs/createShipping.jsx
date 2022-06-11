import React, { useEffect, useCallback, useMemo } from 'react';
import { Form, Grid } from 'semantic-ui-react';
import FormField from '../formField';
import { useDispatch, useSelector } from 'react-redux';
import { columnsTypesConfigSelector } from '../../../../ducks/representations';
import { SHIPPINGS_GRID } from '../../../../constants/grids';
import OrdersConstructor from '../../../../components/BaseComponents/OrdersConstructor';
import ReturnCostsConstructor from '../../../../components/BaseComponents/ReturnCostsConstructor';
import { useTranslation } from 'react-i18next';
import {
    clearError,
    createConfigSelector,
    createDefaultValueSelector,
    getCardConfigRequest,
} from '../../../../ducks/gridCard';
import Field from '../../../../components/BaseComponents';
import { CONSTRUCTOR_TYPE } from '../../../../constants/columnTypes';

const CreateShipping = ({ form = {}, onChange, settings, error }) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    const columnsConfig = useSelector(createConfigSelector);

    const defaultValues = useSelector(createDefaultValueSelector);
    const defaultOrdersItem = useMemo(
        () => ({
            palletsFrom: '',
            palletsTo: '',
            clientOrderNumber: '',
            orderNumber: '',
            orderType: null,
            weightKg: '',
            orderAmountExcludingVAT: '',
        }),
        [],
    );

    useEffect(() => {
        dispatch(getCardConfigRequest({ name: SHIPPINGS_GRID }));
    }, []);

    useEffect(() => {
        defaultValues &&
            Object.keys(defaultValues).forEach(key =>
                onChange(null, { name: key, value: defaultValues[key] }),
            );
    }, [defaultValues]);

    const handleChangeShippingWarehouseId = useCallback((e, { name, value }) => {
        onChange(e, {
            name,
            value,
        });
        onChange(e, { name: 'shippingAddress', value: value ? value.address : null });
    }, []);

    const handleChangeDeliveryWarehouseId = useCallback((e, { name, value }) => {
        onChange(e, {
            name,
            value,
        });
        onChange(e, { name: 'deliveryAddress', value: value ? value.address : null });
    }, []);

    return (
        <>
            <Form className="tabs-card">
                <Grid>
                    <Grid.Row columns={3}>
                        <Grid.Column>
                            <FormField
                                name="tarifficationType"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="shippingDate"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="deliveryDate"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={2}>
                        <Grid.Column>
                            <FormField
                                name="shippingWarehouseId"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={handleChangeShippingWarehouseId}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="deliveryWarehouseId"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={handleChangeDeliveryWarehouseId}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={2}>
                        <Grid.Column>
                            <FormField
                                name="shippingAddress"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="deliveryAddress"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={3}>
                        <Grid.Column>
                            <FormField
                                name="palletsCount"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="totalWeightKg"
                                columnsConfig={columnsConfig}
                                isRequired={form.distributeDataByOrders}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="totalOrderAmountExcludingVAT"
                                columnsConfig={columnsConfig}
                                isRequired={form.distributeDataByOrders}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={3}>
                        <Grid.Column>
                            <FormField
                                name="carrierId"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="bodyTypeId"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="poolingProductType"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={3}>
                        <Grid.Column>
                            <FormField
                                name="RouteNumber"
                                text="routeNumber"
                                columnsConfig={columnsConfig}
                                isRequired
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="BottlesCount"
                                text="bottlesCount"
                                columnsConfig={columnsConfig}
                                isRequired
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        <Grid.Column>
                            <FormField
                                name="Volume9l"
                                text="volume9l"
                                columnsConfig={columnsConfig}
                                isRequired
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                    </Grid.Row>
                    <Grid.Row columns={1}>
                        <Grid.Column>
                            <FormField
                                name="distributeDataByOrders"
                                columnsConfig={columnsConfig}
                                settings={settings}
                                form={form}
                                error={error}
                                onChange={onChange}
                            />
                        </Grid.Column>
                        {/* <Grid.Column>
                        <FormField
                            name="fillOrdersData"
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                    </Grid.Column>*/}
                    </Grid.Row>
                    <Grid.Row>
                        <Grid.Column width={4}>
                            <Form.Field>
                                <label>№ паллет</label>
                            </Form.Field>
                        </Grid.Column>
                        <Grid.Column width={2}>
                            <Form.Field>
                                <label>№ заказа</label>
                            </Form.Field>
                        </Grid.Column>
                        <Grid.Column width={2}>
                            <Form.Field>
                                <label>№ накладной</label>
                            </Form.Field>
                        </Grid.Column>
                        <Grid.Column width={2}>
                            <Form.Field>
                                <label>Тип</label>
                            </Form.Field>
                        </Grid.Column>
                        <Grid.Column width={2}>
                            <Form.Field>
                                <label>{`Вес${!form.distributeDataByOrders ? ' *' : ''}`}</label>
                            </Form.Field>
                        </Grid.Column>
                        <Grid.Column width={2}>
                            <Form.Field>
                                <label>{`Стоимость${
                                    !form.distributeDataByOrders ? ' *' : ''
                                }`}</label>
                            </Form.Field>
                        </Grid.Column>
                        <Grid.Column width={2}></Grid.Column>
                    </Grid.Row>
                </Grid>
            </Form>
            <Field
                name="orders"
                type={CONSTRUCTOR_TYPE}
                component={<OrdersConstructor />}
                columnsConfig={columnsConfig}
                defaultItems={defaultOrdersItem}
                settings={settings}
                form={form}
                error={error}
                onChange={onChange}
            />
        </>
    );
};

export default CreateShipping;
