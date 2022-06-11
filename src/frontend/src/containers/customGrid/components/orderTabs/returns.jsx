import React from 'react';
import { useTranslation } from 'react-i18next';
import { Form, Grid } from 'semantic-ui-react';
import FormField from '../../../../components/BaseComponents';
import { BIG_TEXT_TYPE, DATE_TYPE, TEXT_TYPE } from '../../../../constants/columnTypes';

const Returns = ({ form, onChange, settings, error }) => {
    const { t } = useTranslation();

    return (
        <Form className="tabs-card">
            <Grid>
                <Grid.Row columns={2} stretched>
                    <Grid.Column width={5}>
                        <FormField
                            name="returnShippingAccountNo"
                            value={form['returnShippingAccountNo']}
                            type={TEXT_TYPE}
                            settings={settings['returnShippingAccountNo']}
                            error={error['returnShippingAccountNo']}
                            onChange={onChange}
                        />
                        <FormField
                            name="plannedReturnDate"
                            value={form['plannedReturnDate']}
                            error={error['plannedReturnDate']}
                            type={DATE_TYPE}
                            settings={settings['plannedReturnDate']}
                            onChange={onChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="returnInformation"
                            value={form['returnInformation']}
                            error={error['returnInformation']}
                            rows={5}
                            type={BIG_TEXT_TYPE}
                            settings={settings['returnInformation']}
                            onChange={onChange}
                        />
                    </Grid.Column>
                </Grid.Row>
                <Grid.Row columns={2}>
                    <Grid.Column width={5}>
                        <FormField
                            name="actualReturnDate"
                            value={form['actualReturnDate']}
                            error={error['actualReturnDate']}
                            type={DATE_TYPE}
                            settings={settings['actualReturnDate']}
                            onChange={onChange}
                        />
                    </Grid.Column>
                    <Grid.Column>
                        <FormField
                            name="majorAdoptionNumber"
                            value={form['majorAdoptionNumber']}
                            error={error['majorAdoptionNumber']}
                            type={TEXT_TYPE}
                            settings={settings['majorAdoptionNumber']}
                            onChange={onChange}
                        />
                    </Grid.Column>
                </Grid.Row>
            </Grid>
        </Form>
    );
};

export default Returns;
