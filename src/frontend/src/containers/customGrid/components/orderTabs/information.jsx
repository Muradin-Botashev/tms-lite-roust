import React, { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Form, Grid, Segment } from 'semantic-ui-react';
import { useSelector } from 'react-redux';
import FormField from '../formField';
import { columnsTypesConfigSelector } from '../../../../ducks/representations';
import { ORDERS_GRID } from '../../../../constants/grids';

const Information = ({
    form,
    onChange,
    uniquenessNumberCheck,
    settings,
    error,
}) => {
    const { t } = useTranslation();

    const columnsConfig = useSelector((state) => columnsTypesConfigSelector(state, ORDERS_GRID));

    const handleChangeShippingWarehouseId = useCallback((e, { name, value }) => {
        onChange(e, {
            name,
            value,
        });
        onChange(e, { name: 'shippingAddress', value: value && value.address ? value.address : '' });
    }, []);

    const handleChangeDeliveryWarehouseId = useCallback((e, { name, value }) => {

        onChange(e, {
            name,
            value,
        });
        onChange(e, { name: 'clientName', value: value && value.warehouseName ? value.warehouseName : '' });
        onChange(e, { name: 'deliveryAddress', value: value && value.address ? value.address : '' });
        onChange(e, { name: 'deliveryCity', value: value && value.city ? value.city : '' });
        onChange(e, { name: 'deliveryRegion', value: value && value.region ? value.region : '' });
    }, []);

    return (
        <Form className="tabs-card">
            <Grid>
                <Grid.Row>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('general info')}</label>
                            <Segment>
                                <Grid>
                                    <Grid.Row columns={form.id ? 4 : 3}>
                                        {
                                            !form.id &&  <Grid.Column>
                                                <FormField
                                                    name="orderNumber"
                                                    columnsConfig={columnsConfig}
                                                    settings={settings}
                                                    isRequired
                                                    form={form}
                                                    error={error}
                                                    onBlur={uniquenessNumberCheck}
                                                    onChange={onChange}
                                                />
                                            </Grid.Column>
                                        }
                                        <Grid.Column>
                                            <FormField
                                                name="clientOrderNumber"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column>
                                            <FormField
                                                name="payer"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        {
                                            form.id && <Grid.Column>
                                                <FormField
                                                    name="pickingTypeId"
                                                    columnsConfig={columnsConfig}
                                                    settings={settings}
                                                    form={form}
                                                    error={error}
                                                    onChange={onChange}
                                                />
                                            </Grid.Column>
                                        }
                                        {
                                            form.id && <Grid.Column>
                                                <FormField
                                                    name="orderType"
                                                    isDisabled
                                                    isTranslate
                                                    columnsConfig={columnsConfig}
                                                    settings={settings}
                                                    form={form}
                                                    error={error}
                                                    onChange={onChange}
                                                />
                                            </Grid.Column>
                                        }
                                    </Grid.Row>
                                    <Grid.Row columns={4}>
                                        <Grid.Column>
                                            <FormField
                                                name="orderDate"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
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
                                        <Grid.Column>
                                            <Form.Field>
                                                <label>{t('temperature')}</label>
                                                <div className="temperature-fields">
                                                    <label>{t('from')}</label>
                                                    <FormField
                                                        noLabel
                                                        name="temperatureMin"
                                                        value={form['temperatureMin']}
                                                        columnsConfig={columnsConfig}
                                                        settings={settings}
                                                        form={form}
                                                        error={error}
                                                        onChange={onChange}
                                                    />
                                                    <label>{t('to')}</label>
                                                    <FormField
                                                        noLabel
                                                        name="temperatureMax"
                                                        columnsConfig={columnsConfig}
                                                        settings={settings}
                                                        form={form}
                                                        error={error}
                                                        onChange={onChange}
                                                    />
                                                </div>
                                            </Form.Field>
                                        </Grid.Column>
                                    </Grid.Row>
                                    {
                                        form.id && <Grid.Row columns={4}>
                                            <Grid.Column>
                                                <FormField
                                                    name="deliveryCity"
                                                    isRequired
                                                    columnsConfig={columnsConfig}
                                                    settings={settings}
                                                    form={form}
                                                    error={error}
                                                    onChange={onChange}
                                                />
                                            </Grid.Column>
                                            <Grid.Column>
                                                <FormField
                                                    name="deliveryRegion"
                                                    isRequired
                                                    columnsConfig={columnsConfig}
                                                    settings={settings}
                                                    form={form}
                                                    error={error}
                                                    onChange={onChange}
                                                />
                                            </Grid.Column>
                                        </Grid.Row>
                                    }
                                </Grid>
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('route')}</label>
                            <Segment>
                                <Grid>
                                    <Grid.Row columns={2}>
                                        <Grid.Column>
                                            <FormField
                                                name="shippingAddress"
                                                rows={2}
                                                isRequired
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
                                                rows={2}
                                                isRequired
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                    </Grid.Row>
                                    <Grid.Row columns={3}>
                                        <Grid.Column className="mini-column">
                                            <FormField
                                                name="shippingDate"
                                                isRequired
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                maxDate={form.deliveryDate}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column className="mini-column">
                                            <FormField
                                                name="deliveryDate"
                                                isRequired
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                minDate={form.shippingDate}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column>
                                            <FormField
                                                name="TransportZone"
                                                text="transportZone"
                                                rows={2}
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                    </Grid.Row>
                                </Grid>
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('palletsCountGroup')}</label>
                            <Segment className="mini-column">
                                <Grid>
                                    <Grid.Row columns={3}>
                                        <Grid.Column>
                                            <FormField
                                                name="palletsCount"
                                                isRequired
                                                text="prepare"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column>
                                            <FormField
                                                name="actualPalletsCount"
                                                text="plan"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column>
                                            <FormField
                                                name="confirmedPalletsCount"
                                                text="fact"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                    </Grid.Row>
                                </Grid>
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={2}>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('boxesCountGroup')}</label>
                            <Segment className="mini-column">
                                <Grid>
                                    <Grid.Row columns={2}>
                                        <Grid.Column>
                                            <FormField
                                                name="boxesCount"
                                                text="prepare"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column>
                                            <FormField
                                                name="confirmedBoxesCount"
                                                text="fact"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                    </Grid.Row>
                                </Grid>
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('weigth')}</label>
                            <Segment
                                style={{ height: 'calc(100% - 22px)' }}
                                className="mini-column"
                            >
                                <Grid>
                                    <Grid.Row columns={2}>
                                        <Grid.Column>
                                            <FormField
                                                name="weightKg"
                                                text="planWeigth"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column>
                                            <FormField
                                                name="actualWeightKg"
                                                text="factWeigth"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                    </Grid.Row>
                                </Grid>
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row>
                    <Grid.Column>
                        <Form.Field>
                            <Grid>
                                <Grid.Row columns={3}>
                                    <Grid.Column>
                                        <FormField
                                            name="BottlesCount"
                                            text="bottlesCount"
                                            columnsConfig={columnsConfig}
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
                                            settings={settings}
                                            form={form}
                                            error={error}
                                            onChange={onChange}
                                        />
                                    </Grid.Column>
                                    <Grid.Column>
                                        <FormField
                                            name="PaymentCondition"
                                            text="paymentCondition"
                                            columnsConfig={columnsConfig}
                                            settings={settings}
                                            form={form}
                                            error={error}
                                            onChange={onChange}
                                        />
                                    </Grid.Column>
                                </Grid.Row>
                            </Grid>
                        </Form.Field>
                    </Grid.Column>
                </Grid.Row>
            </Grid>
        </Form>
    );
};

export default Information;
