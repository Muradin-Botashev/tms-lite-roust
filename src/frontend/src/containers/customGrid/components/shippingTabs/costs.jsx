import React, {useCallback} from 'react';
import { Form, Grid, Segment } from 'semantic-ui-react';
import { useTranslation } from 'react-i18next';
import FormField from '../formField';
import { SHIPPINGS_GRID } from '../../../../constants/grids';
import { useSelector } from 'react-redux';
import { columnsTypesConfigSelector } from '../../../../ducks/representations';

const fields = [
    'basicDeliveryCostWithoutVAT',
    'downtimeRate',
    'orderCosts',
    'otherCosts',
];

const Costs = ({ form = {}, onChange, settings, error }) => {
    const { t } = useTranslation();

    const columnsConfig = useSelector(state => columnsTypesConfigSelector(state, SHIPPINGS_GRID));

    const handleChange = useCallback((e, { name, value }) => {
        onChange(e, { name, value });

        let totalDeliveryCostWithoutVAT = 0;

        if (name !== 'orderCosts') {
            totalDeliveryCostWithoutVAT = value ? parseFloat(value) : 0;
        } else if (value && value.length) {
            value.forEach(item => {
                totalDeliveryCostWithoutVAT = totalDeliveryCostWithoutVAT + (item.returnCostWithoutVAT ? parseFloat(item.returnCostWithoutVAT) : 0);
            })
        }

        fields.filter(field => field !== name).forEach(field => {
            if (field !== 'orderCosts') {
                totalDeliveryCostWithoutVAT =
                    totalDeliveryCostWithoutVAT + (form[field] ? parseFloat(form[field]) : 0);
            } else if (form[field] && form[field].length) {
                form[field].forEach(item => {
                    totalDeliveryCostWithoutVAT = totalDeliveryCostWithoutVAT + (item.returnCostWithoutVAT ? parseFloat(item.returnCostWithoutVAT) : 0);
                })
            }

        });

        onChange(e, {
            name: 'totalDeliveryCostWithoutVAT',
            value: totalDeliveryCostWithoutVAT,
        });

        onChange(e, {
            name: 'totalDeliveryCost',
            value: (parseFloat(totalDeliveryCostWithoutVAT) * 120) / 100,
        });

    }, [form]);

    return (
        <Form className="tabs-card">
            <Grid>
                <Grid.Row columns={2}>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('Стоимость перевозки')}</label>
                            <Segment>
                                <FormField
                                    name="basicDeliveryCostWithoutVAT"
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    onChange={onChange}
                                />
                                <FormField
                                    name="totalDeliveryCostWithoutVAT"
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    onChange={onChange}
                                />
                                <FormField
                                    name="totalDeliveryCost"
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    onChange={onChange}
                                />
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('Дополнительные расходы')}</label>
                            <Segment>
                                <FormField
                                    name="downtimeRate"
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    onChange={handleChange}
                                />
                                <FormField
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    name="returnCostWithoutVAT"
                                    onChange={handleChange}
                                />
                                <FormField
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    name="otherCosts"
                                    onChange={handleChange}
                                />
                                <FormField
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    name="costsComments"
                                    onChange={handleChange}
                                />
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={1}>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('returnCostWithoutVAT')}</label>
                            <Segment>
                                <FormField
                                    columnsConfig={columnsConfig}
                                    settings={settings}
                                    form={form}
                                    error={error}
                                    orders={form.orders}
                                    name="orderCosts"
                                    onChange={handleChange}
                                />
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                            {/*<Grid.Column>
                        <FormField
                            name="basicDeliveryCostWithoutVAT"
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="totalDeliveryCostWithoutVAT"
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="totalDeliveryCost"
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
                            name="downtimeRate"
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={handleChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            name="returnCostWithoutVAT"
                            onChange={handleChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            name="otherCosts"
                            onChange={handleChange}
                        />
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row>
                    <Grid.Column>
                        <FormField
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            orders={form.orders}
                            name="orderCosts"
                            onChange={handleChange}
                        />
                    </Grid.Column>*/}
                </Grid.Row>
            </Grid>
        </Form>
    );
};

export default Costs;
