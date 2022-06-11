import React, {useState} from 'react';
import {Button, Form, Grid} from "semantic-ui-react";
import Date from "../../components/BaseComponents/Date";
import {useTranslation} from "react-i18next";
import {useDispatch, useSelector} from "react-redux";
import {exportProgressSelector, reportExportToExcelRequest} from "../../ducks/reports";
import {OPERATIONAL_REPORT_TYPE, REGISTRY_REPORT_TYPE} from "../../constants/reportType";
import {dateToString} from "../../utils/dateTimeFormater";

const RegistryReport = () => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    let [params, setParams] = useState({
        startDate: dateToString(),
        endDate: dateToString(),
    });

    const exportProgress = useSelector(exportProgressSelector);

    const handleChangeParams = (e, { name, value }) => {
        const newParams = {
            ...params,
            [name]: value,
        };
        setParams(newParams);
    };

    const generateReport = () => {
        dispatch(reportExportToExcelRequest({
            type: REGISTRY_REPORT_TYPE,
            params,
        }));
    };

    return (
        <div className="container">
            <div className="report">
                <div className="report_params">
                    <Form>
                        <Grid>
                            <Grid.Row columns={3} verticalAlign="bottom">
                                <Grid.Column>
                                    <Date
                                        name="startDate"
                                        value={params.startDate}
                                        maxDate={params.endDate}
                                        onChange={handleChangeParams}
                                    />
                                </Grid.Column>
                                <Grid.Column>
                                    <Date
                                        name="endDate"
                                        value={params.endDate}
                                        minDate={params.startDate}
                                        onChange={handleChangeParams}
                                    />
                                </Grid.Column>
                                <Grid.Column textAlign="right">
                                    <Button primary loading={exportProgress} onClick={generateReport}>
                                        {t('exportToExcel')}
                                    </Button>
                                </Grid.Column>
                            </Grid.Row>
                        </Grid>
                    </Form>
                </div>
            </div>
        </div>
    )
};

export default RegistryReport;
