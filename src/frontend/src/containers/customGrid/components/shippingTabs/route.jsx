import React, { useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { useSelector } from 'react-redux';
import { Form, Grid } from 'semantic-ui-react';
import FormField from '../../../../components/BaseComponents';
import {
    BIG_TEXT_TYPE,
    DATE_TIME_TYPE,
    DATE_TYPE,
    NUMBER_TYPE,
    STATE_TYPE,
} from '../../../../constants/columnTypes';
import { settingsExtSelector } from '../../../../ducks/gridCard';

const Route = ({
    name,
    form = {},
    point = {},
    onChange,
    pointChange,
    index,
    settings: baseSettings,
}) => {
    const { t } = useTranslation();

    const settings = useSelector(state => settingsExtSelector(state, form.status));

    const handleChange = useCallback(
        (e, { name, value }) => {
            pointChange(
                {
                    ...point,
                    [name]: value,
                },
                index,
            );
        },
        [point],
    );

    return (
        <Form style={{ paddingLeft: '12px' }}>
            <Grid>
                <Grid.Row columns={2}>
                    <Grid.Column>
                        <FormField
                            name="plannedDate"
                            text={point.isLoading ? 'plannedDate_loading' : 'plannedDate_delivery'}
                            value={point['plannedDate']}
                            type={DATE_TIME_TYPE}
                            settings={settings['plannedDate']}
                            onChange={handleChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="vehicleStatus"
                            value={point['vehicleStatus']}
                            source="vehicleState"
                            type={STATE_TYPE}
                            settings={settings['vehicleStatus']}
                            onChange={handleChange}
                        />
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={2}>
                    <Grid.Column>
                        <FormField
                            name="arrivalTime"
                            value={point['arrivalTime']}
                            settings={settings['arrivalTime']}
                            type={DATE_TIME_TYPE}
                            onChange={handleChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="departureTime"
                            value={point['departureTime']}
                            settings={settings['departureTime']}
                            type={DATE_TIME_TYPE}
                            onChange={handleChange}
                        />
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={1}>
                    <Grid.Column width={16}>
                        <FormField
                            name="address"
                            value={point['address']}
                            settings={settings['address']}
                            type={BIG_TEXT_TYPE}
                            onChange={handleChange}
                        />
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={1}>
                    <Grid.Column width={18}>
                        <FormField
                            name="deviationReasonsComments"
                            type={BIG_TEXT_TYPE}
                            value={form['deviationReasonsComments']}
                            settings={baseSettings['deviationReasonsComments']}
                            onChange={onChange}
                        />
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={1}>
                    <Grid.Column width={8}>
                        <FormField
                            name="trucksDowntime"
                            value={point['trucksDowntime']}
                            settings={settings['trucksDowntime']}
                            type={NUMBER_TYPE}
                            onChange={handleChange}
                        />
                    </Grid.Column>
                </Grid.Row>
            </Grid>
        </Form>
    );
};

export default Route;
