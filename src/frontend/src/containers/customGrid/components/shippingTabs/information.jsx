import React from 'react';
import { useTranslation } from 'react-i18next';
import { Form, Grid, Segment } from 'semantic-ui-react';
import FormField from '../formField';
import { NUMBER_TYPE, SELECT_TYPE } from '../../../../constants/columnTypes';
import {useSelector} from "react-redux";
import {columnsTypesConfigSelector} from "../../../../ducks/representations";
import {ORDERS_GRID, SHIPPINGS_GRID} from "../../../../constants/grids";

const Information = ({ form = {}, onChange, settings, error }) => {
    const { t } = useTranslation();

    const columnsConfig = useSelector(state => columnsTypesConfigSelector(state, SHIPPINGS_GRID));

    return (
        <Form className="tabs-card">
            <Grid>
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
                            name="deliveryType"
                            isTranslate
                            columnsConfig={columnsConfig}
                            settings={settings}
                            form={form}
                            error={error}
                            onChange={onChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="tarifficationType"
                            isTranslate
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
                            name="vehicleTypeId"
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
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('temperature')}</label>
                            <div className="temperature-fields">
                                <label>{t('from')}</label>
                                <FormField
                                    noLabel
                                    name="temperatureMin"
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
                <Grid.Row>
                    <Grid.Column>
                        <Form.Field>
                            <label>{t('weigth')}</label>
                            <Segment className="mini-column">
                                <Grid>
                                    <Grid.Row columns={3}>
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
            </Grid>
        </Form>
    );
};

export default Information;
