import React from 'react';
import {Form, Grid, Segment} from 'semantic-ui-react';
import {useTranslation} from 'react-i18next';
import FormField from '../formField';
import {SHIPPINGS_GRID} from "../../../../constants/grids";
import {useSelector} from "react-redux";
import {columnsTypesConfigSelector} from "../../../../ducks/representations";

const Accounts = ({ form = {}, onChange, settings, error }) => {
    const { t } = useTranslation();

    const columnsConfig = useSelector(state => columnsTypesConfigSelector(state, SHIPPINGS_GRID));

    return (
        <Form className="tabs-card">
            <Grid>
                <Grid.Row columns={2} stretched>
                    <Grid.Column>
                        <FormField
                            name="deliveryCostWithoutVAT"
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                        <FormField
                            name="additionalCostsWithoutVAT"
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                        <FormField
                            name="returnCostWithoutVAT"
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                        {/* <Text
                            name="invoiceNumber"
                            value={form['invoiceNumber']}
                            onChange={onChange}
                        />
                        <Text
                            name="invoiceAmountWithoutVAT"
                            value={form['invoiceAmountWithoutVAT']}
                            onChange={onChange}
                        />*/}
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="additionalCostsComments"
                            rows={10}
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={1}>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('reconciliation of expenses')}</label>
                            <Segment style={{ height: 'calc(100% - 22px)' }}>
                                <Grid>
                                    <Grid.Row columns={2}>
                                        <Grid.Column>
                                            <FormField
                                                checked={form['costsConfirmedByCarrier']}
                                                name="costsConfirmedByCarrier"
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                        <Grid.Column>
                                            <FormField
                                                checked={form['costsConfirmedByShipper']}
                                                columnsConfig={columnsConfig}
                                                settings={settings}
                                                form={form}
                                                error={error}
                                                name="costsConfirmedByShipper"
                                                onChange={onChange}
                                            />
                                        </Grid.Column>
                                    </Grid.Row>
                                </Grid>
                            </Segment>
                        </Form.Field>
                    </Grid.Column>
                </Grid.Row>
            </Grid>
        </Form>
    );
};

export default Accounts;
